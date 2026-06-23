using System;
using System.Threading;

namespace SWM.UI.Services
{
    /// <summary>
    /// Nhận CMx → M701=1, COx khi M2302=1. Nhận C2x → M703=1 (quay ngược CV03_IP02).
    /// </summary>
    internal sealed class ConveyorCommandService
    {
        private readonly SerialCommunicationService _serial;
        private readonly PlcService _plc;
        private int _waitingForCv02Full;

        public event Action<string> StatusChanged;

        public ConveyorCommandService(SerialCommunicationService serial, PlcService plc)
        {
            _serial = serial;
            _plc = plc;
            _serial.ConveyorCommandReceived += OnConveyorCommandReceived;
            _serial.Cv03ReverseCommandReceived += OnCv03ReverseCommandReceived;
        }

        public void PollPendingAck()
        {
            if (Interlocked.CompareExchange(ref _waitingForCv02Full, 0, 0) != 1)
                return;

            if (!_plc.IsCv02Io01Full())
                return;

            SendCoAck();
        }

        private void OnConveyorCommandReceived()
        {
            if (Interlocked.CompareExchange(ref _waitingForCv02Full, 0, 0) == 1)
            {
                SetStatus("Đang chờ CV02_IO01 có hàng — bỏ qua CM mới.");
                return;
            }

            if (!_plc.IsConnected)
            {
                SetStatus("Nhận CM: PLC chưa kết nối — không set M701.");
                return;
            }

            _plc.StartConveyorRotation();

            if (_plc.IsCv02Io01Full())
            {
                SendCoAck();
                return;
            }

            Interlocked.Exchange(ref _waitingForCv02Full, 1);
            SetStatus("Nhận CM → M701=1, chờ CV02_IO01 có hàng (M2302)...");
        }

        private void OnCv03ReverseCommandReceived()
        {
            if (!_plc.IsConnected)
            {
                SetStatus("Nhận C2: PLC chưa kết nối — không set M703.");
                return;
            }

            _plc.StartCv03Ip02ReverseRotation();
            SetStatus("Nhận C2 → M703=1 (quay ngược CV03_IP02).");
        }

        private void SendCoAck()
        {
            Interlocked.Exchange(ref _waitingForCv02Full, 0);

            if (!_serial.SendMessage("CO"))
            {
                SetStatus("CV02_IO01 có hàng nhưng không gửi được COx.");
                return;
            }

            SetStatus("CV02_IO01 có hàng (M2302=1) → đã gửi COx.");
        }

        private void SetStatus(string message) => StatusChanged?.Invoke(message);
    }
}
