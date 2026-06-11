using SWM.UI.Config;
using System;
using System.IO.Ports;
using System.Windows;

namespace SWM.UI.Services
{
    /// <summary>
    /// Cổng COM: tin kết thúc bằng 'x' — "1"/"2" băng tải, "C1x" yêu cầu lấy hàng IP01 (khi băng tải đã có hàng).
    /// </summary>
    internal sealed class SerialCommunicationService : IDisposable
    {
        private readonly SerialPort _port = new SerialPort();
        public event Action ImportRequested;
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

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_port.BytesToRead > 500)
                {
                    _port.DiscardInBuffer();
                    return;
                }

                // Ví dụ "C1x" → ReadTo("x") trả về "C1"
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
            if (IsImportRequest(data))
                ImportRequested?.Invoke();          // C1x → kiểm tra IP01 FULL rồi tạo lệnh nhập kho
        }

        private static bool IsImportRequest(string data)
        {
            return string.Equals(data, "C1", StringComparison.OrdinalIgnoreCase);
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
