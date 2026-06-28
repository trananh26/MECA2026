# Project Brief — Smart Warehouse Management System (SWM)

> Tài liệu nền tảng của Memory Bank. Mọi file khác bám theo file này.

## Mục tiêu

Phần mềm SCADA chạy trên PC điều phối **kho thông minh**: điều khiển AGV và băng tải
thông qua PLC Mitsubishi, đồng bộ trạng thái ô kho (Buffer / BF) với SQL Server, và
giao tiếp với thiết bị nhận dạng/robot bên ngoài qua cổng serial.

## Phạm vi cốt lõi

- Kết nối & đọc/ghi tag PLC qua MX Component (`ActUtlType`).
- Nhận lệnh serial từ thiết bị ngoài (`C1x`, `CMx`, `C2x`) và phản hồi (`COx`).
- Tạo và theo dõi **lệnh vận chuyển AGV** (nhập IP01→BF, xuất BF→OP01).
- Hiển thị map kho thời gian thực: ô BF trống/đầy, vị trí AGV, panel trạng thái kết nối.
- Ghi log cảnh báo và trạng thái xuống database.

## Ngoài phạm vi

- Phần mềm **không** tự nhận dạng vật/mã — chỉ nhận lệnh từ thiết bị ngoài qua serial.
- Không hỗ trợ lệnh AGV IP→OP trực tiếp hoặc di chuyển nội bộ BF↔BF.
- Logic reset/tắt băng tải (M701/M703) do PLC tự xử lý, không nằm trong code PC.

## Thành phần solution

| Project | Vai trò |
|---------|---------|
| `SWM.UI` | Giao diện WPF + các service điều phối (PLC, serial, lệnh). Entry point. |
| `SWM.BL` | Business logic: layout kho, lệnh vận chuyển, cảnh báo, báo cáo. |
| `SWM.DL` | Truy cập dữ liệu SQL Server. |
| `SWM.Common` | Model dùng chung (AGV, BFLayout, TransportCommand, InventoryAging…). |

## Nguồn tài liệu gốc

Tài liệu hệ thống chi tiết: `docs/TAI_LIEU_HE_THONG.md` (tiếng Việt, sinh từ mã nguồn).
Memory Bank này tóm tắt + bổ sung ngữ cảnh làm việc cho agent.
