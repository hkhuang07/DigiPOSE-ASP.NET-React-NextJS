DIGIPOSE - MASTER ARCHITECTURE & FUNCTIONAL BLUEPRINT
I. TẦM NHÌN VÀ ĐỊNH VỊ DỰ ÁN (PROJECT VISION)
DigiPOSE (Digital Point of Sale Enterprise) không phải là một website thương mại điện tử (B2C) đơn thuần. Đây là một hệ thống Phần mềm Quản trị Nguồn lực & Điểm bán hàng tập trung (ERP/PoS - B2B). Hệ thống được thiết kế để giải quyết bài toán vận hành của các chuỗi bán lẻ, siêu thị hoặc hệ thống phân phối đa chi nhánh.

Mục tiêu cốt lõi: Tốc độ giao dịch tại quầy đạt mức mili-giây (O(1) lookup), toàn vẹn dữ liệu lịch sử tuyệt đối, bảo mật đa tầng và sẵn sàng mở rộng theo chiều ngang (Scale-out).

II. KIẾN TRÚC HỆ THỐNG HYBRID (DECOUPLED ARCHITECTURE)
Dự án ứng dụng kiến trúc kết hợp, chia cắt rõ ràng ranh giới giữa Quản trị viên và Vận hành viên.

Khu vực Admin CMS (Server-Side Rendering):

Công nghệ: ASP.NET Core MVC 8.0 (Razor Views).

Mục đích: Xây dựng giao diện nhập liệu siêu tốc cho Quản lý / Admin bằng Scaffolding. Xử lý các tác vụ quản trị danh mục (CRUD Master Data), cấu hình hệ thống, xem báo cáo tổng quan.

Xác thực (Auth): Sử dụng Cookie Authentication (Stateful) với vòng đời theo ca làm việc.

Khu vực Point of Sale & Frontend (RESTful Web API):

Công nghệ: ASP.NET Core Web API, trả về chuẩn JSON. Tiền đề để tích hợp với Frontend React.js/Next.js tại quầy.

Mục đích: Xử lý các tác vụ có tần suất cao và yêu cầu tốc độ như Quét mã vạch (Barcode Scanner), Lập đơn nháp, Thanh toán, Kiểm kho.

Xác thực (Auth): Sử dụng JWT (JSON Web Token) (Stateless) truyền qua header Authorization: Bearer.

III. THIẾT KẾ CƠ SỞ DỮ LIỆU ĐA TẦNG (16 TABLES - 3NF)
Hệ thống sử dụng SQL Server và Entity Framework Core, được chuẩn hóa 3NF và chia thành 4 Bounded Contexts. Toàn bộ hệ thống bị vô hiệu hóa tính năng Cascade Delete bằng Fluent API (DeleteBehavior.Restrict) để bảo vệ dữ liệu lịch sử.

1. Phân hệ Cấu Hình & Nhân Sự (IAM & Org)
1. Branches (Chi nhánh): Mỏ neo của toàn bộ dữ liệu. Mọi giao dịch, nhân viên, tồn kho đều phải gắn với một chi nhánh.

2. Roles (Vai trò): Quản lý phân quyền (Admin, Branch Manager, Cashier).

3. Users (Nhân sự): Chứa thông tin đăng nhập, PasswordHash (mã hóa BCrypt), liên kết khóa ngoại với BranchId và RoleId.

4. Counters (Quầy thu ngân): Máy PoS vật lý tại chi nhánh (BranchId).

5. Shifts (Ca làm việc): Quản lý dòng tiền phiên giao dịch. Lưu thời gian mở/đóng, StartCash (Tiền đầu ca), EndCash (Tiền cuối ca). Liên kết với UserId và CounterId.

2. Phân hệ Đối Tác (Partners)
6. CustomerTypes (Loại khách hàng): Phân loại Khách lẻ, VIP, Doanh nghiệp.

7. Customers (Khách hàng): Quản lý định danh qua Số điện thoại, lưu trữ RewardPoints (Điểm thưởng CRM).

8. Suppliers (Nhà cung cấp): Thông tin đối tác nhập hàng, quản lý DebtBalance (Công nợ).

3. Phân hệ Từ Điển & Tồn Kho (Catalog & Inventory)
9. Categories (Danh mục): Phân loại hàng hóa.

10. Units (Đơn vị tính): Cái, Hộp, Kg.

11. Products (Hàng hóa cốt lõi): Chứa SKU (Mã vạch - Unique Index), BasePrice. Tích hợp [Timestamp] RowVersion để chống xung đột dữ liệu (Optimistic Concurrency).

12. ProductInventories (Tồn kho thực tế): Bảng trung gian giải quyết bài toán đa chi nhánh (1 Sản phẩm - N Chi nhánh). Chứa StockQuantity (Tồn thực tế) và MinStockLevel (Mức cảnh báo).

13. StockVouchers (Chứng từ kho): Lưu thông tin nhập/xuất/kiểm kê. Chứa VoucherType (Import, Export, Return), TotalValue, liên kết với SupplierId (nếu là phiếu nhập).

14. StockVoucherDetails (Chi tiết chứng từ): Cấu trúc cha-con với StockVouchers, lưu Quantity và ActualPrice (Giá nhập thực tế tại thời điểm lập phiếu).

4. Phân hệ Giao Dịch Bán Hàng (Sales Core)
15. Orders (Hóa đơn/Đơn hàng): Giao dịch tổng. Trạng thái (Draft, Completed, Cancelled). Liên kết với ShiftId, UserId và CustomerId.

16. OrderDetails (Chi tiết hóa đơn): Lưu các dòng sản phẩm bán ra. Ràng buộc sinh tử: Giá bán (UnitPrice) và Tên ĐVT (UnitName) phải được COPY cứng từ Products sang đây tại thời điểm bán để bảo toàn báo cáo doanh thu nếu giá gốc thay đổi trong tương lai.

IV. BẢN ĐỒ RÀNG BUỘC QUAN HỆ (ENTITY RELATIONSHIPS)
(1 - N):

Branch -> User, Counter, ProductInventory, StockVoucher.

Role -> User.

User -> Shift, StockVoucher, Order.

Counter -> Shift.

Shift -> Order (Toàn bộ doanh thu trong ca được tổng hợp từ đây).

CustomerType -> Customer.

Customer -> Order.

Supplier -> StockVoucher.

Category -> Product.

Unit -> Product.

Product -> ProductInventory, StockVoucherDetail, OrderDetail.

StockVoucher -> StockVoucherDetail.

Order -> OrderDetail.

V. ĐẶC TẢ YÊU CẦU CHỨC NĂNG CHI TIẾT (FUNCTIONAL REQUIREMENTS)
1. Phân hệ Quản lý Hàng hóa & Tồn kho (Catalog & Inventory Management)
Quản lý Danh mục & Sản phẩm: CRUD Sản phẩm, thiết lập Mã vạch (SKU), Đơn giá, Đơn vị tính. Quét mã vạch nhanh bằng API GET /api/v1/products/scan-barcode/{sku} trả về kết quả O(1).

Quản lý Nhập Kho (Import): Lập StockVoucher với Type = "Import". Bắt buộc liên kết với SupplierId. Khi phiếu hoàn tất, hệ thống kích hoạt transaction cộng dồn StockQuantity vào bảng ProductInventories tương ứng với BranchId. Tính toán lại trung bình giá vốn.

Quản lý Xuất Kho / Hủy hàng (Export/Dispose): Lập StockVoucher với Type = "Export". Không cần SupplierId. Trừ StockQuantity trực tiếp khỏi kho.

Quản lý Trả hàng (Return): Khách hàng hoàn trả hoặc xuất trả Nhà cung cấp. Sử dụng StockVoucher với Type = "Return", kèm theo tham chiếu ghi chú.

Quản lý Đợt hàng (Batch Management qua Chứng từ): Hệ thống không dùng bảng "Lô/Đợt" độc lập để tránh thắt cổ chai hiệu năng. Thay vào đó, "Đợt hàng" được quản lý trực tiếp thông qua Mã Chứng từ nhập kho (StockVoucherId). Có thể truy xuất chính xác đợt hàng nhập ngày nào, của nhà cung cấp nào, số lượng bao nhiêu thông qua báo cáo chứng từ.

2. Phân hệ Quản lý Ca làm việc & Quầy (Shift & Counter Management)
Quản lý Quầy: Định danh các trạm thu ngân vật lý tại từng chi nhánh.

Đăng ký & Mở Ca làm việc (Open Shift): Thu ngân đăng nhập bằng JWT, chọn Quầy (CounterId), nhập Số tiền mặt hiện có trong két (StartCash) để tạo mới một Shift (Trạng thái: "Open").

Đóng ca & Kết toán (Close Shift): Khi hết ca, hệ thống tính tổng doanh thu từ tất cả Orders thuộc ShiftId đó. Cộng với StartCash sinh ra EndCash (Tiền mặt phải có trên lý thuyết). Thu ngân kiểm đếm tiền thực tế, cập nhật trạng thái ca thành "Closed".

3. Phân hệ Bán hàng tại quầy (Point of Sale Operations)
Tạo Đơn Nháp (Draft Order): Thu ngân quét mã vạch sản phẩm. API tạo một Order với trạng thái "Draft". Mọi cập nhật số lượng (Tăng/Giảm/Xóa món) đều tác động vào OrderDetails của Đơn nháp này. Không dùng Session giỏ hàng, giúp hệ thống không bị mất phiên làm việc nếu sập điện.

Đặt hàng / Khách mua (Checkout):

Thu ngân chọn phương thức thanh toán.

Hệ thống gắn CustomerId (Nếu có) để tích lũy RewardPoints.

Chuyển trạng thái Order sang "Completed".

Khởi chạy DB Transaction: Trừ tồn kho (ProductInventories), Ghi nhận doanh thu vào Shift.

Phát hành Hóa đơn (E-Invoice): Tích hợp dịch vụ SMTP MailKit. Sau khi Checkout thành công, trigger luồng Background gửi Biên lai điện tử dạng HTML thẳng vào Email của khách hàng thông qua thông tin từ bảng Customers.

4. Phân hệ Hệ thống Truyền thông & Khách hàng (CRM & Internal System)
Quản trị Tệp Khách hàng (CRM): Cập nhật hạng thành viên, tra cứu lịch sử mua hàng xuyên suốt hệ thống (Lấy 5 Order gần nhất bằng API).

Bảng tin Nội bộ (Announcements): (Thay thế chức năng Blog/Tin tức). Admin sử dụng CKEditor để viết các thông báo vận hành (Khuyến mãi mới, thay đổi chính sách). API đẩy thông báo có cờ IsUrgent lên màn hình máy PoS của nhân viên.

5. Phân hệ Báo cáo & Thống kê (Reporting & Analytics)
Công nghệ áp dụng: Sử dụng thư viện System.Linq.Dynamic.Core kết hợp Server-side DataTables.net.

Thống kê Doanh thu: Gom nhóm (GroupBy) theo BranchId, UserId (Nhân viên), hoặc khoảng thời gian (Ngày/Tháng). Tính tổng FinalTotal từ bảng Orders.

Thống kê Hàng hóa: Báo cáo tồn kho dựa trên ProductInventories. Lọc các sản phẩm có StockQuantity <= MinStockLevel để cảnh báo nhập hàng.

Tra soát Chứng từ: Tìm kiếm động mọi phiếu Nhập/Xuất kho theo khoảng thời gian, theo Nhà cung cấp, hoặc theo ID chứng từ.

VI. TIÊU CHUẨN KỸ THUẬT VÀ BẢO MẬT (TECHNICAL & SECURITY STANDARDS)
Phòng chống Deadlock (Concurrency): Bảng Products được gắn mỏ neo [Timestamp] byte[] RowVersion. Bất kỳ giao dịch nào sửa đổi giá gốc sẽ bị chặn (DbUpdateConcurrencyException) nếu có giao dịch khác đang thực thi đồng thời.

Toàn vẹn Lịch sử (Immutable History): UnitPrice và UnitName lưu dạng snapshot trong OrderDetails và StockVoucherDetails. Không bao giờ có sự cố truy vấn ngược làm thay đổi doanh thu năm cũ.

Mã hóa & Xác thực kép:

Mật khẩu 100% băm bằng BCrypt.

Cookie Auth cho Admin / Quản lý (Trượt phiên sau 8 tiếng).

JWT Bearer cho Frontend PoS (Có thời hạn, cấp qua api/v1/auth/login).

Transaction Nguyên khối (ACID): Quá trình thanh toán (Checkout) được gói trong IDbContextTransaction. Nếu việc trừ tồn kho thành công nhưng cộng tiền vào ca làm việc thất bại, toàn bộ quá trình sẽ Rollback, không sinh ra rác dữ liệu. Dữ liệu chuẩn bị sẵn sàng cho các hạ tầng CI/CD, GitOps sau này.