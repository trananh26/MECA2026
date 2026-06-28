# Active Context — trọng tâm hiện tại

> Cập nhật: 2026-06-28

## Việc đang làm

### 1. Tính năng X30 → CAPx (mới thêm)
Yêu cầu: đọc tín hiệu `X30`; khi `X30` OFF→ON thì gửi serial `CAPx`, **chỉ 1 lần mỗi lần đổi trạng thái**.

Đã triển khai bằng mẫu rising edge trong `ConveyorCommandService`:
- Thêm field `private bool _lastX30;`
- Thêm method `PollCaptureSignal()`: đọc `X30`, nếu `x30On && !_lastX30` thì `_serial.SendMessage("CAP")`
  (gửi ra dây `CAPx`), cập nhật trạng thái qua `SetStatus`, rồi `_lastX30 = x30On`.
- Gọi `_conveyor.PollCaptureSignal()` ở cuối `PlcMonitorService.Poll()` (chu kỳ 0.5s).

**Chưa xác nhận:** `X30` có đọc được trực tiếp qua MX Component trên PLC này không. Nếu ngõ vào ánh xạ
sang relay M, cần đổi chuỗi `"X30"` sang địa chỉ tương ứng.

### 2. Đang điều tra: "CMx có vẻ không được thực hiện"
Các nguyên nhân khả dĩ (chưa chốt do thiếu thông tin triệu chứng từ người dùng):

1. **State machine chờ ACK** (khả năng cao nhất): sau CMx đầu tiên, `_waitingForCoAck=1` và mọi CMx
   sau bị bỏ qua ("Đang chờ M709 — bỏ qua CM mới.") cho tới khi `M709` lên → gửi `COx` → reset cờ.
   Nếu `M709` không lên thì CM kế tiếp luôn bị nuốt. Đây là hành vi có sẵn, không do thay đổi X30.
2. **Xung đột COM đa luồng**: lệnh đọc `X30` mỗi 0.5s trên luồng UI có thể đụng `SetDevice("M701")`
   trên luồng nền serial; `ActUtlType` không thread-safe → ghi M701 có thể âm thầm fail (mã lỗi bị bỏ).
3. **`X30` không hợp lệ** → đọc lỗi/trả giá trị rác, có thể nhiễu hoặc flood `CAPx`.
4. PLC chưa kết nối → `OnConveyorCommandReceived` báo "PLC chưa kết nối — không set M701."

**Bước tiếp theo gợi ý:** xem chuỗi `lblConveyorStatus` lúc gửi CMx để xác định nhánh nào; cân nhắc
serialize truy cập `_plc` bằng `lock`, và/hoặc chỉ đọc `X30` khi đã xác nhận tag hợp lệ.

## Lưu ý quyết định

- Đặt logic X30 trong `ConveyorCommandService` vì service này đã có sẵn cả `_plc` và `_serial`.
- Giao thức serial tự thêm hậu tố `x`, nên gửi `"CAP"` → trên dây là `CAPx` (đồng nhất với `CO`→`COx`).

## Trạng thái git lúc bắt đầu phiên

- `M SmartWarehouseManagermentSystem/SWM.UI/config/appsettings.json`
- `?? haui/` (thư mục chưa theo dõi)
- Thay đổi X30/CAPx ở `ConveyorCommandService.cs` và `PlcMonitorService.cs` (chưa commit).
