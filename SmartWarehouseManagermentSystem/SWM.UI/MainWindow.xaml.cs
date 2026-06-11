using SWM.BL;
using SWM.Common;
using SWM.UI.Config;
using SWM.UI.Services;
using SWM.UI.Themes;
using SWM.UI.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SWM.UI
{
    /// <summary>
    /// Màn hình chính: map kho + AGV, điều phối qua các service (PLC, serial, lệnh vận chuyển).
    /// Luồng: Serial/HMI/Manual → TransportCommand → PLC → Monitor cập nhật UI.
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- Services ---
        private readonly PlcService _plcService;
        private readonly TransportCommandService _transportService;
        private readonly SerialCommunicationService _serialService;
        private readonly PlcMonitorService _plcMonitor;
        private readonly ConnectionStatusService _connectionStatus = new ConnectionStatusService();
        private readonly DispatcherTimer _conveyorTimer;

        // --- Map / layout UI ---
        private readonly List<BFLayout> _buffers = new List<BFLayout>();
        private readonly List<Node> _nodes = new List<Node>();
        private readonly List<Link> _links = new List<Link>();
        private readonly List<AGV> _agvs = new List<AGV>();
        private readonly uc_Buffer[] _bufferControls = new uc_Buffer[10000];
        private readonly uc_Tag[] _tagControls = new uc_Tag[10000];
        private readonly AGV_Slim[] _agvControls = new AGV_Slim[10000];
        private DataTable _mapRoutes;

        private string _oldAgvLocation = "5";
        private int _agvX;
        private string _deleteJobId;
        private string _jobState;
        private DateTime _deleteJobCreateTime;

        public MainWindow()
        {
            InitializeComponent();

            _plcService = new PlcService(AppConfiguration.Current.Plc.IpAddress);
            _transportService = new TransportCommandService(_plcService);
            _serialService = new SerialCommunicationService();
            _plcMonitor = new PlcMonitorService(_plcService, _transportService);
            _conveyorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _conveyorTimer.Tick += ConveyorTimer_Tick;

            WireServiceEvents();

            if (_plcService.Connect())
                _plcMonitor.InitializeAgvState("2", "EMPTY");

            _serialService.Connect();

            Load_Layout();
            Load_Map();
            LoadInitialAgv();
            Update_AGV("2");

            StartTimers();
            UpdateConnectionPanel();
        }

        // Nối event service → cập nhật UI trên UI thread
        private void WireServiceEvents()
        {
            _transportService.CommandsChanged += () => Dispatcher.Invoke(LoadTransportCommand);
            _transportService.LayoutChanged += () => Dispatcher.Invoke(Load_Layout);
                      
            // C1x + băng tải IP01 có hàng → tạo lệnh nhập kho
            _serialService.ImportRequested += () => Dispatcher.Invoke(OnSerialImportRequested);
            _serialService.ErrorOccurred += message => Dispatcher.Invoke(() =>
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error));

            _plcMonitor.AgvLocationChanged += location => Dispatcher.Invoke(() => Update_AGV(location));
            _plcMonitor.LayoutRefreshRequested += () => Dispatcher.Invoke(Load_Layout);
        }

        // Timer: xử lý hàng đợi lệnh | nhịp sống PLC | poll trạng thái | ping mạng
        private void StartTimers()
        {
            var commandTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            commandTimer.Tick += (s, e) => _transportService.ProcessPendingCommands();
            commandTimer.Start();

            var plcAliveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            plcAliveTimer.Tick += (s, e) => _plcService.SendAlivePulse();
            plcAliveTimer.Start();

            var monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            monitorTimer.Tick += (s, e) =>
            {
                _plcMonitor.Poll();
                UpdateConnectionPanel();
            };
            monitorTimer.Start();

            var pingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            pingTimer.Tick += (s, e) => _plcService.RefreshNetworkStatus();
            pingTimer.Start();
        }

        // Cập nhật panel trạng thái kết nối bên phải dashboard
        private void UpdateConnectionPanel()
        {
            ConnectionStatusInfo info = _connectionStatus.Collect(_plcService, _serialService, _transportService);

            lblConnUpdated.Text = "Cập nhật: " + info.LastUpdated.ToString("HH:mm:ss");

            SetStatusIndicator(indPlcConn, lblPlcConn, info.PlcConnected);
            SetStatusIndicator(indPlcPing, lblPlcPing, info.PlcNetworkOnline);
            lblPlcAddress.Text = info.PlcIp + "  |  Station " + info.PlcStation;

            SetStatusIndicator(indSerial, lblSerialStatus, info.SerialConnected, "OPEN", "CLOSED");
            lblSerialConfig.Text = info.SerialPort + "  |  " + info.SerialBaudRate + " bps";

            SetStatusIndicator(indDatabase, lblDatabaseStatus, info.DatabaseConnected);

            lblAgvInfo.Text = info.AgvId + "  |  Node " + info.AgvLocation + "  |  " + info.AgvLoadState;
            lblIp01State.Text = info.InputPortState;
            lblOp01State.Text = info.OutputPortState;
            lblIp01State.Foreground = info.InputPortState == "FULL" ? ThemeColors.BufferFull : ThemeColors.BufferEmptyText;
            lblOp01State.Foreground = info.OutputPortState == "FULL" ? ThemeColors.BufferFull : ThemeColors.BufferEmptyText;

            lblJobStatus.Text = info.CurrentCommandStatus;
            lblJobRoute.Text = info.CurrentCommandRoute;
            lblJobId.Text = info.CurrentCommandId;
        }

        private void SetStatusIndicator(System.Windows.Shapes.Ellipse indicator, TextBlock label, bool isOnline, string onlineText = "ONLINE", string offlineText = "OFFLINE")
        {
            var brush = isOnline ? (Brush)FindResource("StatusOnlineBrush") : (Brush)FindResource("StatusOfflineBrush");
            indicator.Fill = brush;
            label.Text = isOnline ? onlineText : offlineText;
            label.Foreground = brush;
        }

        // C1x từ serial: chỉ tạo lệnh khi IP01 (M2300) đang có hàng và còn ô BF trống
        private void OnSerialImportRequested()
        {
            switch (_transportService.CreateImportCommand())
            {
                case ImportCommandResult.Success:
                    LoadTransportCommand();
                    break;
                case ImportCommandResult.InputPortEmpty:
                    MessageBox.Show("Không tạo lệnh nhập: băng tải IP01 chưa có hàng.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case ImportCommandResult.NoEmptySlot:
                    MessageBox.Show("Không tạo lệnh nhập: kho đã đầy, không còn ô trống.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                default:
                    MessageBox.Show("Không tạo được lệnh nhập. Vui lòng thử lại.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void ConveyorTimer_Tick(object sender, EventArgs e)
        {
            //_plcService.StopConveyor();
            //_conveyorTimer.Stop();
        }

        private void LoadInitialAgv()
        {
            try
            {
                const string query = @"SELECT A.ID AS AGVID, A.BAYID AS BAYID,A.FULLSTATE AS STATE,A.CURRENTNODEID AS CURRENTTAG,PROCESSINGSTATE AS STATUS,
                                    B.XPOS AS X_POS, YPOS AS Y_POS, A.ALARMSTATE AS ALARM,A.BATTERYVOLTAGE AS BATERRY,
                                    A.RUNSTATE AS RUNSTATE, A.CONNECTIONSTATE AS CONNECTIONSTATE
                                    FROM NA_R_VEHICLE A,NA_R_NODE B
                                    WHERE a.CURRENTNODEID =b.ID";
                LoadAgvFromTable(BLLayout.ReadAGVCurrentParam(query));
            }
            catch (Exception)
            {
            }
        }

        private void LoadAgvFromTable(DataTable agvTable)
        {
            for (int i = 0; i < agvTable.Rows.Count; i++)
            {
                _agvs.Add(new AGV
                {
                    ID = agvTable.Rows[i]["AGVID"].ToString(),
                    BAYID = agvTable.Rows[i]["BAYID"].ToString(),
                    STATE = agvTable.Rows[i]["STATE"].ToString(),
                    STATUS = agvTable.Rows[i]["STATUS"].ToString(),
                    BATTERY = Convert.ToInt32(agvTable.Rows[i]["BATERRY"]),
                    NODE = agvTable.Rows[i]["CURRENTTAG"].ToString(),
                    X = Convert.ToInt32(agvTable.Rows[i]["X_POS"]),
                    Y = Convert.ToInt32(agvTable.Rows[i]["Y_POS"]),
                    ALARM = agvTable.Rows[i]["ALARM"].ToString(),
                    CONNECTSTATE = agvTable.Rows[i]["CONNECTIONSTATE"].ToString(),
                    RUNSTATE = agvTable.Rows[i]["RUNSTATE"].ToString()
                });
            }

            Dispatcher.Invoke(AddAgvToMap);
        }

        private void AddAgvToMap()
        {
            try
            {
                foreach (AGV agv in _agvs)
                {
                    if (!int.TryParse(agv.ID, out int agvId) || agvId >= 10000)
                        continue;

                    _agvControls[agvId] = new AGV_Slim
                    {
                        Height = 40,
                        Width = 30,
                        color_Baterry = ThemeColors.BatteryDefault,
                        AGV_Name = agvId.ToString()
                    };

                    ApplyAgvVisualState(_agvControls[agvId], agv);
                    Canvas.SetLeft(_agvControls[agvId], agv.X - 15);
                    Canvas.SetTop(_agvControls[agvId], agv.Y - 20);
                    cvs_Map.Children.Add(_agvControls[agvId]);
                }
            }
            catch (Exception)
            {
            }
        }

        private void Update_AGV(string location)
        {
            try
            {
                DataTable dtAgv = BLLayout.Load_AGV();
                _agvs.Clear();

                for (int i = 0; i < dtAgv.Rows.Count; i++)
                {
                    AGV agv = new AGV
                    {
                        ID = dtAgv.Rows[i]["ID"].ToString(),
                        BAYID = dtAgv.Rows[i]["BAYID"].ToString(),
                        STATE = dtAgv.Rows[i]["FULLSTATE"].ToString(),
                        STATUS = dtAgv.Rows[i]["PROCESSINGSTATE"].ToString(),
                        BATTERY = Convert.ToInt32(dtAgv.Rows[i]["BATTERYVOLTAGE"]),
                        NODE = dtAgv.Rows[i]["CURRENTNODEID"].ToString(),
                        X = Convert.ToInt32(dtAgv.Rows[i]["X_POS"]),
                        Y = Convert.ToInt32(dtAgv.Rows[i]["Y_POS"]),
                        ALARM = dtAgv.Rows[i]["ALARMSTATE"].ToString(),
                        CONNECTSTATE = dtAgv.Rows[i]["CONNECTIONSTATE"].ToString(),
                        RUNSTATE = dtAgv.Rows[i]["RUNSTATE"].ToString(),
                        COMMAND = dtAgv.Rows[i]["TRANSPORTCOMMANDID"].ToString()
                    };

                    for (int j = 0; j < _mapRoutes.Rows.Count; j++)
                    {
                        if (agv.NODE == _mapRoutes.Rows[j]["FRTAG"].ToString())
                        {
                            agv.NEXTNODE = _mapRoutes.Rows[j]["TONODE"].ToString();
                            agv.NEXT_X = Convert.ToInt32(_mapRoutes.Rows[j]["TO_X"]);
                            agv.NEXT_Y = Convert.ToInt32(_mapRoutes.Rows[j]["TO_Y"]);
                        }
                    }

                    _agvs.Add(agv);
                }

                Dispatcher.Invoke(() => RefreshAgvOnMap(location));
            }
            catch (Exception ex)
            {
                AlarmLog.LogAlarmToDatabase("08");
                MessageBox.Show(ex.ToString());
            }
        }

        private void RefreshAgvOnMap(string location)
        {
            double agvCount = _agvs.Count;
            double agvFull = 0;
            double agvEmpty = 0;
            double agvConnect = 0;
            double agvRun = 0;

            try
            {
                foreach (AGV agv in _agvs)
                {
                    if (!int.TryParse(agv.ID, out int agvId))
                        continue;

                    _agvControls[agvId].color_Baterry = ThemeColors.BatteryGood;

                    ApplyAgvVisualState(_agvControls[agvId], agv);

                    if (agv.STATE == "FULL")
                        agvFull++;
                    else if (agv.STATE == "EMPTY")
                        agvEmpty++;

                    if (agv.CONNECTSTATE == "CONNECT")
                        agvConnect++;

                    if (agv.RUNSTATE == "RUN")
                        agvRun++;

                    if (int.Parse(location) > int.Parse(_oldAgvLocation))
                        _agvControls[agvId].Direction_AGV = -90;
                    else if (int.Parse(location) < int.Parse(_oldAgvLocation))
                        _agvControls[agvId].Direction_AGV = 90;

                    TranslateTransform transform = new TranslateTransform();
                    _agvControls[agvId].RenderTransform = transform;
                    transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(_agvX, agv.X - 515, TimeSpan.FromSeconds(2)));
                    transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(3, 3, TimeSpan.FromSeconds(2)));
                    _agvX = agv.X - 168;

                    _agvControls[agvId].ToolTip = string.Format("Vehicle ID: {0}\nBAY ID: {1}\nStatus: {2}\nTrCmdId: {3}", agv.ID, agv.BAYID, agv.STATUS, agv.COMMAND);
                    ToolTipService.SetShowDuration(_agvControls[agvId], 2000000);
                }

                _oldAgvLocation = location;

                AGV_Connect.TotalValue = agvCount;
                AGV_FullState.TotalValue = agvCount;
                AGV_RunState.TotalValue = agvCount;

                AGV_FullState.FullValue = agvFull;
                AGV_FullState.EmptyValue = agvCount - agvFull;
                AGV_FullState.AngleValue = agvEmpty / agvCount * 360;

                AGV_Connect.FullValue = agvConnect;
                AGV_Connect.EmptyValue = agvCount - agvConnect;
                AGV_Connect.AngleValue = AGV_Connect.EmptyValue / agvCount * 360;

                AGV_RunState.FullValue = agvRun;
                AGV_RunState.EmptyValue = agvCount - agvRun;
                AGV_RunState.AngleValue = AGV_RunState.EmptyValue / agvCount * 360;
            }
            catch (Exception)
            {
            }
        }

        private static void ApplyAgvVisualState(AGV_Slim control, AGV agv)
        {
            if (agv.ALARM == "NOALARM")
            {
                control.colorbackgroud = ThemeColors.AgvRun;
            }
            else
            {
                control.colorbackgroud = ThemeColors.AgvAlarm;
            }

            if (agv.STATE == "FULL")
                control.colorTray = ThemeColors.AgvTrayFull;
            else if (agv.STATE == "EMPTY")
            {
                control.colorTray = control.colorbackgroud;
                control.rtgSlottray.StrokeThickness = 0;
            }
        }

        private void Load_Map()
        {
            try
            {
                DataTable dtMap = BLLayout.LoadMapConfig();
                _mapRoutes = dtMap;

                for (int i = 0; i < dtMap.Rows.Count; i++)
                {
                    try
                    {
                        _nodes.Add(new Node
                        {
                            ID = dtMap.Rows[i]["FRTAG"].ToString(),
                            X = Convert.ToInt32(dtMap.Rows[i]["FROM_X"]),
                            Y = Convert.ToInt32(dtMap.Rows[i]["FROM_Y"])
                        });

                        string linkId = dtMap.Rows[i]["LINK_ID"].ToString();
                        _links.Add(new Link
                        {
                            ID = linkId,
                            Source = linkId.Substring(0, 4),
                            Dest = linkId.Substring(5, 4),
                            Distance = dtMap.Rows[i]["DIS"].ToString(),
                            StartX = Convert.ToInt32(dtMap.Rows[i]["FROM_X"]),
                            StartY = Convert.ToInt32(dtMap.Rows[i]["FROM_Y"]),
                            EndX = Convert.ToInt32(dtMap.Rows[i]["TO_X"]),
                            EndY = Convert.ToInt32(dtMap.Rows[i]["TO_Y"])
                        });
                    }
                    catch (Exception)
                    {
                        AlarmLog.LogAlarmToDatabase("09");
                    }
                }

                Draw_Map();
            }
            catch (Exception)
            {
            }
        }

        private void Draw_Map()
        {
            try
            {
                foreach (Link link in _links)
                {
                    Line line = new Line
                    {
                        StrokeThickness = 2,
                        Stroke = ThemeColors.MapLink,
                        ToolTip = link.ID,
                        X1 = link.StartX,
                        Y1 = link.StartY,
                        X2 = link.EndX,
                        Y2 = link.EndY
                    };
                    cvs_Map.Children.Add(line);
                }

                Draw_Node();
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        private void Draw_Node()
        {
            try
            {
                foreach (Node node in _nodes)
                {
                    if (!int.TryParse(node.ID, out int nodeId) || nodeId >= 10000)
                        continue;

                    _tagControls[nodeId] = new uc_Tag
                    {
                        Height = 6,
                        Width = 6,
                        colorbackgroud = ThemeColors.MapNode,
                        ToolTip = string.Format("Node: {0}", node.ID),
                        Visibility = Visibility.Visible
                    };

                    foreach (Link link in _links)
                    {
                        if (node.ID != link.Source)
                            continue;

                        if (link.StartX > link.EndX && link.StartY == link.EndY)
                            _tagControls[nodeId].rotation_tag.Angle = 0;
                        else if (link.StartX < link.EndX && link.StartY == link.EndY)
                            _tagControls[nodeId].rotation_tag.Angle = 180;
                    }

                    ToolTipService.SetShowDuration(_tagControls[nodeId], 20000);
                    Canvas.SetLeft(_tagControls[nodeId], node.X - 3);
                    Canvas.SetTop(_tagControls[nodeId], node.Y - 3);
                    cvs_Map.Children.Add(_tagControls[nodeId]);
                }
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        private void Load_Layout()
        {
            try
            {
                int fullCount = 0;
                int emptyCount = 0;
                DataTable dtLayout = BLLayout.LoadLayoutConfig();
                _buffers.Clear();

                for (int i = 0; i < dtLayout.Rows.Count; i++)
                {
                    BFLayout buffer = new BFLayout
                    {
                        ID = dtLayout.Rows[i]["BFID"].ToString(),
                        X = Convert.ToInt32(dtLayout.Rows[i]["XPOS"]),
                        Y = Convert.ToInt32(dtLayout.Rows[i]["YPOS"]),
                        PORTNAME = dtLayout.Rows[i]["BFNAME"].ToString(),
                        FULLSTATE = dtLayout.Rows[i]["FULLSTATE"].ToString(),
                        TRAYID = dtLayout.Rows[i]["TRAYID"].ToString(),
                        PRODUCTID = dtLayout.Rows[i]["PRODUCTCODE"].ToString()
                    };

                    if (buffer.FULLSTATE == "FULL")
                        fullCount++;
                    else
                        emptyCount++;

                    // Thời gian tồn kho = Now - UPDATETIME (chỉ ô đang FULL)
                    buffer.AGINGTIME = buffer.FULLSTATE == "FULL"
                        ? InventoryAging.FormatFromUpdateTime(dtLayout.Rows[i]["UPDATETIME"].ToString())
                        : string.Empty;
                    _buffers.Add(buffer);
                }

                Dispatcher.Invoke(() =>
                {
                    Load_BFLayout();
                    Buffer_FullState.TotalValue = dtLayout.Rows.Count;
                    AGV_FullState.FullValue = fullCount;
                    AGV_FullState.EmptyValue = dtLayout.Rows.Count - emptyCount;
                    AGV_FullState.AngleValue = (double)emptyCount / dtLayout.Rows.Count * 360;
                });
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        private void Load_BFLayout()
        {
            try
            {
                foreach (BFLayout buffer in _buffers)
                {
                    if (!int.TryParse(buffer.ID, out int bufferId) || bufferId >= 10000)
                        continue;

                    _bufferControls[bufferId] = new uc_Buffer { FullState = buffer.FULLSTATE };
                    if (_bufferControls[bufferId].FullState == "FULL")
                    {
                        _bufferControls[bufferId].Material_ID = buffer.PRODUCTID;
                        _bufferControls[bufferId].Material_Code = buffer.TRAYID;
                        _bufferControls[bufferId].Aging_Time = buffer.AGINGTIME;
                        _bufferControls[bufferId].Background = ThemeColors.BufferFull;
                        _bufferControls[bufferId].rtgSlottray.Foreground = ThemeColors.BufferFullText;
                    }
                    else
                    {
                        _bufferControls[bufferId].Aging_Time = string.Empty;
                        _bufferControls[bufferId].Background = ThemeColors.BufferEmpty;
                        _bufferControls[bufferId].rtgSlottray.Foreground = ThemeColors.BufferEmptyText;
                    }

                    _bufferControls[bufferId].Port_Name = buffer.PORTNAME;
                    if (_bufferControls[bufferId].Port_Name.Substring(0, 12) == "B1STK01_CV01")
                    {
                        _bufferControls[bufferId].Height = 145;
                        _bufferControls[bufferId].Width = 240;
                    }
                    else
                    {
                        _bufferControls[bufferId].Height = 130;
                        _bufferControls[bufferId].Width = 240;
                    }

                    _bufferControls[bufferId].ToolTip = string.Format("Port Name: {0}\nTray ID: {1}\nProduct ID: {2}\nTrCmdId: {3}", buffer.PORTNAME, buffer.TRAYID, buffer.PRODUCTID, "");
                    ToolTipService.SetShowDuration(_bufferControls[bufferId], 2000000);
                    Canvas.SetLeft(_bufferControls[bufferId], buffer.X);
                    Canvas.SetTop(_bufferControls[bufferId], buffer.Y);
                    cvs_Map.Children.Add(_bufferControls[bufferId]);
                }
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPanel(Grid_Map);
            lblUser.Text = BLLogin.DisplayName;
            LoadTransportCommand();
        }

        private void ShowPanel(UIElement visiblePanel)
        {
            Grid_Map.Visibility = Visibility.Hidden;
            Grid_AGVDetail.Visibility = Visibility.Hidden;
            Grid_CommandHistory.Visibility = Visibility.Hidden;
            visiblePanel.Visibility = Visibility.Visible;
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(Grid_Map);
            lblUser.Text = BLLogin.DisplayName;
        }

        private void btnBufferDetail_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(Grid_AGVDetail);
            dtgBFData.ItemsSource = _transportService.LoadBufferDetail().DefaultView;
        }

        private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(Grid_CommandHistory);
            LoadTransportCommand();
        }

        private void LoadTransportCommand()
        {
            DataTable commands = _transportService.LoadCommandHistory();
            dtgCommandHistory.ItemsSource = commands.DefaultView;
            dtgCommandHistory.Columns[11].Visibility = Visibility.Hidden;
            dtgCommandHistory.Columns[12].Visibility = Visibility.Hidden;
            dtgCommandHistory.Columns[2].Header = "ID lệnh vận chuyển";

            DataTable jobCount = _transportService.LoadJobCount();
            lbl_LoadCount.Content = jobCount.Rows[0]["InputCommand"].ToString();
            lbl_UnloadCount.Content = jobCount.Rows[0]["OutputCommand"].ToString();
        }

        private void btnManualControl_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(Grid_CommandHistory);
            LoadTransportCommand();
            new ManualControlWindow().ShowDialog();
            LoadTransportCommand();
        }

        private void btnDeleteCommand_Click(object sender, RoutedEventArgs e)
        {
            if (_jobState == "JOB CREATE")
            {
                _transportService.DeleteJob(_deleteJobId, _deleteJobCreateTime, _jobState);
                LoadTransportCommand();
            }
            else
            {
                MessageBox.Show("Không thể xóa lệnh đang trong quá trình thực hiện!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnAddCommand_Click(object sender, RoutedEventArgs e)
        {
            new ManualControlWindow().ShowDialog();
            LoadTransportCommand();
        }

        private void dtgCommandHistory_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.HeaderStyle = new Style(typeof(DataGridColumnHeader));
            e.Column.HeaderStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            e.Column.HeaderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, Brushes.Transparent));
        }

        private void dtgCommandHistory_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (!(dtgCommandHistory.SelectedItem is DataRowView row))
                return;

            _deleteJobId = row["CommandID"].ToString();
            _deleteJobCreateTime = DateTime.Parse(row["JobCreat"].ToString());
            _jobState = row["CommandStatus"].ToString();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _serialService.Dispose();
            _plcService.Dispose();
        }
    }
}
