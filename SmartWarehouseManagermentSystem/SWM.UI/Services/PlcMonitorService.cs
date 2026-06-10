using SWM.BL;
using System;

namespace SWM.UI.Services
{
    internal sealed class PlcMonitorService
    {
        private readonly PlcService _plc;
        private readonly TransportCommandService _transport;

        private string _oldFullState = "EMPTY";
        private string _oldLocation = "5";
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
                _transport.UpdateCommandProgress();

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

                string alarm = _plc.GetDeviceInt("D2500").ToString();
                if (alarm != _oldAlarm)
                {
                    AlarmLog.LogAlarmToDatabase(alarm);
                    _oldAlarm = alarm;
                }

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

                _transport.HandleHmiOutputRequest(_plc.GetDeviceInt("D2350"));
            }
            catch (Exception)
            {
            }
        }
    }
}
