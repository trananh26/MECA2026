# Tech Context — công nghệ, cấu hình, vận hành

## Nền tảng

- **Ngôn ngữ/Framework:** C# trên **.NET Framework 4.8**, ứng dụng **WPF**.
- **Solution:** `SmartWarehouseManagermentSystem.sln` (4 project: SWM.UI, SWM.BL, SWM.DL, SWM.Common).
- **Entry point:** `SWM.UI` → `App.xaml.cs` (gọi `AppConfiguration.Load()`), `MainWindow.xaml.cs`.

## Phụ thuộc ngoài

| Thành phần | Ghi chú |
|------------|---------|
| Mitsubishi **MX Component** (`ActUtlTypeLib.ActUtlType`) | Bắt buộc cài trên PC; cấu hình logical station. |
| **SQL Server Express** | Catalog mặc định `SmartWarehouse` trên `.\SQLExpress`. |
| **Cổng COM** | Thiết bị nhận dạng/robot (mặc định COM23 @ 115200). |
| `System.IO.Ports.SerialPort` | Giao tiếp serial. |

## Cấu hình — `SWM.UI/config/appsettings.json`

> File được copy ra `bin/Debug/config/` khi build. Đọc bằng `AppConfiguration.Load()`.

Giá trị hiện tại trong repo:

| Section | Key | Giá trị hiện tại |
|---------|-----|------------------|
| Database | ConnectionString | `Data Source=.\SQLExpress;Initial Catalog=SmartWarehouse;Integrated Security=True` |
| Plc | IpAddress | `192.168.3.80` |
| Plc | StationNumber | `25` |
| Serial | PortName | `COM23` |
| Serial | BaudRate | `115200` |
| Warehouse | AgvId | `105` |
| Warehouse | StkId | `B1STK01` |
| Warehouse | InputPortId / OutputPortId | `1` / `11` |

> Lưu ý: `appsettings.json` đang có thay đổi cục bộ (git `M`) — kiểm tra giá trị thực trước khi chạy.

## Chu kỳ timer (trong `MainWindow.StartTimers()`)

| Timer | Chu kỳ | Chức năng |
|-------|--------|-----------|
| commandTimer | 2s | `TransportCommandService.ProcessPendingCommands()` |
| plcAliveTimer | 0.5s | `PlcService.SendAlivePulse()` (toggle M845) |
| **monitorTimer** | **0.5s** | `PlcMonitorService.Poll()` + cập nhật panel kết nối |
| pingTimer | 5s | `PlcService.RefreshNetworkStatus()` (ping IP PLC) |

> Tài liệu cũ ghi Poll "2s" nhưng **mã nguồn thực tế là 0.5s**.

## Bảng tag PLC

### Đọc (PLC → PC)
`M706` IP01 có hàng · `M707` OP01 có hàng · `M708` CV02_IO01 có hàng · `M709` cờ gửi COx ·
`M510` AGV FULL · `D800` vị trí AGV (raw>5 thì −4) · `M3000` hoàn thành lệnh ·
`M33` yêu cầu xuất HMI · `D2500` mã cảnh báo.
`X30` ngõ vào — đọc để phát `CAPx` (xem activeContext; cần xác nhận đọc được qua MX Component).

### Ghi (PC → PLC)
`M701` quay CV02 (CMx) · `M703` quay ngược CV03_IP02 (C2x) · `M39` cờ gửi lệnh AGV (tự reset sau 3s) ·
`D502` loại lệnh (1=nhập,2=xuất) · `D500` BFID · `M845` alive pulse.

## Khởi động / build

1. Chỉnh `config/appsettings.json` (PLC IP, COM, connection string).
2. Build `SmartWarehouseManagermentSystem.sln` (Visual Studio, .NET Framework 4.8).
3. Chạy `SWM.UI.exe` — tự kết nối PLC và mở serial khi khởi động.

## Môi trường dev

- OS: Windows (win32 10.0.19045), shell PowerShell.
- Git repo gốc: `D:/HaUI_SEEE/MECA2026`; thư mục làm việc: `SmartWarehouseManagermentSystem/`.
