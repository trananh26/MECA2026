using SWM.UI.Config;
using System;
using System.IO.Ports;
using System.Windows;

namespace SWM.UI.Services
{
    /// <summary>
    /// Cổng COM, tin kết thúc 'x': C1x=nhập kho; CMx=quay băng tải; C2x=quay ngược CV03_IP02.
    /// </summary>
    internal sealed class SerialCommunicationService : IDisposable
    {
        private readonly SerialPort _port = new SerialPort();

        public bool IsConnected => _port.IsOpen;
        public string PortName => _port.PortName;
        public int BaudRate => _port.BaudRate;

        public event Action ImportRequested;
        public event Action ConveyorCommandReceived;
        public event Action Cv03ReverseCommandReceived;
        public event Action<string> ErrorOccurred;

        public void Connect()
        {
            _port.PortName = AppConfiguration.Current.Serial.PortName;
            _port.BaudRate = AppConfiguration.Current.Serial.BaudRate;
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

        public bool SendMessage(string body)
        {
            try
            {
                if (!_port.IsOpen)
                    return false;

                _port.Write(body + "x");
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex.ToString());
                return false;
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
            if (string.Equals(data, "C1", StringComparison.OrdinalIgnoreCase))
            {
                ImportRequested?.Invoke();
                return;
            }

            if (string.Equals(data, "CM", StringComparison.OrdinalIgnoreCase))
            {
                ConveyorCommandReceived?.Invoke();
                return;
            }

            if (string.Equals(data, "C2", StringComparison.OrdinalIgnoreCase))
                Cv03ReverseCommandReceived?.Invoke();
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
