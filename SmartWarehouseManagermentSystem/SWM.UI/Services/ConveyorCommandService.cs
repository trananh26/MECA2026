using System;
using System.Threading;

namespace SWM.UI.Services
{
    /// <summary>
    /// Nhận CMx → M701=1, gửi COx khi M709=1 (M708 báo có hàng tại CV02_IO01).
    /// Nhận C2x → M703=1 (quay ngược CV03_IP02).
    /// </summary>
    internal sealed class ConveyorCommandService
    {
        private readonly SerialCommunicationService _serial;
        private readonly PlcService _plc;
        private int _waitingForCoAck;
        private int _m708StatusReported;

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
            if (Interlocked.CompareExchange(ref _waitingForCoAck, 0, 0) != 1)
                return;

            if (_plc.IsCv02Io01Full() && Interlocked.CompareExchange(ref _m708StatusReported, 0, 0) == 0)
            {
                Interlocked.Exchange(ref _m708StatusReported, 1);
                SetStatus("M708=1 (CV02_IO01 có hàng), chờ M709 ON để gửi COx...");
            }

            if (!_plc.IsCoAckReady())
                return;

            SendCoAck();
        }

        private void OnConveyorCommandReceived()
        {
            if (Interlocked.CompareExchange(ref _waitingForCoAck, 0, 0) == 1)
            {
                SetStatus("Đang chờ M709 — bỏ qua CM mới.");
                return;
            }

            if (!_plc.IsConnected)
            {
                SetStatus("Nhận CM: PLC chưa kết nối — không set M701.");
                return;
            }

            _plc.StartConveyorRotation();

            if (_plc.IsCoAckReady())
            {
                SendCoAck();
                return;
            }

            Interlocked.Exchange(ref _m708StatusReported, 0);
            Interlocked.Exchange(ref _waitingForCoAck, 1);
            SetStatus("Nhận CM → M701=1, chờ M709 ON để gửi COx...");
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
            Interlocked.Exchange(ref _waitingForCoAck, 0);
            Interlocked.Exchange(ref _m708StatusReported, 0);

            if (!_serial.SendMessage("CO"))
            {
                SetStatus("M709 ON nhưng không gửi được COx.");
                return;
            }

            SetStatus("M709 ON → đã gửi COx.");
        }

        private void SetStatus(string message) => StatusChanged?.Invoke(message);
    }
}
