# Memory Bank — SWM

Bộ tài liệu ngữ cảnh để agent (và người) nhanh chóng nắm dự án **Smart Warehouse Management System**.
Đọc theo thứ tự dưới đây; mỗi file xây trên file trước.

## Thứ tự đọc

1. [`projectbrief.md`](projectbrief.md) — nền tảng: mục tiêu, phạm vi, thành phần.
2. [`productContext.md`](productContext.md) — vì sao tồn tại, luồng nghiệp vụ, trải nghiệm.
3. [`systemPatterns.md`](systemPatterns.md) — kiến trúc, service, mẫu thiết kế, cảnh báo concurrency.
4. [`techContext.md`](techContext.md) — công nghệ, cấu hình, tag PLC, timer, build/run.
5. [`activeContext.md`](activeContext.md) — trọng tâm hiện tại, việc đang làm, quyết định gần đây.
6. [`progress.md`](progress.md) — cái gì chạy, cái gì còn dở, hạn chế đã biết.

## Quy ước cập nhật

- Khi đổi **tag PLC** hoặc **giao thức serial** → cập nhật `systemPatterns.md` + `techContext.md`.
- Khi bắt đầu/đổi hướng công việc → cập nhật `activeContext.md`.
- Khi hoàn thành/phát hiện vấn đề → cập nhật `progress.md`.
- Tài liệu hệ thống chi tiết (nguồn gốc): [`../docs/TAI_LIEU_HE_THONG.md`](../docs/TAI_LIEU_HE_THONG.md).
