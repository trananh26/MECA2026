# Progress — trạng thái hoạt động

> Cập nhật: 2026-06-28

## Đã hoạt động

- Kết nối PLC qua `ActUtlType` (logical station từ config) + ping mạng định kỳ.
- Mở cổng serial, parse lệnh nhận `C1x` / `CMx` / `C2x`, gửi `COx`.
- Tạo & theo dõi lệnh AGV: nhập (IP01→BF), xuất (BF→OP01) với vòng đời JOB.
- Nguồn lệnh: serial `C1x`, HMI PLC `M33`, Manual Control, hàng đợi tự động (timer 2s).
- Map kho WPF: ô BF trống/đầy, vị trí AGV, thời gian tồn kho, panel SYSTEM MONITOR.
- Ghi log cảnh báo (`D2500`, mất mạng "04") xuống DB.
- Alive pulse `M845` (0.5s).

## Mới thêm (cần kiểm thử thực tế)

- **X30 → CAPx**: rising-edge gửi `CAPx` một lần mỗi lần X30 ON.
  - Phụ thuộc: `X30` đọc được qua MX Component; cổng serial đang mở.

## Đang nghi vấn / cần điều tra

- **CMx "không được thực hiện"**: chưa chốt nguyên nhân (xem `activeContext.md`). Nghi nhất là
  cờ `_waitingForCoAck` chặn CM mới khi `M709` chưa lên, hoặc xung đột COM đa luồng do thêm đọc X30.

## Hạn chế đã biết

- `ActUtlType` không thread-safe nhưng đang bị truy cập từ nhiều luồng (UI timer + serial thread).
- Mã trả về (HRESULT) của `GetDevice`/`SetDevice` bị bỏ qua → lỗi đọc/ghi PLC âm thầm.
- Không có cơ chế reset `M701`/`M703` phía PC (giao cho PLC tự xử lý).
- `appsettings.json` có thay đổi cục bộ chưa commit — dễ lệch giữa môi trường.

## Việc có thể làm tiếp

- Xác nhận `X30` đọc được; cân nhắc lock quanh truy cập `_plc`.
- Kiểm tra mã trả về của `SetDevice("M701")` và log khi ghi thất bại.
- Bổ sung điều kiện thoát cờ `_waitingForCoAck` (timeout) nếu `M709` không lên.
