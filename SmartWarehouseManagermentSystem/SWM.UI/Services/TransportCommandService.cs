using SWM.BL;
using SWM.Common;
using System;
using System.Data;

namespace SWM.UI.Services
{
    internal sealed class TransportCommandService
    {
        private readonly PlcService _plc;
        private int _oldOutputRequest;

        public CurrentTransportCommand CurrentJob { get; } = new CurrentTransportCommand();
        public string AgvLocation { get; set; } = "5";

        public event Action CommandsChanged;
        public event Action LayoutChanged;

        public TransportCommandService(PlcService plc)
        {
            _plc = plc;
        }

        public void CreateImportCommand()
        {
            try
            {
                DataTable dtEmptyBf = BLLayout.LoadEmptyBF();
                if (dtEmptyBf.Rows.Count == 0)
                    return;

                InsertCommand(
                    WarehouseConstants.InputPortName,
                    dtEmptyBf.Rows[0]["BFNAME"].ToString(),
                    WarehouseConstants.InputPortId,
                    dtEmptyBf.Rows[0]["BFID"].ToString(),
                    string.Empty);

                BLLayout.UpdateTrayID(WarehouseConstants.InputPortId, string.Empty);
                NotifyCommandsChanged();
            }
            catch (Exception)
            {
            }
        }

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

        public void HandleHmiOutputRequest(int outputRequest)
        {
            if (outputRequest <= 0)
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

                if (jobStatus == "JOB CREATE" && (AgvLocation == "0" || AgvLocation == "1"))
                {
                    DataTable dtLayout = BLLayout.LoadLayoutConfig();
                    DataRow[] drSource = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandSource + "%'");
                    DataRow[] drDest = dtLayout.Select("BFNAME LIKE '%" + CurrentJob.CommandDest + "%'");

                    if (drSource[0].ItemArray[2].ToString() == "EMPTY"
                        || (CurrentJob.CommandDestID != WarehouseConstants.OutputPortId && drDest[0].ItemArray[2].ToString() == "FULL"))
                    {
                        CurrentJob.CommandID = currentCommandId;
                        CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                        BLTransportCommand.DeleteJob(CurrentJob.CommandID, CurrentJob.JobCreat);
                    }
                    else
                    {
                        StartJob(dtCommand.Rows[0]);
                    }

                    NotifyCommandsChanged();
                }
                else if (jobStatus != "JOB CREATE")
                {
                    _plc.SetDevice("M2000", 0);
                    CurrentJob.CommandID = currentCommandId;
                    CurrentJob.CommandStatus = jobStatus;
                    CurrentJob.JobCreat = DateTime.Parse(dtCommand.Rows[0]["JobCreat"].ToString());
                    CurrentJob.JobAssign = DateTime.Parse(dtCommand.Rows[0]["JobAssign"].ToString());
                    _plc.ApplyJobToPlc(CurrentJob);
                    BLTransportCommand.UpdateCommandStatus(CurrentJob);
                    BLUpdateAGVStatus.UpdateAGVCommand(_plc.AgvId, CurrentJob.CommandID);
                    NotifyCommandsChanged();
                }
            }
            catch (Exception)
            {
            }
        }

        public void UpdateCommandProgress()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentJob.CommandSourceID) || string.IsNullOrEmpty(CurrentJob.CommandDestID))
                    return;

                int completeState = _plc.GetDeviceInt("M3000");
                int location = _plc.GetDeviceInt("D800");
                int portSource = CurrentJob.CommandSourceID == WarehouseConstants.InputPortId
                    ? 1
                    : 1 + int.Parse(CurrentJob.CommandSourceID.Substring(2, 1));
                int portDest = CurrentJob.CommandDestID == WarehouseConstants.OutputPortId
                    ? 1
                    : 1 + int.Parse(CurrentJob.CommandDestID.Substring(2, 1));

                if (location == portSource && CurrentJob.CommandStatus == "JOB START")
                {
                    _plc.SetDevice("M2000", 1);
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
            _plc.ApplyJobToPlc(CurrentJob);
            BLTransportCommand.UpdateCommandStatus(CurrentJob);
            BLUpdateAGVStatus.UpdateAGVCommand(_plc.AgvId, CurrentJob.CommandID);
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
