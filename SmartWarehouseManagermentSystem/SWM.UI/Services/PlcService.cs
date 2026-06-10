using ActUtlTypeLib;
using SWM.BL;
using SWM.Common;
using System;
using System.Net.NetworkInformation;
using System.Windows;

namespace SWM.UI.Services
{
    internal sealed class PlcService : IDisposable
    {
        private readonly ActUtlType _plc = new ActUtlType();
        private int _aliveToggle;

        public string AgvId { get; } = WarehouseConstants.AgvId;
        public string IpAddress { get; }

        public PlcService(string ipAddress)
        {
            IpAddress = ipAddress;
        }

        public bool Connect()
        {
            try
            {
                _plc.ActLogicalStationNumber = WarehouseConstants.PlcStationNumber;
                _plc.Open();
                BLUpdateAGVStatus.UpdateAGVStatus(AgvId, "5", "EMPTY");
                MessageBox.Show("Kết nối PLC thành công", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        public void SendAlivePulse()
        {
            _aliveToggle++;
            if (_aliveToggle >= 100)
                _aliveToggle = 0;

            SetDevice("M845", _aliveToggle % 2 == 0 ? 1 : 0);
        }

        public void Ping()
        {
            try
            {
                PingReply reply = new Ping().Send(IpAddress);
                if (reply.Status != IPStatus.Success)
                {
                    AlarmLog.LogAlarmToDatabase("04");
                    MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("04");
                MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void StartConveyorIn() => SetDevice("M2100", 1);

        public void StartConveyorOut() => SetDevice("M2200", 1);

        public void StopConveyor()
        {
            SetDevice("M2200", 0);
            SetDevice("M2301", 0);
            SetDevice("Y4", 0);
        }

        public void ResetHmiOutputRequest() => SetDevice("D2350", 0);

        public int GetDeviceInt(string address)
        {
            int value;
            _plc.GetDevice(address, out value);
            return value;
        }

        public void SetDevice(string address, int value) => _plc.SetDevice(address, value);

        public void ApplyJobToPlc(CurrentTransportCommand job)
        {
            SetDevice("M2000", 0);

            int commandType;
            if (job.CommandSourceID == WarehouseConstants.InputPortId && job.CommandDestID != WarehouseConstants.OutputPortId)
                commandType = 1;
            else if (job.CommandDestID == WarehouseConstants.OutputPortId && job.CommandSourceID != WarehouseConstants.InputPortId)
                commandType = 2;
            else if (job.CommandSourceID == WarehouseConstants.InputPortId && job.CommandDestID == WarehouseConstants.OutputPortId)
                commandType = 3;
            else
                commandType = 4;

            SetDevice("D3000", commandType);
            SetDevice("M2000", 1);
            SetDevice("D2100", int.Parse(job.CommandSourceID));
            SetDevice("D2150", int.Parse(job.CommandDestID));
        }

        public void Dispose()
        {
            try
            {
                _plc.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
