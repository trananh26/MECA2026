# Product Context — vì sao dự án tồn tại

## Vấn đề giải quyết

Kho cần một lớp điều phối (SCADA) giữa **thiết bị vật lý** (PLC, AGV, băng tải, robot/thiết bị
nhận dạng) và **dữ liệu nghiệp vụ** (trạng thái ô kho, lệnh nhập/xuất, tồn kho). Nếu thiếu lớp này,
việc nhập/xuất hàng phải thao tác thủ công và không có bức tranh tổng thể theo thời gian thực.

## SWM làm gì cho người dùng

- **Operator** nhìn map kho trực quan: ô nào trống/đầy, AGV đang ở đâu, thời gian tồn của từng ô.
- Tự động tạo lệnh AGV khi có tín hiệu (serial `C1x`, HMI PLC `M33`, hoặc thao tác Manual Control).
- Phản hồi thiết bị ngoài (`COx`) đúng thời điểm băng tải sẵn sàng.
- Cảnh báo khi mất kết nối PLC / mạng, ghi log cảnh báo xuống DB.

## Luồng nghiệp vụ chính (nhập hàng)

1. Thiết bị ngoài gửi `CMx` → PC ghi `M701=1` (quay băng tải CV02).
2. Khi `M708=1` (CV02_IO01 có hàng) rồi `M709=1` → PC gửi `COx` xác nhận.
3. Băng tải đưa hàng tới IP01 (`M706=1`).
4. Thiết bị ngoài gửi `C1x` → PC tạo lệnh AGV `IP01 → ô BF trống`.
5. AGV chạy theo vòng đời `JOB CREATE → START → TRANSFERING → COMPLETE`; ô BF chuyển sang FULL, cập nhật DB + UI.

## Luồng xuất hàng

- HMI PLC tăng `M33` → PC tạo lệnh xuất ô FULL đầu tiên (`BF → OP01`), rồi reset `M33`.
- Hoặc operator chọn thủ công `Ô BF → OP01` trong Manual Control.

## Trải nghiệm kỳ vọng

- Khởi động là tự kết nối PLC + mở serial, hiển thị trạng thái rõ ràng trên panel SYSTEM MONITOR.
- Map cập nhật gần thời gian thực (poll 0.5s).
- Thao tác Manual Control bị giới hạn đúng 2 loại lệnh hợp lệ để tránh sai sót.
