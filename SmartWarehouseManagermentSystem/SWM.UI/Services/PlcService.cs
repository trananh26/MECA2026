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
        public bool IsConnected { get; private set; }
        public bool IsNetworkReachable { get; private set; }

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
                IsConnected = true;
                RefreshNetworkStatus();
                BLUpdateAGVStatus.UpdateAGVStatus(AgvId, "2", "EMPTY");
                MessageBox.Show("Kết nối PLC thành công", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception)
            {
                IsConnected = false;
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

        // Kiểm tra ping PLC (không popup — dùng cho panel trạng thái)
        public void RefreshNetworkStatus()
        {
            try
            {
                PingReply reply = new Ping().Send(IpAddress, 1000);
                bool online = reply.Status == IPStatus.Success;
                if (!online)
                    AlarmLog.LogAlarmToDatabase("04");
                IsNetworkReachable = online;
            }
            catch (Exception)
            {
                AlarmLog.LogAlarmToDatabase("04");
                IsNetworkReachable = false;
            }
        }

        // Xóa cờ yêu cầu xuất từ HMI sau khi đã tạo lệnh
        public void ResetHmiOutputRequest() => SetDevice("M33", 0);

        // IP01 có hàng trên băng tải (PLC M706 = 1)
        public bool IsInputPortFull() => GetDeviceInt("M706") == 1;

        // CV02_IO01 có hàng (PLC M708 = 1)
        public bool IsCv02Io01Full() => GetDeviceInt("M708") == 1;

        // PLC bật M709 → SCADA gửi COx
        public bool IsCoAckReady() => GetDeviceInt("M709") == 1;

        // CMx: quay băng tải nhập (CV02)
        public void StartConveyorRotation() => SetDevice("M701", 1);

        // C2x: quay ngược băng tải CV03_IP02
        public void StartCv03Ip02ReverseRotation() => SetDevice("M703", 1);

        public int GetDeviceInt(string address)
        {
            int value;
            _plc.GetDevice(address, out value);
            return value;
        }

        // D800: nếu > 4 thì trừ 4 để map về node trên sơ đồ kho
        public int GetAgvLocation()
        {
            int raw = GetDeviceInt("D800");
            return raw > 4 ? raw - 4 : raw;
        }

        public void SetDevice(string address, int value) => _plc.SetDevice(address, value);

        // Ghi lệnh lên PLC: 1=IP01→BF (D500=ô đích), 2=BF→OP01 (D500=ô nguồn)
        public bool ApplyJobToPlc(CurrentTransportCommand job)
        {
            if (!TryGetBufferSlotId(job, out int bufferSlotId, out int commandType))
                return false;
                        
            SetDevice("D502", commandType);
            SetDevice("D500", bufferSlotId);
            SetDevice("M39", 1);
            SetDevice("M39", 0);
            return true;
        }

        internal static bool TryGetBufferSlotId(CurrentTransportCommand job, out int bufferSlotId, out int commandType)
        {
            bufferSlotId = 0;
            commandType = 0;

            if (job.CommandSourceID == WarehouseConstants.InputPortId
                && job.CommandDestID != WarehouseConstants.OutputPortId
                && job.CommandDestID != WarehouseConstants.InputPortId)
            {
                commandType = 1;
                bufferSlotId = int.Parse(job.CommandDestID);
                return true;
            }

            if (job.CommandDestID == WarehouseConstants.OutputPortId
                && job.CommandSourceID != WarehouseConstants.InputPortId
                && job.CommandSourceID != WarehouseConstants.OutputPortId)
            {
                commandType = 2;
                bufferSlotId = int.Parse(job.CommandSourceID);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            try
            {
                _plc.Close();
                IsConnected = false;
            }
            catch (Exception)
            {
            }
        }
    }
}
