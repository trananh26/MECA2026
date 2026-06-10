using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;
using System.Net.Sockets;
using System.Net;
using System.IO.Ports;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using SWM.UI.View;
using System.Reflection;
using ActUtlTypeLib;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using SWM.Common;
using SWM.BL;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using SWM.DL;
using System.Runtime.Serialization.Formatters.Binary;

namespace SWM.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort SWMPort = new SerialPort();

        private List<BFLayout> lstBF = new List<BFLayout>();
        private List<LeftBF> lst_LeftBF = new List<LeftBF>();
        private List<RightBF> lst_RightBF = new List<RightBF>();
        private List<Node> lstNode = new List<Node>();
        private List<Link> lstLink = new List<Link>();
        private List<AGV> lstAGV = new List<AGV>();
        private CurrentTransportCommand CurrentJob = new CurrentTransportCommand();
        private uc_Buffer[] uc_Buffer = new uc_Buffer[10000];
        private uc_Tag[] uc_Tag = new uc_Tag[10000];
        private AGV_Slim[] AGV_Slim = new AGV_Slim[10000];
        private DataTable dtMaps = new DataTable();
        private ActUtlType PLC = new ActUtlType();

        DispatcherTimer Timer_ConveyerRun;

        private string AGVID = "105";

        private string IP_PLC = "192.168.3.250";

        private int Port = 443;
        IPEndPoint IP;
        Socket sever_x;
        List<Socket> client_list;
        delegate void MyDelegate();

        private string _oldFullState, _oldLocaion;
        private int _oldOutputRequest;
        private string _oldLeftState, _oldRightState, _oldInputState, _oldOutpuState;
        private string _crAlarm;
        private int _agvX = 0;
        private int _agvY = 0;

        public MainWindow()
        {
            InitializeComponent();
            MakeSerrverConnect();
            Connect_PLC();
            ConnectAGV();
            Load_Layout();
            Load_Map();
            AGV_check();
            Update_AGV("5");

            DispatcherTimer Timer_CheckCommand = new DispatcherTimer();
            Timer_CheckCommand.Interval = TimeSpan.FromSeconds(2);
            Timer_CheckCommand.Tick += Timer_CheckCommand_Tick;
            Timer_CheckCommand.Start();

            DispatcherTimer Timer_Check_PLCAlive = new DispatcherTimer();
            Timer_Check_PLCAlive.Interval = TimeSpan.FromSeconds(0.5);
            Timer_Check_PLCAlive.Tick += Timer_Check_PLCAlive_Tick;
            Timer_Check_PLCAlive.Start();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();

            DispatcherTimer TimerPing = new DispatcherTimer();
            TimerPing.Interval = TimeSpan.FromSeconds(5);
            TimerPing.Tick += TimerPing_Tick;
            TimerPing.Start();

            Timer_ConveyerRun = new DispatcherTimer();
            Timer_ConveyerRun.Interval = TimeSpan.FromSeconds(5);
            Timer_ConveyerRun.Tick += Timer_ConveyerRun_Tick;
        }

        //Check alive PLC
        int t = 0;

        private void ConnectAGV()
        {
            SWMPort.PortName = clsFileIO.ReadValue("COM_SWMPORT");
            SWMPort.BaudRate = int.Parse(clsFileIO.ReadValue("BAURATE"));
            SWMPort.DataReceived += SWMPort_DataReceived;
            try
            {
                if (!SWMPort.IsOpen)
                    SWMPort.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Không kết nối được cổng serial. Vui lòng kiểm tra lại cấu hình COM.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SWMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (SWMPort.BytesToRead > 500)
                {
                    SWMPort.DiscardInBuffer();
                    return;
                }
                string data = SWMPort.ReadTo("x");

                data = data.Trim();
                ArduinoDataAnalys(data);

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// xử lý dữ liệu nhận từ ACS
        /// </summary>
        /// <param name="data"></param>
        private void ArduinoDataAnalys(string data)
        {
            try
            {
                //quay băng tải IN
                if (data == "1")
                {
                    PLC.SetDevice("M2100", 1);
                }
                //Quay băng tải out
                else if (data == "2")
                {
                    PLC.SetDevice("M2200", 1);
                    Timer_ConveyerRun.Start();
                }
                //Nhập hàng lên kho khi nhận tin C1x từ cổng serial
                else if (data.StartsWith("C1"))
                {
                    this.Dispatcher.Invoke(CreateImportCommand);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Tạo lệnh nhập hàng từ cổng vào (IP01) lên ô trống trong kho.
        /// </summary>
        private void CreateImportCommand()
        {
            try
            {
                DataTable dtEmptyBF = BLLayout.LoadEmptyBF();
                if (dtEmptyBF.Rows.Count == 0)
                    return;

                TransportCommand transport = new TransportCommand();
                transport.AGVID = "105";
                transport.STKID = "B1STK01";
                transport.CommandID = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + "B1STK01_CV01_IP01" + "_" + dtEmptyBF.Rows[0]["BFNAME"].ToString();
                transport.CommandSource = "B1STK01_CV01_IP01";
                transport.CommandDest = dtEmptyBF.Rows[0]["BFNAME"].ToString();
                transport.CommandSourceID = "215";
                transport.CommandDestID = dtEmptyBF.Rows[0]["BFID"].ToString();
                transport.CommandStatus = "JOB CREATE";
                transport.JobStart = DateTime.Now;
                transport.TrayID = "";

                BLTransportCommand.InsertTransportCommand(transport);
                BLLayout.UpdateTrayID(transport.CommandSourceID, "");
                LoadTransportCommand();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// dừng băng tải sau 5s quay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_ConveyerRun_Tick(object sender, EventArgs e)
        {
            PLC.SetDevice("M2200", 0);
            PLC.SetDevice("M2301", 0);
            PLC.SetDevice("Y4", 0);
            Timer_ConveyerRun.Stop();
        }
        private void Timer_Check_PLCAlive_Tick(object sender, EventArgs e)
        {
            t++;
            if (t >= 100)
                t = 0;

            if (t % 1 == 0)
                PLC.SetDevice("M845", 1);
            else
                PLC.SetDevice("M845", 0);

        }

        private void TimerPing_Tick(object sender, EventArgs e)
        {
            PingPLC();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                ReadPLCParameter();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Tạo server kết nối
        /// </summary>
        private void MakeSerrverConnect()
        {
            client_list = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, Port);
            sever_x = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            sever_x.Bind(IP);
            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        sever_x.Listen(100);
                        Socket client_x = sever_x.Accept();
                        client_list.Add(client_x);
                        Thread recevie = new Thread(Recevie_Data);
                        recevie.IsBackground = true;
                        recevie.Start(client_x);
                    }
                }
                catch (Exception)
                {
                    IP = new IPEndPoint(IPAddress.Any, Port);
                    sever_x = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }

        /// <summary>
        /// nhận data từ client
        /// </summary>
        /// <param name="obj"></param>
        private void Recevie_Data(object obj)
        {
            Socket client_x = obj as Socket;
            try
            {
                while (true)
                {
                    Byte[] data = new Byte[14];
                    client_x.Receive(data);
                    string mess_rev = (string)Deseriliaze(data);//105AC0101O1
                    this.Dispatcher.Invoke(() =>
                    {
                        //Do something

                        //quay băng tải IN
                        if (mess_rev.Substring(0, 1) == "1")
                        {
                            PLC.SetDevice("M2100", 1);
                        }
                        //Quay băng tải out
                        else if (mess_rev.Substring(0, 1) == "2")
                        {
                            PLC.SetDevice("M2200", 1);
                            Timer_ConveyerRun.Start();
                        }

                        //Reply
                    });

                }
            }
            catch (Exception)
            {
                client_list.Remove(client_x);
                client_x.Close();
            }
        }

        object Deseriliaze(byte[] data)
        {
            MemoryStream Stream_x = new MemoryStream(data, 0, 14);
            BinaryFormatter formatter = new BinaryFormatter();
            string data_AGV = System.Text.Encoding.ASCII.GetString(data, 0, 14);
            return data_AGV;
        }

        private void Connect_PLC()
        {
            try
            {
                PLC.ActLogicalStationNumber = 25;
                PLC.Open();
                //PLC.SetDevice("D5100", 0);
                //PLC.SetDevice("D5200", 1);
                //PLC.SetDevice("M1", 1);
                BLUpdateAGVStatus.UpdateAGVStatus(AGVID, "5", "EMPTY");
                _oldFullState = "EMPTY"; _oldLocaion = "5";
                MessageBox.Show("Kết nối PLC thành công", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PingPLC()
        {
            try
            {
                Ping PLCPing = new Ping();
                PingReply Reply = PLCPing.Send(IP_PLC);
                // check when the ping is not success
                if (Reply.Status != IPStatus.Success)
                {
                    AlarmLog.LogAlarmToDatabase("04");
                    MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                AlarmLog.LogAlarmToDatabase("04");
                MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // định kỳ kiểm tra trạng thái lệnh
        private void Timer_CheckCommand_Tick(object sender, EventArgs e)
        {
            //Hàm xử lý call lệnh vận chuyển sang PLC
            try
            {
                DataTable dtCommand = new DataTable();
                dtCommand = BLReport.GetTransportCommand();
                string _crCommandID = dtCommand.Rows[0]["CommandID"].ToString();
                string _crJobStatus = dtCommand.Rows[0]["CommandStatus"].ToString();
                if (_crCommandID != CurrentJob.CommandID)
                {
                    CurrentJob.AGVID = dtCommand.Rows[0]["AGVID"].ToString();
                    CurrentJob.STKID = dtCommand.Rows[0]["STKID"].ToString();
                    CurrentJob.CommandSource = dtCommand.Rows[0]["CommandSource"].ToString();
                    CurrentJob.CommandSourceID = dtCommand.Rows[0]["CommandSourceID"].ToString();
                    CurrentJob.CommandDest = dtCommand.Rows[0]["CommandDest"].ToString();
                    CurrentJob.CommandDestID = dtCommand.Rows[0]["CommandDestID"].ToString();
                    CurrentJob.TrayID = dtCommand.Rows[0]["TrayID"].ToString();
                    CurrentJob.ProductID = dtCommand.Rows[0]["ProductID"].ToString();
                    if (_crJobStatus == "JOB CREATE" && (_oldLocaion == "0" || _oldLocaion == "1"))
                    {
                        DataTable dtLayout = new DataTable();
                        dtLayout = BLLayout.LoadLayoutConfig();
                        DataRow[] drSource = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandSource + "%'");
                        DataRow[] drDest = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandDest + "%'");

                        // nếu source EMPTY hoặc dest full thì cancel lệnh 
                        if (drSource[0].ItemArray[2].ToString() == "EMPTY" || (CurrentJob.CommandDestID != "115" && drDest[0].ItemArray[2].ToString() == "FULL"))
                        {
                            CurrentJob.CommandID = dtCommand.Rows[0]["CommandID"].ToString();
                            CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                            BLTransportCommand.DeleteJob(CurrentJob.CommandID, CurrentJob.JobCreat);
                        }
                        else
                        {
                            CurrentJob.CommandID = dtCommand.Rows[0]["CommandID"].ToString();
                            CurrentJob.CommandStatus = "JOB START";
                            CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                            CurrentJob.JobAssign = DateTime.Now;

                            //Kích bit reset lệnh
                            PLC.SetDevice("M2000", 0);

                            //Kiểm tra loại lệnh 
                            if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID != "115")
                            {
                                //Lấy hàng
                                PLC.SetDevice("D3000", 1);
                            }
                            else if (CurrentJob.CommandDestID == "115" && CurrentJob.CommandSourceID != "215")
                            {
                                //Trả hàng
                                PLC.SetDevice("D3000", 2);
                            }
                            else if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID == "115")
                            {
                                //Chuyển từ IP ra OP
                                PLC.SetDevice("D3000", 3);
                            }
                            else if (CurrentJob.CommandSourceID != "215" && CurrentJob.CommandDestID != "115")
                            {
                                //Đảo hàng trong kho
                                PLC.SetDevice("D3000", 4);
                            }
                            //Kích bit start lệnh
                            PLC.SetDevice("M2000", 1);

                            //Ghi source và dest xuống PLC
                            PLC.SetDevice("D2100", int.Parse(CurrentJob.CommandSourceID));
                            PLC.SetDevice("D2150", int.Parse(CurrentJob.CommandDestID));
                            BLTransportCommand.UpdateCommandStatus(CurrentJob);
                            BLUpdateAGVStatus.UpdateAGVCommand(AGVID, CurrentJob.CommandID);
                        }

                        //Load lại danh sách lệnh vận chuyển
                        LoadTransportCommand();
                    }
                    else if (_crJobStatus != "JOB CREATE")
                    {
                        //Kích bit reset lệnh
                        PLC.SetDevice("M2000", 0);

                        CurrentJob.CommandID = dtCommand.Rows[0]["CommandID"].ToString();
                        CurrentJob.CommandStatus = dtCommand.Rows[0]["CommandStatus"].ToString();
                        CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                        CurrentJob.JobAssign = DateTime.Parse(dtCommand.Rows[0]["JobAssign"].ToString());

                        //Kiểm tra loại lệnh 
                        if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID != "115")
                        {
                            //Lấy hàng
                            PLC.SetDevice("D3000", 1);
                        }
                        else if (CurrentJob.CommandDestID == "115" && CurrentJob.CommandSourceID != "215")
                        {
                            //Trả hàng
                            PLC.SetDevice("D3000", 2);
                        }
                        else if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID == "115")
                        {
                            //Chuyển từ IP ra OP
                            PLC.SetDevice("D3000", 3);
                        }
                        else if (CurrentJob.CommandSourceID != "215" && CurrentJob.CommandDestID != "115")
                        {
                            //Đảo hàng trong kho
                            PLC.SetDevice("D3000", 4);
                        }
                        //Kích bit start lệnh
                        PLC.SetDevice("M2000", 1);

                        //Ghi source và dest xuống PLC
                        PLC.SetDevice("D2100", int.Parse(CurrentJob.CommandSourceID));
                        PLC.SetDevice("D2150", int.Parse(CurrentJob.CommandDestID));
                        BLTransportCommand.UpdateCommandStatus(CurrentJob);
                        BLUpdateAGVStatus.UpdateAGVCommand(AGVID, CurrentJob.CommandID);

                        //Load lại danh sách lệnh vận chuyển
                        LoadTransportCommand();
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        //thuệc hiện restart lại lệnh đang thực hiện
        private void RefreshCommand()
        {
            DataTable dtCommand = new DataTable();
            dtCommand = BLReport.GetTransportCommand();
            string _crCommandID = dtCommand.Rows[0]["CommandID"].ToString();
            string _crJobStatus = dtCommand.Rows[0]["CommandStatus"].ToString();

            CurrentJob.AGVID = dtCommand.Rows[0]["AGVID"].ToString();
            CurrentJob.STKID = dtCommand.Rows[0]["STKID"].ToString();
            CurrentJob.CommandSource = dtCommand.Rows[0]["CommandSource"].ToString();
            CurrentJob.CommandSourceID = dtCommand.Rows[0]["CommandSourceID"].ToString();
            CurrentJob.CommandDest = dtCommand.Rows[0]["CommandDest"].ToString();
            CurrentJob.CommandDestID = dtCommand.Rows[0]["CommandDestID"].ToString();
            CurrentJob.TrayID = dtCommand.Rows[0]["TrayID"].ToString();
            CurrentJob.ProductID = dtCommand.Rows[0]["ProductID"].ToString();
            if (_crJobStatus != "JOB CREATE")
            {
                //Kích bit reset lệnh 
                PLC.SetDevice("M2000", 0);

                CurrentJob.CommandID = dtCommand.Rows[0]["CommandID"].ToString();
                CurrentJob.CommandStatus = dtCommand.Rows[0]["CommandStatus"].ToString();
                CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                CurrentJob.JobAssign = DateTime.Parse(dtCommand.Rows[0]["JobAssign"].ToString());

                //Kiểm tra loại lệnh 
                if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID != "115")
                {
                    //Lấy hàng
                    PLC.SetDevice("D3000", 1);
                }
                else if (CurrentJob.CommandDestID == "115" && CurrentJob.CommandSourceID != "215")
                {
                    //Trả hàng
                    PLC.SetDevice("D3000", 2);
                }
                else if (CurrentJob.CommandSourceID == "215" && CurrentJob.CommandDestID == "115")
                {
                    //Chuyển từ IP ra OP
                    PLC.SetDevice("D3000", 3);
                }
                else if (CurrentJob.CommandSourceID != "215" && CurrentJob.CommandDestID != "115")
                {
                    //Đảo hàng trong kho
                    PLC.SetDevice("D3000", 4);
                }
                //Kích bit start lệnh
                PLC.SetDevice("M2000", 1);

                //Ghi source và dest xuống PLC
                PLC.SetDevice("D2100", int.Parse(CurrentJob.CommandSourceID));
                PLC.SetDevice("D2150", int.Parse(CurrentJob.CommandDestID));
                BLTransportCommand.UpdateCommandStatus(CurrentJob);
                BLUpdateAGVStatus.UpdateAGVCommand(AGVID, CurrentJob.CommandID);

                //Load lại danh sách lệnh vận chuyển
                LoadTransportCommand();
            }
        }

        //Cập nhật trạng thái lệnh theo vị trí
        private void UpdateCommandStatusByLocation()
        {
            try
            {
                int _completeState;
                PLC.GetDevice("M3000", out _completeState);

                int _transferingDestState;
                PLC.GetDevice("M510", out _transferingDestState);

                int _location;
                PLC.GetDevice("D800", out _location);
                string strLocation = _location.ToString();
                int _portSource = 0;
                int _portDest = 0;
                if (CurrentJob.CommandSourceID.Length > 0 && CurrentJob.CommandDestID.Length > 0)
                {
                    if (CurrentJob.CommandSourceID == "215") _portSource = 1;
                    else _portSource = 1 + int.Parse(CurrentJob.CommandSourceID.Substring(2, 1));

                    if (CurrentJob.CommandDestID == "115") _portDest = 1;
                    else _portDest = 1 + int.Parse(CurrentJob.CommandDestID.Substring(2, 1));

                    //check vị trí AGV để update trạng thái lệnh
                    if ((_location == _portSource) && CurrentJob.CommandStatus == "JOB START") // lấy hàng
                    {
                        //Kích bit start lệnh
                        PLC.SetDevice("M2000", 1);

                        CurrentJob.CommandStatus = "TRANSFERING DEST";
                        BLTransportCommand.UpdateCommandStatus(CurrentJob);
                        BLLayout.UpdateBFStateByStep(CurrentJob.CommandSourceID, "EMPTY");
                        Load_Layout();
                        LoadTransportCommand();
                    }
                    else if ((_location == _portDest || _completeState == 1) && CurrentJob.CommandStatus == "TRANSFERING DEST") //trả hàng  (_location == _portDest ||)
                    {
                        CurrentJob.CommandStatus = "JOB COMPLETE";
                        CurrentJob.JobComplete = DateTime.Now;
                        BLTransportCommand.UpdateCommandStatus(CurrentJob);
                        BLLayout.UpdateBFStateByStep(CurrentJob.CommandDestID, "FULL");
                        BLLayout.UpdateTrayID(CurrentJob.CommandDestID, CurrentJob.TrayID);
                        LoadTransportCommand();
                        Load_Layout();
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void ReadPLCParameter()
        {
            try
            {
                UpdateCommandStatusByLocation();
                //D500: Trạng thái Full/Emplty
                int _fullState;
                PLC.GetDevice("M510", out _fullState);
                string strAGVFullState;
                if (_fullState == 1) strAGVFullState = "FULL";
                else strAGVFullState = "EMPTY";

                //D550: Vị trí AGV
                int _location;
                PLC.GetDevice("D800", out _location);
                string strLocation = _location.ToString();
                if ((strLocation != _oldLocaion) || (strAGVFullState != _oldFullState))
                {
                    BLUpdateAGVStatus.UpdateAGVStatus(AGVID, strLocation, strAGVFullState);
                    Update_AGV(strLocation);
                    _oldFullState = strAGVFullState;
                    _oldLocaion = strLocation;
                }

                //D2500: Báo lỗi PLC
                int _Alarm;
                PLC.GetDevice("D2500", out _Alarm);
                string strAlarm = _Alarm.ToString();
                if (strAlarm != _crAlarm)
                {
                    AlarmLog.LogAlarmToDatabase(strAlarm);
                    _crAlarm = strAlarm;
                }
             
                //Trạng thái IP01
                int _inputState;
                PLC.GetDevice("M2300", out _inputState);
                string strInputState = _inputState == 1 ? "FULL" : "EMPTY";
                if (strInputState != _oldInputState)
                {
                    BLLayout.UpdateInOutState(215, strInputState);
                    Load_Layout();
                    _oldInputState = strInputState;
                }

                //Trạng thái OP01
                int _OutputState;
                PLC.GetDevice("M2301", out _OutputState);
                string strOutputState;
                if (_OutputState == 1) strOutputState = "FULL";
                else strOutputState = "EMPTY";

                if (strOutputState != _oldOutpuState)
                {
                    BLLayout.UpdateInOutState(115, strOutputState);
                    foreach(Socket client in client_list)
                    {
                        // Process the data sent by the client
                        strOutputState = strOutputState.ToUpper();

                        byte[] msg = Encoding.UTF8.GetBytes(strOutputState); //Gửi trạng thái băng tải out sang ACS

                        // Send back a response
                        client.Send(msg);
                        
                    }
                    //BLACSComunication.UpdateOutputState(strOutputState);
                    Load_Layout();
                    _oldOutpuState = strOutputState;
                }

                int outputRequest;
                PLC.GetDevice("D2350", out outputRequest);
                HandleHmiOutputRequest(outputRequest);
            }
            catch (Exception)
            {

            }

        }

        private void HandleHmiOutputRequest(int outputRequest)
        {
            if (outputRequest <= 0)
            {
                _oldOutputRequest = 0;
                return;
            }

            if (outputRequest == _oldOutputRequest)
                return;

            DataTable dtPort = BLLayout.LoadFullBF();
            if (dtPort.Rows.Count > 0)
                CreateExportCommand(dtPort);

            PLC.SetDevice("D2350", 0);
            _oldOutputRequest = outputRequest;
        }

        private void CreateExportCommand(DataTable dtPort)
        {
            string portSource = dtPort.Rows[0]["BFNAME"].ToString();
            string idSource = dtPort.Rows[0]["BFID"].ToString();
            TransportCommand transport = new TransportCommand();
            transport.AGVID = "105";
            transport.STKID = "B1STK01";
            transport.CommandID = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + portSource + "_" + "B1STK01_CV01_OP01";
            transport.CommandSource = portSource;
            transport.CommandDest = "B1STK01_CV01_OP01";
            transport.CommandSourceID = idSource;
            transport.CommandDestID = "115";
            transport.CommandStatus = "JOB CREATE";
            transport.JobStart = DateTime.Now;
            transport.TrayID = dtPort.Rows[0]["TRAYID"].ToString();

            BLTransportCommand.InsertTransportCommand(transport);
            LoadTransportCommand();
        }

        private void AGV_check()
        {
            DataTable AGV_load = new DataTable();

            string dbcomman = "";
            try
            {

                dbcomman = @"SELECT A.ID AS AGVID, A.BAYID AS BAYID,A.FULLSTATE AS STATE,A.CURRENTNODEID AS CURRENTTAG,PROCESSINGSTATE AS STATUS,
                                    B.XPOS AS X_POS, YPOS AS Y_POS, A.ALARMSTATE AS ALARM,A.BATTERYVOLTAGE AS BATERRY,
                                    A.RUNSTATE AS RUNSTATE, A.CONNECTIONSTATE AS CONNECTIONSTATE
                                    FROM NA_R_VEHICLE A,NA_R_NODE B
                                    WHERE a.CURRENTNODEID =b.ID";

                AGV_load.Clear();
                AGV_load = BLLayout.ReadAGVCurrentParam(dbcomman);


                Load_AGV(AGV_load);

            }
            catch (Exception)
            {
                ;
            }
        }

        private void Load_AGV(DataTable AGV_load)
        {
            try
            {
                for (int i = 0; i < AGV_load.Rows.Count; i++)
                {
                    AGV AGV_startup = new AGV();
                    AGV_startup.ID = AGV_load.Rows[i]["AGVID"].ToString();
                    AGV_startup.BAYID = AGV_load.Rows[i]["BAYID"].ToString();
                    AGV_startup.STATE = AGV_load.Rows[i]["STATE"].ToString();
                    AGV_startup.STATUS = AGV_load.Rows[i]["STATUS"].ToString();
                    AGV_startup.BATTERY = Convert.ToInt32(AGV_load.Rows[i]["BATERRY"]);
                    AGV_startup.NODE = AGV_load.Rows[i]["CURRENTTAG"].ToString();
                    AGV_startup.X = Convert.ToInt32(AGV_load.Rows[i]["X_POS"]);
                    AGV_startup.Y = Convert.ToInt32(AGV_load.Rows[i]["Y_POS"]);
                    AGV_startup.ALARM = AGV_load.Rows[i]["ALARM"].ToString();
                    AGV_startup.CONNECTSTATE = AGV_load.Rows[i]["CONNECTIONSTATE"].ToString();
                    AGV_startup.RUNSTATE = AGV_load.Rows[i]["RUNSTATE"].ToString();
                    lstAGV.Add(AGV_startup);
                    //txb_link.Text = node.ID;
                }
                this.Dispatcher.Invoke(() =>
                {
                    Add_AGV();
                });
            }
            catch (Exception)
            {

                ;
            }

        }

        private void Add_AGV()
        {
            try
            {
                foreach (var agv in lstAGV)
                {
                    int ID_AGV = 0;


                    int.TryParse(agv.ID.ToString(), out ID_AGV);
                    if (ID_AGV < 10000)
                    {

                        AGV_Slim[ID_AGV] = new AGV_Slim();
                        AGV_Slim[ID_AGV].Height = 40;
                        AGV_Slim[ID_AGV].Width = 30;
                        AGV_Slim[ID_AGV].color_Baterry = System.Windows.Media.Brushes.Orange;
                        AGV_Slim[ID_AGV].AGV_Name = ID_AGV.ToString();

                        if (agv.ALARM == "NOALARM")
                        {
                            if (agv.STATUS == "RUN")
                                AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Lime;
                            else if (agv.STATUS == "PARK")
                                AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Yellow;
                            else if (agv.STATUS == "CHARGE")
                                AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.PaleGoldenrod;
                            else if (agv.STATUS == "IDLE")
                                AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.PaleVioletRed;
                        }

                        else
                            AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Red;
                        if (agv.STATE == "FULL")
                            AGV_Slim[ID_AGV].colorTray = System.Windows.Media.Brushes.Black;
                        else if (agv.STATE == "EMPTY")
                        {
                            AGV_Slim[ID_AGV].colorTray = AGV_Slim[ID_AGV].colorbackgroud;
                            AGV_Slim[ID_AGV].rtgSlottray.StrokeThickness = 0;
                        }
                        Canvas.SetLeft(AGV_Slim[ID_AGV], agv.X - 15);
                        Canvas.SetTop(AGV_Slim[ID_AGV], agv.Y - 20);
                        cvs_Map.Children.Add(AGV_Slim[ID_AGV]);
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        private void Update_AGV(string strLocation)
        {

            try
            {
                DataTable dtAGV = new DataTable();
                dtAGV = BLLayout.Load_AGV();

                lstAGV.Clear();

                for (int i = 0; i < dtAGV.Rows.Count; i++)
                {
                    AGV AGV_startup = new AGV();

                    //int.TryParse(AGV_startup.ID.ToString(), out ID_AGV);
                    AGV_startup.ID = dtAGV.Rows[i]["ID"].ToString();
                    AGV_startup.BAYID = dtAGV.Rows[i]["BAYID"].ToString();
                    AGV_startup.STATE = dtAGV.Rows[i]["FULLSTATE"].ToString();
                    AGV_startup.STATUS = dtAGV.Rows[i]["PROCESSINGSTATE"].ToString();
                    AGV_startup.BATTERY = Convert.ToInt32(dtAGV.Rows[i]["BATTERYVOLTAGE"]);
                    AGV_startup.NODE = dtAGV.Rows[i]["CURRENTNODEID"].ToString();
                    AGV_startup.X = Convert.ToInt32(dtAGV.Rows[i]["X_POS"]);
                    AGV_startup.Y = Convert.ToInt32(dtAGV.Rows[i]["Y_POS"]);
                    AGV_startup.ALARM = dtAGV.Rows[i]["ALARMSTATE"].ToString();
                    AGV_startup.CONNECTSTATE = dtAGV.Rows[i]["CONNECTIONSTATE"].ToString();
                    AGV_startup.RUNSTATE = dtAGV.Rows[i]["RUNSTATE"].ToString();
                    AGV_startup.COMMAND = dtAGV.Rows[i]["TRANSPORTCOMMANDID"].ToString();

                    for (int j = 0; j < dtMaps.Rows.Count; j++)
                    {
                        if (AGV_startup.NODE == dtMaps.Rows[j]["FRTAG"].ToString())
                        {
                            AGV_startup.NEXTNODE = dtMaps.Rows[j]["TONODE"].ToString();
                            AGV_startup.NEXT_X = Convert.ToInt32(dtMaps.Rows[j]["TO_X"]);
                            AGV_startup.NEXT_Y = Convert.ToInt32(dtMaps.Rows[j]["TO_Y"]);
                        }
                    }
                    lstAGV.Add(AGV_startup);
                }
                this.Dispatcher.Invoke(() =>
                {
                    AGV_Update(strLocation);
                });

            }
            catch (Exception e)
            {
                AlarmLog.LogAlarmToDatabase("08");
                MessageBox.Show(e.ToString());
            }
        }

        private void AGV_Update(string strLocation)
        {
            double AGV_count = lstAGV.Count;
            double AGV_full = 0;
            double AGV_Empty = 0;
            double AGV_connect = 0;
            double AGV_Run = 0;

            try
            {
                foreach (var agv in lstAGV)
                {
                    int ID_AGV = 0;

                    int.TryParse(agv.ID.ToString(), out ID_AGV);

                    if (agv.BATTERY >= 25)
                        AGV_Slim[ID_AGV].color_Baterry = System.Windows.Media.Brushes.LimeGreen;
                    else if (agv.BATTERY >= 24 && agv.BATTERY < 25)
                        AGV_Slim[ID_AGV].color_Baterry = System.Windows.Media.Brushes.Orange;
                    else
                        AGV_Slim[ID_AGV].color_Baterry = System.Windows.Media.Brushes.Tomato;

                    if (agv.ALARM == "NOALARM")
                    {
                        if (agv.STATUS == "RUN")
                            AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Lime;
                        else if (agv.STATUS == "PARK")
                            AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Yellow;
                        else if (agv.STATUS == "CHARGE")
                            AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.PaleGoldenrod;
                        else if (agv.STATUS == "IDLE")
                            AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.PaleVioletRed;
                    }

                    else
                        AGV_Slim[ID_AGV].colorbackgroud = System.Windows.Media.Brushes.Red;
                    if (agv.STATE == "FULL")
                    {
                        AGV_Slim[ID_AGV].colorTray = System.Windows.Media.Brushes.Black;
                        AGV_full++;
                    }

                    else if (agv.STATE == "EMPTY")
                    {
                        AGV_Slim[ID_AGV].colorTray = AGV_Slim[ID_AGV].colorbackgroud;
                        AGV_Slim[ID_AGV].rtgSlottray.StrokeThickness = 0;
                        AGV_Empty++;
                    }

                    if (agv.CONNECTSTATE == "CONNECT")
                        AGV_connect++;

                    if (agv.RUNSTATE == "RUN")
                        AGV_Run++;

                    ////================update huong di chuyen cho ag

                    if (int.Parse(strLocation) > int.Parse(_oldLocaion))
                        AGV_Slim[ID_AGV].Direction_AGV = -90; //Update huong di chuyen cua AGV

                    else if (int.Parse(strLocation) < int.Parse(_oldLocaion))
                        AGV_Slim[ID_AGV].Direction_AGV = 90; //Update huong di chuyen cua AGV


                    TranslateTransform trans = new TranslateTransform();
                    AGV_Slim[ID_AGV].RenderTransform = trans;
                    DoubleAnimation animX = new DoubleAnimation(_agvX, agv.X - 168, TimeSpan.FromSeconds(2));
                    trans.BeginAnimation(TranslateTransform.XProperty, animX);
                    DoubleAnimation animY = new DoubleAnimation(3, 3, TimeSpan.FromSeconds(2));
                    trans.BeginAnimation(TranslateTransform.YProperty, animY);

                    _agvX = agv.X - 168;
                    _agvY = 0;

                    AGV_Slim[ID_AGV].ToolTip = string.Format("Vehicle ID: {0}\nBAY ID: {1}\nStatus: {2}\nTrCmdId: {3}", agv.ID, agv.BAYID, agv.STATUS, agv.COMMAND);

                    ToolTipService.SetShowDuration(AGV_Slim[ID_AGV], 2000000);
                }

                AGV_Connect.TotalValue = AGV_count;
                AGV_FullState.TotalValue = AGV_count;
                AGV_RunState.TotalValue = AGV_count;

                AGV_FullState.FullValue = AGV_full;
                AGV_FullState.EmptyValue = AGV_count - AGV_full;
                double empty_rate = (AGV_Empty) / (AGV_count) * 360;
                AGV_FullState.AngleValue = empty_rate;

                AGV_Connect.FullValue = AGV_connect;    //Full=Connect   Empty=Disconnect
                AGV_Connect.EmptyValue = AGV_count - AGV_connect;
                AGV_Connect.AngleValue = AGV_Connect.EmptyValue / AGV_count * 360;

                AGV_RunState.FullValue = AGV_Run;       //Full=Run      Empty=Stop
                AGV_RunState.EmptyValue = AGV_count - AGV_Run;
                AGV_RunState.AngleValue = AGV_RunState.EmptyValue / AGV_count * 360;

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void Load_Map()
        {
            try
            {
                DataTable dtMap = new DataTable();
                dtMap = BLLayout.LoadMapConfig();
                dtMaps = dtMap;

                for (int i = 0; i < dtMap.Rows.Count; i++)
                {
                    try
                    {
                        Node node = new Node();
                        node.ID = dtMap.Rows[i]["FRTAG"].ToString();
                        node.X = Convert.ToInt32(dtMap.Rows[i]["FROM_X"]);
                        node.Y = Convert.ToInt32(dtMap.Rows[i]["FROM_Y"]);
                        lstNode.Add(node);

                        Link horlink = new Link();
                        horlink.ID = dtMap.Rows[i]["LINK_ID"].ToString();
                        horlink.Source = horlink.ID.Substring(0, 4);
                        horlink.Dest = horlink.ID.Substring(5, 4);
                        horlink.Distance = dtMap.Rows[i]["DIS"].ToString();
                        horlink.StartX = Convert.ToInt32(dtMap.Rows[i]["FROM_X"]);
                        horlink.StartY = Convert.ToInt32(dtMap.Rows[i]["FROM_Y"]);
                        horlink.EndX = Convert.ToInt32(dtMap.Rows[i]["TO_X"]);
                        horlink.EndY = Convert.ToInt32(dtMap.Rows[i]["TO_Y"]);
                        lstLink.Add(horlink);
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
                foreach (Link link in lstLink)
                {
                    Line line = new Line { StrokeThickness = 2, Stroke = System.Windows.Media.Brushes.Gray, ToolTip = link.ID };
                    //line.ToolTip= string.Format("Link: {0}", link.ID);
                    line.X1 = link.StartX;
                    line.Y1 = link.StartY;
                    line.X2 = link.EndX;
                    line.Y2 = link.EndY;
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
                foreach (var node in lstNode)
                {
                    int Node_ID = 0;
                    int.TryParse(node.ID.ToString(), out Node_ID);
                    uc_Tag[Node_ID] = new uc_Tag();
                    uc_Tag[Node_ID].Height = 6;
                    uc_Tag[Node_ID].Width = 6;
                    foreach (var link in lstLink)
                    {
                        if (node.ID == link.Source)
                        {

                            if (link.StartX > link.EndX && link.StartY == link.EndY)
                                uc_Tag[Node_ID].rotation_tag.Angle = 0;

                            else if (link.StartX < link.EndX && link.StartY == link.EndY)
                                uc_Tag[Node_ID].rotation_tag.Angle = 180;
                        }
                    }

                    if (Node_ID < 10000)
                    {
                        uc_Tag[Node_ID].colorbackgroud = System.Windows.Media.Brushes.DimGray;
                        uc_Tag[Node_ID].ToolTip = string.Format("Node: {0}", node.ID);
                        ToolTipService.SetShowDuration(uc_Tag[Node_ID], 20000);
                        Canvas.SetLeft(uc_Tag[Node_ID], node.X - 3);
                        Canvas.SetTop(uc_Tag[Node_ID], node.Y - 3);

                        cvs_Map.Children.Add(uc_Tag[Node_ID]);
                        uc_Tag[Node_ID].Visibility = Visibility.Visible;
                    }
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
                int _FullBF = 0; int _EmptyBF = 0;
                DataTable dtLayout = new DataTable();
                dtLayout = BLLayout.LoadLayoutConfig();
                lstBF.Clear();
                for (int i = 0; i < dtLayout.Rows.Count; i++)
                {
                    BFLayout BF_Startup = new BFLayout();
                    BF_Startup.ID = dtLayout.Rows[i]["BFID"].ToString();
                    BF_Startup.X = Convert.ToInt32(dtLayout.Rows[i]["XPOS"]);
                    BF_Startup.Y = Convert.ToInt32(dtLayout.Rows[i]["YPOS"]);
                    BF_Startup.PORTNAME = dtLayout.Rows[i]["BFNAME"].ToString();
                    BF_Startup.FULLSTATE = dtLayout.Rows[i]["FULLSTATE"].ToString();
                    if (BF_Startup.FULLSTATE == "FULL")
                        _FullBF++;
                    else
                        _EmptyBF++;
                    BF_Startup.TRAYID = dtLayout.Rows[i]["TRAYID"].ToString();
                    BF_Startup.PRODUCTID = dtLayout.Rows[i]["PRODUCTCODE"].ToString();
                    TimeSpan x = DateTime.Now - DateTime.Parse(dtLayout.Rows[i]["UPDATETIME"].ToString());
                    BF_Startup.AGINGTIME = (x.Days * 24).ToString() + ":" + x.Minutes.ToString();// + ":" + x.Seconds.ToString();
                    lstBF.Add(BF_Startup);
                    //txb_link.Text = node.ID;
                }
                this.Dispatcher.Invoke(() =>
                {
                    Load_BFLayout();

                    Buffer_FullState.TotalValue = dtLayout.Rows.Count;

                    AGV_FullState.FullValue = _FullBF;
                    AGV_FullState.EmptyValue = dtLayout.Rows.Count - _EmptyBF;
                    double empty_rate = (_EmptyBF) / (dtLayout.Rows.Count) * 360;
                    AGV_FullState.AngleValue = empty_rate;
                });
            }
            catch (Exception e)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        private void Load_BFLayout()
        {
            try
            {
                foreach (var bF in lstBF)
                {
                    int BFID = 0;

                    int.TryParse(bF.ID.ToString(), out BFID);
                    if (BFID < 10000)
                    {

                        uc_Buffer[BFID] = new uc_Buffer();
                        uc_Buffer[BFID].FullState = bF.FULLSTATE;
                        if (uc_Buffer[BFID].FullState == "FULL")
                        {
                            uc_Buffer[BFID].Material_ID = bF.PRODUCTID;
                            uc_Buffer[BFID].Material_Code = bF.TRAYID;
                            uc_Buffer[BFID].Aging_Time = bF.AGINGTIME;
                            uc_Buffer[BFID].Background = Brushes.Green;
                            uc_Buffer[BFID].rtgSlottray.Foreground = Brushes.White;// Brush.Color.FromRgb(0, 128, 0);
                        }
                        else
                        {
                            uc_Buffer[BFID].Background = Brushes.PaleGreen;
                            uc_Buffer[BFID].rtgSlottray.Foreground = Brushes.Black;
                        }

                        uc_Buffer[BFID].Port_Name = bF.PORTNAME;
                        if (uc_Buffer[BFID].Port_Name.Substring(0, 12) == "B1STK01_CV01")
                        {
                            uc_Buffer[BFID].Height = 145;
                            uc_Buffer[BFID].Width = 240;
                        }
                        else
                        {
                            uc_Buffer[BFID].Height = 130;
                            uc_Buffer[BFID].Width = 240;
                        }

                        uc_Buffer[BFID].ToolTip = string.Format("Port Name: {0}\nTray ID: {1}\nProduct ID: {2}\nTrCmdId: {3}", bF.PORTNAME, bF.TRAYID, bF.PRODUCTID, "");

                        ToolTipService.SetShowDuration(uc_Buffer[BFID], 2000000);

                        Canvas.SetLeft(uc_Buffer[BFID], bF.X);
                        Canvas.SetTop(uc_Buffer[BFID], bF.Y);
                        cvs_Map.Children.Add(uc_Buffer[BFID]);
                    }
                }
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("09");
            }
        }

        byte[] etx = { 0x03 };
        byte[] stx = { 0x02 };
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Grid_Map.Visibility = Visibility.Visible;
            Grid_AGVDetail.Visibility = Visibility.Hidden;
            Grid_CommandHistory.Visibility = Visibility.Hidden;
            lblUser.Text = BLLogin.DisplayName;
            LoadTransportCommand();

            DataTable dt = new DataTable();
            dt = BLLayout.LoadLayoutConfig();
            DataRow[] dt_Left = dt.Select("BFNAME LIKE '%L%'");
            DataRow[] dt_Right = dt.Select("BFNAME LIKE '%R%'");
            LoadBFList(dt_Left, dt_Right);
        }

        private void LoadBFList(DataRow[] dt_Left, DataRow[] dt_Right)
        {

            foreach (DataRow _itemLeft in dt_Left)
            {
                LeftBF L_BF = new LeftBF();
                L_BF.ID = _itemLeft.ItemArray[9].ToString();
                L_BF.PORTNAME = _itemLeft.ItemArray[1].ToString();
                L_BF.FULLSTATE = _itemLeft.ItemArray[2].ToString();
                L_BF.TRAYID = _itemLeft.ItemArray[3].ToString();

                lst_LeftBF.Add(L_BF);
            }
            foreach (DataRow _itemRight in dt_Right)
            {
                RightBF R_BF = new RightBF();
                R_BF.ID = _itemRight.ItemArray[9].ToString();
                R_BF.PORTNAME = _itemRight.ItemArray[1].ToString();
                R_BF.FULLSTATE = _itemRight.ItemArray[2].ToString();
                R_BF.TRAYID = _itemRight.ItemArray[3].ToString();

                lst_RightBF.Add(R_BF);
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            Grid_Map.Visibility = Visibility.Visible;
            Grid_AGVDetail.Visibility = Visibility.Hidden;
            Grid_CommandHistory.Visibility = Visibility.Hidden;
            lblUser.Text = BLLogin.DisplayName;
        }

        private void btnBufferDetail_Click(object sender, RoutedEventArgs e)
        {
            Grid_Map.Visibility = Visibility.Hidden;
            Grid_AGVDetail.Visibility = Visibility.Visible;
            Grid_CommandHistory.Visibility = Visibility.Hidden;
            DataTable dtDetail = new DataTable();
            dtDetail = BLReport.GetBFDetail();
            dtgBFData.ItemsSource = dtDetail.DefaultView;

        }

        private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
        {
            Grid_Map.Visibility = Visibility.Hidden;
            Grid_AGVDetail.Visibility = Visibility.Hidden;
            Grid_CommandHistory.Visibility = Visibility.Visible;
            LoadTransportCommand();

        }

        private void LoadTransportCommand()
        {
            DataTable dtDetail = new DataTable();
            dtDetail = BLReport.GetTransportCommand();
            dtgCommandHistory.ItemsSource = dtDetail.DefaultView;
            //dtgCommandHistory.ColumnHeaderStyle = new Style(typeof(DataGridColumnHeader));
            //dtgCommandHistory.ColumnHeaderStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

            dtgCommandHistory.Columns[11].Visibility = Visibility.Hidden;
            dtgCommandHistory.Columns[12].Visibility = Visibility.Hidden;
            dtgCommandHistory.Columns[2].Header = "ID lệnh vận chuyển";

            DataTable dtJobCount = new DataTable();
            dtJobCount = BLReport.GetTransportJobCount();
            lbl_LoadCount.Content = dtJobCount.Rows[0]["InputCommand"].ToString();
            lbl_UnloadCount.Content = dtJobCount.Rows[0]["OutputCommand"].ToString();
        }

        private void btnManualControl_Click(object sender, RoutedEventArgs e)
        {
            Grid_Map.Visibility = Visibility.Hidden;
            Grid_AGVDetail.Visibility = Visibility.Hidden;
            Grid_CommandHistory.Visibility = Visibility.Visible;
            LoadTransportCommand();
            ManualControlWindow frm = new ManualControlWindow();
            frm.ShowDialog();
            LoadTransportCommand();
        }

        string _deleteJobID, _jobState;
        DateTime _deleteJobCreateTime;
        private void btnDeleteCommand_Click(object sender, RoutedEventArgs e)
        {
            // xóa lệnh, thực hiện update trạng thái lệnh về job cancel
            if (_jobState == "JOB CREATE")
            {
                BLTransportCommand.DeleteJob(_deleteJobID, _deleteJobCreateTime);
                LoadTransportCommand();
            }
            else
            {
                MessageBox.Show("Không thể xóa lệnh đang trong quá trình thực hiện!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        private void btnAddCommand_Click(object sender, RoutedEventArgs e)
        {
            ManualControlWindow frm = new ManualControlWindow();
            frm.ShowDialog();
            LoadTransportCommand();
        }

        private void dtgCommandHistory_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.HeaderStyle = new Style(typeof(DataGridColumnHeader));
            e.Column.HeaderStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            e.Column.HeaderStyle.Setters.Add(new Setter(BorderBrushProperty, Brushes.Transparent));

            //e.Column.CellStyle = new Style(typeof(DataGridCell));
            //e.Column.CellStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            //e.Column.CellStyle.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
            //e.Column.CellStyle.Setters.Add(new Setter(HeightProperty, Double.Parse("30")));
        }

        private void dtgCommandHistory_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            DataRowView row_selected = gd.SelectedItem as DataRowView;
            if (row_selected != null)
            {
                _deleteJobID = row_selected["CommandID"].ToString();
                _deleteJobCreateTime = DateTime.Parse(row_selected["JobCreat"].ToString());
                _jobState = row_selected["CommandStatus"].ToString();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            sever_x.Close();
            //PLC.SetDevice("D5200", 0);
            //PLC.SetDevice("M1", 0);
            PLC.Close();
        }

        string strcheck;
        byte[] checksum(string data)
        {
            char[] datachecksum = data.ToArray();
            int sum = 0;
            for (int j = 0; j < datachecksum.Length; j++)
            {
                sum += datachecksum[j];
            }
            int a = sum & 15;
            if (a < 10) a += 48;
            else a += 87;
            byte[] aa = { (byte)a };
            data += Encoding.ASCII.GetString(aa);
            strcheck = data;
            char[] dataxx = new char[14];
            for (int i = 1; i < 12; i++)
            {
                dataxx[i] = datachecksum[i - 1];
            }
            dataxx[0] = (char)0x02;
            dataxx[12] = (char)a;
            dataxx[13] = (char)0x03;
            byte[] bdata = new byte[14];
            for (int i = 0; i < 14; i++)
            {
                bdata[i] = (byte)dataxx[i];
            }
            return bdata;
        }
    }
}
