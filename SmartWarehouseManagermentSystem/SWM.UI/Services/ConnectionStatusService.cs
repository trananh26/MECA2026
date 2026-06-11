using SWM.BL;
using SWM.UI.Config;
using System;

namespace SWM.UI.Services
{
    internal sealed class ConnectionStatusInfo
    {
        public DateTime LastUpdated { get; set; }

        public bool PlcConnected { get; set; }
        public bool PlcNetworkOnline { get; set; }
        public string PlcIp { get; set; }
        public int PlcStation { get; set; }

        public bool SerialConnected { get; set; }
        public string SerialPort { get; set; }
        public int SerialBaudRate { get; set; }

        public bool DatabaseConnected { get; set; }

        public string AgvId { get; set; }
        public string AgvLocation { get; set; }
        public string AgvLoadState { get; set; }

        public string InputPortState { get; set; }
        public string OutputPortState { get; set; }

        public string CurrentCommandId { get; set; }
        public string CurrentCommandStatus { get; set; }
        public string CurrentCommandRoute { get; set; }
    }

    /// <summary>Thu thập trạng thái kết nối PLC, Serial, DB và thiết bị cho panel dashboard.</summary>
    internal sealed class ConnectionStatusService
    {
        public ConnectionStatusInfo Collect(PlcService plc, SerialCommunicationService serial, TransportCommandService transport)
        {
            var info = new ConnectionStatusInfo
            {
                LastUpdated = DateTime.Now,
                PlcConnected = plc.IsConnected,
                PlcNetworkOnline = plc.IsNetworkReachable,
                PlcIp = plc.IpAddress,
                PlcStation = AppConfiguration.Current.Plc.StationNumber,
                SerialConnected = serial.IsConnected,
                SerialPort = serial.PortName,
                SerialBaudRate = serial.BaudRate,
                AgvId = plc.AgvId,
                AgvLocation = transport.AgvLocation,
                CurrentCommandId = transport.CurrentJob.CommandID ?? "—",
                CurrentCommandStatus = string.IsNullOrEmpty(transport.CurrentJob.CommandStatus) ? "—" : transport.CurrentJob.CommandStatus,
                CurrentCommandRoute = BuildRoute(transport)
            };

            try
            {
                BLLayout.LoadLayoutConfig();
                info.DatabaseConnected = true;
            }
            catch (Exception)
            {
                info.DatabaseConnected = false;
            }

            if (plc.IsConnected)
            {
                try
                {
                    info.AgvLoadState = plc.GetDeviceInt("M510") == 1 ? "FULL" : "EMPTY";
                    info.InputPortState = plc.GetDeviceInt("M2300") == 1 ? "FULL" : "EMPTY";
                    info.OutputPortState = plc.GetDeviceInt("M2301") == 1 ? "FULL" : "EMPTY";
                }
                catch (Exception)
                {
                    info.AgvLoadState = "—";
                    info.InputPortState = "—";
                    info.OutputPortState = "—";
                }
            }
            else
            {
                info.AgvLoadState = "—";
                info.InputPortState = "—";
                info.OutputPortState = "—";
            }

            return info;
        }

        private static string BuildRoute(TransportCommandService transport)
        {
            var job = transport.CurrentJob;
            if (string.IsNullOrEmpty(job.CommandSource) || string.IsNullOrEmpty(job.CommandDest))
                return "—";

            return job.CommandSource + " → " + job.CommandDest;
        }
    }
}
