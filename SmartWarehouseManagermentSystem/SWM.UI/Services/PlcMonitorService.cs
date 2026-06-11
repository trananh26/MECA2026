using SWM.BL;
using System;

namespace SWM.UI.Services
{
    /// <summary>
    /// Chu kỳ đọc PLC (2s): cập nhật AGV, cảnh báo, trạng thái IP/OP, tiến độ lệnh, yêu cầu xuất HMI.
    /// </summary>
    internal sealed class PlcMonitorService
    {
        private readonly PlcService _plc;
        private readonly TransportCommandService _transport;

        private string _oldFullState = "EMPTY";
        private string _oldLocation = "2";
        private string _oldInputState;
        private string _oldOutputState;
        private string _oldAlarm;

        public event Action<string> AgvLocationChanged;
        public event Action LayoutRefreshRequested;

        public PlcMonitorService(PlcService plc, TransportCommandService transport)
        {
            _plc = plc;
            _transport = transport;
        }

        public void InitializeAgvState(string location, string fullState)
        {
            _oldLocation = location;
            _oldFullState = fullState;
            _transport.AgvLocation = location;
        }

        public void Poll()
        {
            try
            {
                // Theo dõi tiến độ lệnh đang chạy (JOB START → TRANSFERING → COMPLETE)
                _transport.UpdateCommandProgress();

                // AGV: vị trí D800, có hàng M510
                string agvFullState = _plc.GetDeviceInt("M510") == 1 ? "FULL" : "EMPTY";
                string agvLocation = _plc.GetDeviceInt("D800").ToString();
                _transport.AgvLocation = agvLocation;

                if (agvLocation != _oldLocation || agvFullState != _oldFullState)
                {
                    
                    BLUpdateAGVStatus.UpdateAGVStatus(_plc.AgvId, agvLocation, agvFullState);
                    AgvLocationChanged?.Invoke(agvLocation);
                    _oldFullState = agvFullState;
                    _oldLocation = agvLocation;
                }

                // Cảnh báo PLC → ghi DB khi thay đổi
                string alarm = _plc.GetDeviceInt("D2500").ToString();
                if (alarm != _oldAlarm)
                {
                    AlarmLog.LogAlarmToDatabase(alarm);
                    _oldAlarm = alarm;
                }

                // Cổng nhập IP01 (M2300) / xuất OP01 (M2301) → cập nhật layout
                string inputState = _plc.GetDeviceInt("M2300") == 1 ? "FULL" : "EMPTY";
                if (inputState != _oldInputState)
                {
                    BLLayout.UpdateInOutState(int.Parse(WarehouseConstants.InputPortId), inputState);
                    LayoutRefreshRequested?.Invoke();
                    _oldInputState = inputState;
                }

                string outputState = _plc.GetDeviceInt("M2301") == 1 ? "FULL" : "EMPTY";
                if (outputState != _oldOutputState)
                {
                    BLLayout.UpdateInOutState(int.Parse(WarehouseConstants.OutputPortId), outputState);
                    LayoutRefreshRequested?.Invoke();
                    _oldOutputState = outputState;
                }

                // HMI bấm xuất: D2350 > 0 → tạo lệnh xuất slot FULL đầu tiên
                _transport.HandleHmiOutputRequest(_plc.GetDeviceInt("D2350"));
            }
            catch (Exception)
            {
            }
        }
    }
}
