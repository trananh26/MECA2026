using SWM.Common;
using System;
using System.IO.Ports;
using System.Windows;

namespace SWM.UI.Services
{
    internal sealed class SerialCommunicationService : IDisposable
    {
        private readonly SerialPort _port = new SerialPort();

        public event Action ConveyorInRequested;
        public event Action ConveyorOutRequested;
        public event Action ImportRequested;
        public event Action<string> ErrorOccurred;

        public void Connect()
        {
            _port.PortName = clsFileIO.ReadValue("COM_SWMPORT");
            _port.BaudRate = int.Parse(clsFileIO.ReadValue("BAURATE"));
            _port.DataReceived += OnDataReceived;

            try
            {
                if (!_port.IsOpen)
                    _port.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Không kết nối được cổng serial. Vui lòng kiểm tra lại cấu hình COM.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_port.BytesToRead > 500)
                {
                    _port.DiscardInBuffer();
                    return;
                }

                string data = _port.ReadTo("x").Trim();
                ProcessMessage(data);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex.ToString());
            }
        }

        private void ProcessMessage(string data)
        {
            if (data == "1")
                ConveyorInRequested?.Invoke();
            else if (data == "2")
                ConveyorOutRequested?.Invoke();
            else if (data.StartsWith("C1"))
                ImportRequested?.Invoke();
        }

        public void Dispose()
        {
            try
            {
                _port.DataReceived -= OnDataReceived;
                if (_port.IsOpen)
                    _port.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
