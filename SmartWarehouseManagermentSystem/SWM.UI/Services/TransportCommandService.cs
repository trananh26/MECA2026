using SWM.BL;
using SWM.Common;
using System;
using System.Data;

namespace SWM.UI.Services
{
    internal enum ImportCommandResult
    {
        Success,
        InputPortEmpty,
        NoEmptySlot,
        Error
    }

    /// <summary>
    /// Luồng lệnh vận chuyển: tạo lệnh (nhập/xuất/HMI/thủ công) → gán AGV → gửi PLC → cập nhật BF/DB.
    /// </summary>
    internal sealed class TransportCommandService
    {
        private readonly PlcService _plc;
        private int _oldOutputRequest;

        public CurrentTransportCommand CurrentJob { get; } = new CurrentTransportCommand();
        public string AgvLocation { get; set; } = "2";

        public event Action CommandsChanged;
        public event Action LayoutChanged;

        public TransportCommandService(PlcService plc)
        {
            _plc = plc;
        }

        // Serial C1x + IP01 FULL (M706): lấy hàng từ băng tải nhập → ô BF trống đầu tiên
        public ImportCommandResult CreateImportCommand()
        {
            try
            {
                if (!_plc.IsInputPortFull())
                    return ImportCommandResult.InputPortEmpty;

                DataTable dtEmptyBf = BLLayout.LoadEmptyBF();
                if (dtEmptyBf.Rows.Count == 0)
                    return ImportCommandResult.NoEmptySlot;

                InsertCommand(
                    WarehouseConstants.InputPortName,
                    dtEmptyBf.Rows[0]["BFNAME"].ToString(),
                    WarehouseConstants.InputPortId,
                    dtEmptyBf.Rows[0]["BFID"].ToString(),
                    string.Empty);

                BLLayout.UpdateTrayID(WarehouseConstants.InputPortId, string.Empty);
                NotifyCommandsChanged();
                return ImportCommandResult.Success;
            }
            catch (Exception)
            {
                return ImportCommandResult.Error;
            }
        }

        // Xuất: ô BF đầy đầu tiên → OP01
        public void CreateExportCommand(DataTable fullSlots)
        {
            if (fullSlots.Rows.Count == 0)
                return;

            InsertCommand(
                fullSlots.Rows[0]["BFNAME"].ToString(),
                WarehouseConstants.OutputPortName,
                fullSlots.Rows[0]["BFID"].ToString(),
                WarehouseConstants.OutputPortId,
                fullSlots.Rows[0]["TRAYID"].ToString());

            NotifyCommandsChanged();
        }

        // HMI ghi M33: mỗi lần tăng giá trị tạo một lệnh xuất rồi reset M33
        public void HandleHmiOutputRequest(int outputRequest)
        {
            if (outputRequest == 0)
            {
                _oldOutputRequest = 0;
                return;
            }

            if (outputRequest == _oldOutputRequest)
                return;

            CreateExportCommand(BLLayout.LoadFullBF());
            _plc.ResetHmiOutputRequest();
            _oldOutputRequest = outputRequest;
        }

        // Timer 2s: lấy lệnh JOB CREATE đầu hàng, gán AGV khi ở node 0/1, hoặc đẩy lệnh đang chạy lên PLC
        public void ProcessPendingCommands()
        {
            try
            {
                DataTable dtCommand = BLReport.GetTransportCommand();
                if (dtCommand.Rows.Count == 0)
                    return;

                string currentCommandId = dtCommand.Rows[0]["CommandID"].ToString();
                string jobStatus = dtCommand.Rows[0]["CommandStatus"].ToString();
                if (currentCommandId == CurrentJob.CommandID)
                    return;

                LoadCurrentJobFromRow(dtCommand.Rows[0]);

                // AGV ở node sạc/đợi mới nhận lệnh mới
                if (jobStatus == "JOB CREATE" && (AgvLocation == "0" || AgvLocation == "1"))
                {
                    DataTable dtLayout = BLLayout.LoadLayoutConfig();
                    DataRow[] drSource = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandSource + "%'");
                    DataRow[] drDest = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandDest + "%'");

                    if (!PlcService.TryGetBufferSlotId(CurrentJob, out _, out _))
                    {
                        CancelPendingJob(currentCommandId, dtCommand.Rows[0]);
                    }
                    else if (drSource[0].ItemArray[2].ToString() == "EMPTY"
                        || (IsImportJob(CurrentJob) && drDest[0].ItemArray[2].ToString() == "FULL"))
                    {
                        CancelPendingJob(currentCommandId, dtCommand.Rows[0]);
                    }
                    else
                    {
                        StartJob(dtCommand.Rows[0]);
                    }

                    NotifyCommandsChanged();
                }
                else if (jobStatus != "JOB CREATE")
                {
                    CurrentJob.CommandID = currentCommandId;
                    CurrentJob.CommandStatus = jobStatus;
                    CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                    CurrentJob.JobAssign = DateTime.Parse(dtCommand.Rows[0]["JobAssign"].ToString());

                    if (_plc.ApplyJobToPlc(CurrentJob))
                    {
                        BLTransportCommand.UpdateCommandStatus(CurrentJob);
                        BLUpdateAGVStatus.UpdateAGVCommand(_plc.AgvId, CurrentJob.CommandID);
                    }

                    NotifyCommandsChanged();
                }
            }
            catch (Exception)
            {
            }
        }

        // Đọc vị trí AGV (D800): tới nguồn → chuyển TRANSFERING; tới đích hoặc M3000=1 → JOB COMPLETE
        public void UpdateCommandProgress()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentJob.CommandSourceID) || string.IsNullOrEmpty(CurrentJob.CommandDestID))
                    return;

                int completeState = _plc.GetDeviceInt("M3000");
                int location = _plc.GetAgvLocation();
                int portSource = CurrentJob.CommandSourceID == WarehouseConstants.InputPortId
                    ? 1
                    : 1 + int.Parse(CurrentJob.CommandSourceID.Substring(2, 1));
                int portDest = CurrentJob.CommandDestID == WarehouseConstants.OutputPortId
                    ? 1
                    : 1 + int.Parse(CurrentJob.CommandDestID.Substring(2, 1));

                if (location == portSource && CurrentJob.CommandStatus == "JOB START")
                {
                    CurrentJob.CommandStatus = "TRANSFERING DEST";
                    BLTransportCommand.UpdateCommandStatus(CurrentJob);
                    BLLayout.UpdateBFStateByStep(CurrentJob.CommandSourceID, "EMPTY");
                    NotifyLayoutChanged();
                    NotifyCommandsChanged();
                }
                else if ((location == portDest || completeState == 1) && CurrentJob.CommandStatus == "TRANSFERING DEST")
                {
                    CurrentJob.CommandStatus = "JOB COMPLETE";
                    CurrentJob.JobComplete = DateTime.Now;
                    BLTransportCommand.UpdateCommandStatus(CurrentJob);
                    BLLayout.UpdateBFStateByStep(CurrentJob.CommandDestID, "FULL");
                    BLLayout.UpdateTrayID(CurrentJob.CommandDestID, CurrentJob.TrayID);
                    NotifyCommandsChanged();
                    NotifyLayoutChanged();
                }
            }
            catch (Exception)
            {
            }
        }

        public void DeleteJob(string commandId, DateTime jobCreateTime, string jobState)
        {
            if (jobState != "JOB CREATE")
                return;

            BLTransportCommand.DeleteJob(commandId, jobCreateTime);
            NotifyCommandsChanged();
        }

        public DataTable LoadCommandHistory() => BLReport.GetTransportCommand();

        public DataTable LoadJobCount() => BLReport.GetTransportJobCount();

        public DataTable LoadBufferDetail() => BLReport.GetBFDetail();

        private void StartJob(DataRow commandRow)
        {
            CurrentJob.CommandID = commandRow["CommandID"].ToString();
            CurrentJob.CommandStatus = "JOB START";
            CurrentJob.JobCreat = DateTime.Parse(commandRow["JobCreat"].ToString());
            CurrentJob.JobAssign = DateTime.Now;

            if (!_plc.ApplyJobToPlc(CurrentJob))
            {
                BLTransportCommand.DeleteJob(CurrentJob.CommandID, CurrentJob.JobCreat);
                return;
            }

            BLTransportCommand.UpdateCommandStatus(CurrentJob);
            BLUpdateAGVStatus.UpdateAGVCommand(_plc.AgvId, CurrentJob.CommandID);
        }

        private static bool IsImportJob(CurrentTransportCommand job) =>
            job.CommandSourceID == WarehouseConstants.InputPortId;

        private void CancelPendingJob(string commandId, DataRow commandRow)
        {
            CurrentJob.CommandID = commandId;
            CurrentJob.JobCreat = DateTime.Parse(commandRow["JobCreat"].ToString());
            BLTransportCommand.DeleteJob(CurrentJob.CommandID, CurrentJob.JobCreat);
        }

        private void LoadCurrentJobFromRow(DataRow row)
        {
            CurrentJob.AGVID = row["AGVID"].ToString();
            CurrentJob.STKID = row["STKID"].ToString();
            CurrentJob.CommandSource = row["CommandSource"].ToString();
            CurrentJob.CommandSourceID = row["CommandSourceID"].ToString();
            CurrentJob.CommandDest = row["CommandDest"].ToString();
            CurrentJob.CommandDestID = row["CommandDestID"].ToString();
            CurrentJob.TrayID = row["TrayID"].ToString();
            CurrentJob.ProductID = row["ProductID"].ToString();
        }

        private static void InsertCommand(string source, string dest, string sourceId, string destId, string trayId)
        {
            TransportCommand transport = new TransportCommand
            {
                AGVID = WarehouseConstants.AgvId,
                STKID = WarehouseConstants.StkId,
                CommandID = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + source + "_" + dest,
                CommandSource = source,
                CommandDest = dest,
                CommandSourceID = sourceId,
                CommandDestID = destId,
                CommandStatus = "JOB CREATE",
                JobStart = DateTime.Now,
                TrayID = trayId
            };

            BLTransportCommand.InsertTransportCommand(transport);
        }

        private void NotifyCommandsChanged() => CommandsChanged?.Invoke();

        private void NotifyLayoutChanged() => LayoutChanged?.Invoke();
    }
}
