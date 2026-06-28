# System Patterns — kiến trúc & quy ước kỹ thuật

## Kiến trúc tổng thể

```
Serial / HMI PLC / Manual Control
        ↓
TransportCommandService  (tạo & quản lý lệnh vận chuyển)
        ↓
PlcService               (đọc/ghi tag PLC qua ActUtlType)
        ↓
PlcMonitorService.Poll() (poll định kỳ, phát hiện đổi trạng thái)
        ↓
MainWindow (WPF)         (map kho, AGV, panel trạng thái)
```

- UI là WPF. `MainWindow` khởi tạo tất cả service, nối event service → UI bằng `Dispatcher.Invoke`.
- Service nằm trong `SWM.UI/Services`; business logic thuần ở `SWM.BL` (các class `BL*`).

## Các service chính (`SWM.UI/Services`)

| Service | Trách nhiệm |
|---------|-------------|
| `PlcService` | Kết nối ActUtlType; `GetDeviceInt`/`SetDevice`; helper M701/M703/M845/M39; ping mạng. |
| `SerialCommunicationService` | Mở COM, parse tin nhận (`C1`/`CM`/`C2`), gửi tin (`body + "x"`). |
| `ConveyorCommandService` | `CMx`→M701, chờ M709→gửi `COx`; `C2x`→M703. Giữ state máy chờ ACK. |
| `PlcMonitorService` | `Poll()` định kỳ: AGV, IP/OP, alarm, M33, tiến độ JOB, ack băng tải. |
| `TransportCommandService` | CRUD lệnh, vòng đời JOB, đẩy lệnh xuống PLC. |
| `ConnectionStatusService` | Tổng hợp trạng thái PLC/Serial/DB cho panel UI. |

## Mẫu thiết kế quan trọng

### 1. Phát hiện đổi trạng thái bằng biến `_old*`
`PlcMonitorService` so sánh giá trị đọc với biến lưu trước đó, chỉ hành động khi thay đổi
(ví dụ `_oldInputState`, `_oldOutputState`, `_oldAlarm`). Tránh ghi DB / refresh UI dư thừa.

### 2. Phát hiện cạnh lên (rising edge) cho bit
Để "chỉ làm 1 lần mỗi lần ON", dùng mẫu:
```csharp
bool on = _plc.GetDeviceInt("<addr>") == 1;
if (on && !_last) { /* hành động một lần */ }
_last = on;
```
Đang dùng cho `X30 → CAPx` trong `ConveyorCommandService.PollCaptureSignal()`.

### 3. State machine chờ ACK (CM/CO)
`ConveyorCommandService` dùng cờ `_waitingForCoAck` (Interlocked) để **chặn CM mới** cho tới khi
`M709` lên và đã gửi `COx`. Đây là điểm dễ gây hiểu nhầm "CM không chạy" — xem `activeContext.md`.

### 4. Giao thức serial khung `x`
- Gửi: `_port.Write(body + "x")` → ví dụ `"CO"` thành `COx`.
- Nhận: `_port.ReadTo("x").Trim()` rồi so khớp chuỗi (OrdinalIgnoreCase).
- Thêm tin mới: sửa `SerialCommunicationService.ProcessMessage` (nhận) hoặc gọi `SendMessage` (gửi).

### 5. Tag PLC dùng tên thiết bị dạng chuỗi
Mọi truy cập PLC qua tên Mitsubishi: M (relay), D (thanh ghi), X (ngõ vào)…
`GetDevice`/`SetDevice` của `ActUtlType` **bỏ qua mã trả về (HRESULT)** trong code hiện tại.

## Cảnh báo đồng thời (concurrency) — QUAN TRỌNG

`ActUtlType` (MX Component) **không an toàn đa luồng**. Hiện có nhiều luồng cùng truy cập một
instance `_plc`:
- Luồng UI (DispatcherTimer): `Poll()` 0.5s, alive pulse 0.5s.
- Luồng nền của `SerialPort` (`OnDataReceived`): `StartConveyorRotation()` (SetDevice M701)…
- `Timer` reset M39 (threadpool).

→ Khi sửa logic đọc/ghi PLC, cân nhắc khả năng xung đột COM. Một lệnh đọc tag lỗi/chậm trên luồng
này có thể khiến lệnh ghi trên luồng khác âm thầm thất bại. Cân nhắc serialize bằng `lock` quanh
truy cập `_plc` nếu mở rộng.

## Vòng đời lệnh AGV

```
JOB CREATE → JOB START → TRANSFERING DEST → JOB COMPLETE
```
- `JOB CREATE`: AGV ở node 0/1 → gán lệnh, ghi PLC (`D502`, `D500`, `M39`) → `JOB START`.
- `JOB START`: AGV tới node nguồn → `TRANSFERING DEST`, nguồn → EMPTY.
- `TRANSFERING DEST`: AGV tới đích **hoặc** `M3000=1` → `JOB COMPLETE`, đích → FULL.
- Chỉ 2 loại: nhập (`D502=1`, D500=BF đích) và xuất (`D502=2`, D500=BF nguồn).
