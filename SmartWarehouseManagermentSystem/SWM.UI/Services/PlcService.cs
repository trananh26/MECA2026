using ActUtlTypeLib;
using SWM.BL;
using SWM.Common;
using SWM.UI.Config;
using System;
using System.Net.NetworkInformation;
using System.Windows;

namespace SWM.UI.Services
{
    /// <summary>
    /// Giao tiếp Mitsubishi PLC (ActUtlType): kết nối, đọc/ghi device, băng tải, gửi lệnh vận chuyển.
    /// </summary>
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

        // Mở kết nối PLC và khởi tạo trạng thái AGV trên DB
        public bool Connect()
        {
            try
            {
                _plc.ActLogicalStationNumber = AppConfiguration.Current.Plc.StationNumber;
                _plc.Open();
                BLUpdateAGVStatus.UpdateAGVStatus(AgvId, "2", "EMPTY");
                MessageBox.Show("Kết nối PLC thành công", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Không kết nối được với PLC. Vui lòng kiểm tra lại kết nối", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        // Nhịp sống M845 — báo phần mềm vẫn đang chạy
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

        // Xóa cờ yêu cầu xuất từ HMI sau khi đã tạo lệnh
        public void ResetHmiOutputRequest() => SetDevice("D2350", 0);

        // IP01 có hàng trên băng tải (PLC M2300 = 1)
        public bool IsInputPortFull() => GetDeviceInt("M2300") == 1;

        public int GetDeviceInt(string address)
        {
            int value;
            _plc.GetDevice(address, out value);
            return value;
        }

        public void SetDevice(string address, int value) => _plc.SetDevice(address, value);

        // Ghi lệnh vận chuyển lên PLC: loại lệnh (1=nhập, 2=xuất, 3=thẳng IP→OP, 4=nội bộ) + nguồn/đích
        public void ApplyJobToPlc(CurrentTransportCommand job)
        {
            SetDevice("M2000", 0);

            int commandType;
            if (job.CommandSourceID == WarehouseConstants.InputPortId && job.CommandDestID != WarehouseConstants.OutputPortId)
                commandType = 1; // nhập kho
            else if (job.CommandDestID == WarehouseConstants.OutputPortId && job.CommandSourceID != WarehouseConstants.InputPortId)
                commandType = 2; // xuất kho
            else if (job.CommandSourceID == WarehouseConstants.InputPortId && job.CommandDestID == WarehouseConstants.OutputPortId)
                commandType = 3; // IP01 → OP01
            else
                commandType = 4; // di chuyển nội bộ BF

            SetDevice("D3000", commandType);
            SetDevice("D2100", int.Parse(job.CommandSourceID));
            SetDevice("D2150", int.Parse(job.CommandDestID));
            SetDevice("M2000", 1); //bắt đầu thực hiện nhập/xuất/chuyển hàng
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
