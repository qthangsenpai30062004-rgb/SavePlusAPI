# SavePlus API - Hệ thống Y tế Hậu Điều Trị

## 📋 Giới thiệu

SavePlus API là backend hoàn chỉnh cho hệ sinh thái y tế hậu điều trị của **Neosix**, bao gồm:

- **📱 Mobile App**: Dành cho bệnh nhân theo dõi tiến trình điều trị, đặt lịch khám, tư vấn online
- **🏥 Web Admin**: Sản phẩm chính để bán cho cơ sở y tế (phòng khám/bệnh viện)
- **🔄 Multi-tenant**: Nhiều phòng khám có giao diện khác nhau nhưng kết nối cùng 1 app thống nhất
- **📊 Dashboard Analytics**: Hệ thống báo cáo và thống kê toàn diện

## 🎯 Tổng Kết Dự Án

### ✅ **ĐÃ HOÀN THÀNH (90% tính năng cốt lõi)**

SavePlus API đã triển khai **13 modules chính** với **150+ API endpoints**, bao gồm:

- **🔐 Authentication & Authorization** - Hệ thống xác thực đa cấp
- **👥 User & Patient Management** - Quản lý người dùng và bệnh nhân
- **🏢 Multi-tenant System** - Hỗ trợ nhiều phòng khám
- **📅 Appointment Management** - Quản lý lịch hẹn hoàn chỉnh
- **🩺 Medical Consultations** - Quản lý buổi khám và chẩn đoán
- **💊 Prescription Management** - Kê đơn thuốc điện tử
- **📋 Care Plan System** - Kế hoạch chăm sóc chi tiết
- **📊 Health Measurements** - Theo dõi chỉ số sức khỏe
- **📄 Medical Records** - Quản lý hồ sơ y tế và file
- **🔔 Notification System** - Hệ thống thông báo đa kênh
- **⏰ Reminder System** - Nhắc nhở thông minh
- **💬 Chat/Messaging** - Tư vấn trực tuyến
- **📈 Dashboard Analytics** - Báo cáo và thống kê

## 🛠️ Tech Stack

- **Framework**: .NET 8.0 Web API
- **Database**: SQL Server + Entity Framework Core
- **Authentication**: JWT Bearer + HMACSHA512 password hashing
- **OTP System**: Memory Cache với SMS integration
- **File Storage**: Local file system với upload/download APIs
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Multi-tenant, Repository Pattern, Service Layer
- **Security**: Role-based access control với 6 roles cơ bản

## 🔐 Role System

### Core Roles (Đã triển khai)
- **`SystemAdmin`**: Quản trị toàn hệ thống, quản lý tenant
- **`ClinicAdmin`**: Quản trị phòng khám, toàn quyền trong tenant
- **`Doctor`**: Bác sĩ - tạo hồ sơ, chẩn đoán, kê đơn, tư vấn
- **`Nurse`**: Điều dưỡng - theo dõi CarePlan, nhập measurement
- **`Receptionist`**: Lễ tân - quản lý lịch hẹn, check-in
- **`Patient`**: Bệnh nhân - sử dụng mobile app

### Advanced Roles (Phase 2 - Future)
- **`CareCoordinator`**: Điều phối chăm sóc
- **`Scheduler`**: Chuyên viên lịch hẹn  
- **`Billing`**: Kế toán
- **`Caregiver`**: Người chăm sóc

---

# 📚 API Endpoints Chi Tiết

## 🔐 Authentication Controller
**Mục đích**: Xác thực người dùng, quản lý phiên đăng nhập

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/auth/request-otp` | Yêu cầu mã OTP cho bệnh nhân | Mobile app đăng nhập |
| `POST` | `/api/auth/verify-otp` | Xác thực OTP và đăng nhập bệnh nhân | Mobile app authentication |
| `POST` | `/api/auth/staff/request-otp` | Yêu cầu OTP cho nhân viên y tế | Staff OTP login |
| `POST` | `/api/auth/staff/verify-otp` | Xác thực OTP nhân viên | Staff authentication |
| `POST` | `/api/auth/staff/login` | Đăng nhập staff bằng email/password | Web admin login |
| `POST` | `/api/auth/validate-token` | Kiểm tra tính hợp lệ của JWT token | Token validation |
| `POST` | `/api/auth/forgot-password` | Yêu cầu reset mật khẩu | Password recovery |
| `POST` | `/api/auth/reset-password` | Reset mật khẩu mới | Password reset flow |
| `POST` | `/api/auth/change-password` | Đổi mật khẩu cho user đã đăng nhập | Account security |
| `POST` | `/api/auth/logout` | Đăng xuất (client-side token removal) | Logout functionality |

## 👥 Users Controller
**Mục đích**: Quản lý nhân viên y tế và phân quyền

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/users` | Tạo tài khoản nhân viên mới | Staff registration |
| `GET` | `/api/users/{id}` | Lấy thông tin chi tiết user | Profile management |
| `PUT` | `/api/users/{id}` | Cập nhật thông tin user | Profile editing |
| `DELETE` | `/api/users/{id}` | Vô hiệu hóa tài khoản user | Account deactivation |
| `GET` | `/api/users` | Danh sách users với phân trang | Staff directory |
| `GET` | `/api/users/email/{email}` | Tìm user theo email | User lookup |
| `GET` | `/api/users/phone/{phone}` | Tìm user theo số điện thoại | User search |
| `GET` | `/api/users/{id}/doctor-info` | Lấy thông tin bác sĩ chi tiết | Doctor profile |
| `GET` | `/api/users/tenant/{tenantId}` | Lấy users theo phòng khám | Tenant staff list |
| `POST` | `/api/users/{id}/change-password` | Đổi mật khẩu user | Password management |
| `GET` | `/api/users/check-email/{email}` | Kiểm tra email có tồn tại | Email validation |
| `GET` | `/api/users/check-phone/{phone}` | Kiểm tra phone có tồn tại | Phone validation |
| `GET` | `/api/users/roles` | Lấy danh sách roles và permissions | Role management |
| `POST` | `/api/users/{userId}/create-doctor` | Tạo Doctor record cho User | Doctor setup |
| `GET` | `/api/users/tenants/{tenantId}/doctors/search` | Tìm kiếm bác sĩ trong tenant | Doctor autocomplete |

## 👤 Patients Controller
**Mục đích**: Quản lý bệnh nhân và đăng ký tài khoản

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/patients/register` | Đăng ký bệnh nhân mới | Patient onboarding |
| `GET` | `/api/patients/{id}` | Lấy thông tin bệnh nhân | Patient profile |
| `PUT` | `/api/patients/{id}` | Cập nhật thông tin bệnh nhân | Profile editing |
| `GET` | `/api/patients` | Danh sách bệnh nhân với phân trang/tìm kiếm | Patient directory |
| `GET` | `/api/patients/phone/{phone}` | Tìm bệnh nhân theo số điện thoại | Patient lookup |
| `POST` | `/api/patients/{patientId}/enroll/{tenantId}` | Đăng ký bệnh nhân vào phòng khám | Clinic enrollment |
| `GET` | `/api/patients/search/clinics/{phone}` | Tìm bệnh nhân trong tất cả phòng khám | Cross-clinic search |
| `POST` | `/api/patients/login` | Đăng nhập bệnh nhân (deprecated) | Patient authentication |

## 🏢 Tenants Controller
**Mục đích**: Quản lý phòng khám/bệnh viện (Multi-tenant)

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/tenants` | Tạo phòng khám/bệnh viện mới | Clinic registration |
| `GET` | `/api/tenants/{id}` | Lấy thông tin phòng khám | Clinic details |
| `PUT` | `/api/tenants/{id}` | Cập nhật thông tin phòng khám | Clinic management |
| `GET` | `/api/tenants` | Danh sách phòng khám với phân trang | Clinic directory |
| `GET` | `/api/tenants/code/{code}` | Lấy tenant theo mã code | Tenant lookup |
| `GET` | `/api/tenants/{id}/stats` | Thống kê tổng quan phòng khám | Clinic analytics |
| `GET` | `/api/tenants/{id}/patients` | Danh sách bệnh nhân của phòng khám | Patient management |
| `GET` | `/api/tenants/{tenantId}/patients/search` | Tìm kiếm bệnh nhân trong phòng khám | Patient autocomplete |
| `GET` | `/api/tenants/{tenantId}/patients/{patientId}` | Thông tin bệnh nhân cụ thể | Patient details |
| `PUT` | `/api/tenants/{tenantId}/patients/{patientId}` | Cập nhật thông tin bệnh nhân trong phòng khám | Patient management |
| `GET` | `/api/tenants/{tenantId}/doctors` | Danh sách bác sĩ của phòng khám | Doctor directory |
| `GET` | `/api/tenants/{tenantId}/doctors/{doctorId}` | Thông tin bác sĩ cụ thể | Doctor details |

## 📅 Appointments Controller
**Mục đích**: Quản lý lịch hẹn và booking system

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/appointments` | Tạo lịch hẹn mới | Appointment booking |
| `GET` | `/api/appointments/{id}` | Lấy thông tin lịch hẹn | Appointment details |
| `PUT` | `/api/appointments/{id}` | Cập nhật lịch hẹn | Appointment editing |
| `DELETE` | `/api/appointments/{id}` | Hủy lịch hẹn | Appointment cancellation |
| `GET` | `/api/appointments` | Danh sách lịch hẹn với filter | Appointment management |
| `GET` | `/api/appointments/patient/{patientId}` | Lịch hẹn của bệnh nhân | Patient appointments |
| `GET` | `/api/appointments/doctor/{doctorId}` | Lịch hẹn của bác sĩ | Doctor schedule |
| `GET` | `/api/appointments/tenant/{tenantId}` | Lịch hẹn của phòng khám | Clinic schedule |
| `GET` | `/api/appointments/doctor/{doctorId}/availability` | Kiểm tra lịch trống của bác sĩ | Availability check |
| `GET` | `/api/appointments/doctor/{doctorId}/timeslots` | Lấy khung giờ trống | Time slot booking |
| `POST` | `/api/appointments/{id}/confirm` | Xác nhận lịch hẹn | Appointment confirmation |
| `POST` | `/api/appointments/{id}/start` | Bắt đầu cuộc hẹn | Appointment check-in |
| `POST` | `/api/appointments/{id}/complete` | Hoàn thành cuộc hẹn | Appointment completion |
| `GET` | `/api/appointments/today` | Lịch hẹn hôm nay | Today's schedule |

## 🩺 Consultations Controller
**Mục đích**: Quản lý buổi khám bệnh và chẩn đoán

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/consultations` | Tạo consultation mới | Medical consultation |
| `GET` | `/api/consultations/{id}` | Lấy thông tin consultation | Consultation details |
| `PUT` | `/api/consultations/{id}` | Cập nhật consultation | Medical record editing |
| `DELETE` | `/api/consultations/{id}` | Xóa consultation | Record management |
| `GET` | `/api/consultations` | Danh sách consultation với filter | Consultation history |
| `GET` | `/api/consultations/appointment/{appointmentId}` | Consultation theo appointment | Appointment follow-up |
| `GET` | `/api/consultations/patient/{patientId}` | Consultation của bệnh nhân | Patient medical history |
| `GET` | `/api/consultations/doctor/{doctorId}` | Consultation của bác sĩ | Doctor consultation history |
| `GET` | `/api/consultations/tenant/{tenantId}` | Consultation của phòng khám | Clinic medical records |
| `GET` | `/api/consultations/reports` | Báo cáo consultation | Medical reporting |
| `GET` | `/api/consultations/search` | Tìm kiếm consultation | Medical record search |
| `GET` | `/api/consultations/statistics` | Thống kê consultation | Medical analytics |
| `GET` | `/api/consultations/patient/{patientId}/latest` | Consultation gần nhất của bệnh nhân | Latest medical record |
| `GET` | `/api/consultations/diagnosis-codes` | Danh sách mã chẩn đoán đã sử dụng | Diagnosis management |

## 💊 Prescriptions Controller
**Mục đích**: Quản lý đơn thuốc điện tử

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/prescriptions` | Tạo đơn thuốc mới | Electronic prescribing |
| `GET` | `/api/prescriptions/{id}` | Lấy thông tin đơn thuốc | Prescription details |
| `PUT` | `/api/prescriptions/{id}` | Cập nhật đơn thuốc | Prescription editing |
| `DELETE` | `/api/prescriptions/{id}` | Xóa đơn thuốc | Prescription management |
| `GET` | `/api/prescriptions` | Danh sách đơn thuốc với filter | Prescription history |
| `GET` | `/api/prescriptions/patient/{patientId}` | Đơn thuốc của bệnh nhân | Patient medications |
| `GET` | `/api/prescriptions/patient/{patientId}/active` | Đơn thuốc đang hoạt động | Active medications |
| `GET` | `/api/prescriptions/doctor/{doctorId}` | Đơn thuốc của bác sĩ | Doctor prescriptions |
| `POST` | `/api/prescriptions/{prescriptionId}/items` | Thêm thuốc vào đơn | Medication management |
| `PUT` | `/api/prescriptions/items/{itemId}` | Cập nhật thuốc trong đơn | Medication editing |
| `DELETE` | `/api/prescriptions/items/{itemId}` | Xóa thuốc khỏi đơn | Medication removal |
| `GET` | `/api/prescriptions/{prescriptionId}/items` | Danh sách thuốc trong đơn | Prescription items |
| `GET` | `/api/prescriptions/popular-drugs` | Thuốc được kê nhiều nhất | Drug analytics |
| `GET` | `/api/prescriptions/metadata` | Thông tin về dạng thuốc, đường dùng | Prescription metadata |
| `GET` | `/api/prescriptions/doctor/{doctorId}/can-prescribe` | Kiểm tra quyền kê đơn | Authorization check |
| `POST` | `/api/prescriptions/quick` | Kê đơn nhanh | Quick prescribing |

## 📋 CarePlans Controller
**Mục đích**: Quản lý kế hoạch chăm sóc bệnh nhân

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/careplans` | Tạo kế hoạch chăm sóc mới | Care planning |
| `GET` | `/api/careplans/{id}` | Lấy thông tin kế hoạch chăm sóc | Care plan details |
| `PUT` | `/api/careplans/{id}` | Cập nhật kế hoạch chăm sóc | Care plan editing |
| `DELETE` | `/api/careplans/{id}` | Xóa kế hoạch chăm sóc | Care plan management |
| `GET` | `/api/careplans` | Danh sách kế hoạch chăm sóc | Care plan directory |
| `GET` | `/api/careplans/patient/{patientId}/active` | Kế hoạch đang hoạt động của bệnh nhân | Active care plans |
| `GET` | `/api/careplans/{id}/progress` | Tiến độ kế hoạch chăm sóc | Progress tracking |
| `GET` | `/api/careplans/patient/{patientId}/progress` | Tiến độ tất cả kế hoạch của bệnh nhân | Patient progress |
| `POST` | `/api/careplans/{carePlanId}/items` | Thêm item vào kế hoạch | Care plan items |
| `PUT` | `/api/careplans/items/{itemId}` | Cập nhật item kế hoạch | Item editing |
| `DELETE` | `/api/careplans/items/{itemId}` | Xóa item khỏi kế hoạch | Item removal |
| `GET` | `/api/careplans/{carePlanId}/items` | Danh sách items của kế hoạch | Care plan details |
| `POST` | `/api/careplans/items/log` | Ghi log thực hiện item | Activity logging |
| `GET` | `/api/careplans/logs` | Danh sách logs kế hoạch chăm sóc | Activity history |

## 📊 Measurements Controller
**Mục đích**: Quản lý chỉ số sức khỏe và theo dõi

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/measurements` | Tạo số liệu đo lường mới | Health monitoring |
| `GET` | `/api/measurements/{id}` | Lấy thông tin số liệu đo lường | Measurement details |
| `PUT` | `/api/measurements/{id}` | Cập nhật số liệu đo lường | Data correction |
| `DELETE` | `/api/measurements/{id}` | Xóa số liệu đo lường | Data management |
| `GET` | `/api/measurements` | Danh sách số liệu với filter | Health data history |
| `GET` | `/api/measurements/patient/{patientId}` | Số liệu đo lường của bệnh nhân | Patient health data |
| `GET` | `/api/measurements/patient/{patientId}/recent` | Số liệu gần đây của bệnh nhân | Recent measurements |
| `GET` | `/api/measurements/patient/{patientId}/stats` | Thống kê số liệu của bệnh nhân | Health analytics |
| `GET` | `/api/measurements/patient/{patientId}/stats/{type}` | Thống kê theo loại đo lường | Specific health metrics |
| `GET` | `/api/measurements/types` | Danh sách loại đo lường có sẵn | Measurement types |
| `POST` | `/api/measurements/quick` | Nhập số liệu nhanh | Quick data entry |

## 📄 MedicalRecords Controller
**Mục đích**: Quản lý hồ sơ y tế và file đính kèm

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/medicalrecords` | Tạo hồ sơ y tế mới | Medical documentation |
| `GET` | `/api/medicalrecords/{id}` | Lấy thông tin hồ sơ y tế | Record details |
| `PUT` | `/api/medicalrecords/{id}` | Cập nhật hồ sơ y tế | Record editing |
| `DELETE` | `/api/medicalrecords/{id}` | Xóa hồ sơ y tế | Record management |
| `GET` | `/api/medicalrecords` | Danh sách hồ sơ với filter | Medical records |
| `GET` | `/api/medicalrecords/patient/{patientId}` | Hồ sơ y tế của bệnh nhân | Patient records |
| `GET` | `/api/medicalrecords/tenant/{tenantId}` | Hồ sơ y tế của phòng khám | Clinic records |
| `GET` | `/api/medicalrecords/type/{recordType}` | Hồ sơ theo loại | Record filtering |
| `GET` | `/api/medicalrecords/reports` | Báo cáo hồ sơ y tế | Medical reporting |
| `GET` | `/api/medicalrecords/search` | Tìm kiếm hồ sơ y tế | Record search |
| `POST` | `/api/medicalrecords/upload` | Upload file và tạo hồ sơ | File management |
| `GET` | `/api/medicalrecords/{id}/download` | Tải file hồ sơ y tế | File download |
| `GET` | `/api/medicalrecords/patient/{patientId}/summary` | Tổng hợp hồ sơ bệnh nhân | Patient summary |
| `GET` | `/api/medicalrecords/record-types` | Danh sách loại hồ sơ | Record types |
| `GET` | `/api/medicalrecords/{id}/access-check` | Kiểm tra quyền truy cập | Access control |
| `GET` | `/api/medicalrecords/patient/{patientId}/latest` | Hồ sơ gần nhất của bệnh nhân | Latest records |
| `GET` | `/api/medicalrecords/statistics` | Thống kê hồ sơ y tế | Record analytics |
| `GET` | `/api/medicalrecords/pending-review` | Hồ sơ cần xem xét | Pending reviews |

## 🔔 Notifications Controller
**Mục đích**: Hệ thống thông báo đa kênh

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/notifications` | Tạo thông báo mới | Notification creation |
| `GET` | `/api/notifications/{id}` | Lấy thông tin thông báo | Notification details |
| `PUT` | `/api/notifications/{id}` | Cập nhật thông báo | Notification editing |
| `DELETE` | `/api/notifications/{id}` | Xóa thông báo | Notification management |
| `GET` | `/api/notifications` | Danh sách thông báo với filter | Notification history |
| `GET` | `/api/notifications/user/{userId}` | Thông báo của user | User notifications |
| `GET` | `/api/notifications/patient/{patientId}` | Thông báo của bệnh nhân | Patient notifications |
| `GET` | `/api/notifications/tenant/{tenantId}` | Thông báo của phòng khám | Clinic notifications |
| `POST` | `/api/notifications/{id}/mark-read` | Đánh dấu đã đọc | Read status |
| `POST` | `/api/notifications/mark-multiple-read` | Đánh dấu nhiều thông báo đã đọc | Bulk read |
| `POST` | `/api/notifications/mark-all-read` | Đánh dấu tất cả đã đọc | Mark all read |
| `POST` | `/api/notifications/bulk-send` | Gửi thông báo hàng loạt | Bulk notifications |
| `GET` | `/api/notifications/reports` | Báo cáo thông báo | Notification analytics |
| `GET` | `/api/notifications/unread-count` | Số lượng thông báo chưa đọc | Unread count |
| `GET` | `/api/notifications/search` | Tìm kiếm thông báo | Notification search |
| `GET` | `/api/notifications/summary` | Tổng hợp thông báo | Notification summary |
| `POST` | `/api/notifications/send-from-template` | Gửi thông báo từ template | Template notifications |
| `GET` | `/api/notifications/channel/{channel}` | Thông báo theo kênh | Channel filtering |
| `DELETE` | `/api/notifications/cleanup` | Xóa thông báo cũ | Cleanup operations |
| `GET` | `/api/notifications/channels` | Danh sách kênh thông báo | Channel management |
| `POST` | `/api/notifications/appointment-reminder/{appointmentId}` | Gửi nhắc nhở cuộc hẹn | Appointment reminders |
| `POST` | `/api/notifications/test-result` | Gửi thông báo kết quả xét nghiệm | Test result notifications |
| `GET` | `/api/notifications/statistics` | Thống kê thông báo | Notification analytics |
| `GET` | `/api/notifications/recent-unread` | Thông báo chưa đọc gần đây | Recent notifications |
| `GET` | `/api/notifications/templates` | Danh sách template thông báo | Notification templates |

## ⏰ Reminders Controller
**Mục đích**: Hệ thống nhắc nhở thông minh

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/reminders` | Tạo nhắc nhở mới | Reminder creation |
| `GET` | `/api/reminders` | Danh sách nhắc nhở với filter | Reminder management |
| `GET` | `/api/reminders/{reminderId}` | Thông tin chi tiết nhắc nhở | Reminder details |
| `PUT` | `/api/reminders/{reminderId}` | Cập nhật nhắc nhở | Reminder editing |
| `DELETE` | `/api/reminders/{reminderId}` | Xóa nhắc nhở | Reminder removal |
| `POST` | `/api/reminders/{reminderId}/snooze` | Hoãn nhắc nhở | Snooze functionality |
| `POST` | `/api/reminders/{reminderId}/activate` | Kích hoạt nhắc nhở | Reminder activation |
| `POST` | `/api/reminders/{reminderId}/deactivate` | Tắt nhắc nhở | Reminder deactivation |
| `POST` | `/api/reminders/bulk-action` | Thao tác hàng loạt | Bulk operations |
| `GET` | `/api/reminders/upcoming` | Nhắc nhở sắp tới | Upcoming reminders |
| `GET` | `/api/reminders/overdue` | Nhắc nhở quá hạn | Overdue reminders |
| `GET` | `/api/reminders/stats` | Thống kê nhắc nhở | Reminder analytics |
| `GET` | `/api/reminders/due` | Nhắc nhở đến hạn | Due reminders |
| `POST` | `/api/reminders/{reminderId}/mark-fired` | Đánh dấu đã kích hoạt | Background service |
| `GET` | `/api/reminders/templates` | Template nhắc nhở | Reminder templates |
| `POST` | `/api/reminders/from-template` | Tạo nhắc nhở từ template | Template-based creation |
| `GET` | `/api/reminders/patient/{patientId}` | Nhắc nhở của bệnh nhân | Patient reminders |
| `POST` | `/api/reminders/patient/{patientId}/reminders/{reminderId}/snooze` | Bệnh nhân hoãn nhắc nhở | Patient snooze |

## 💬 Conversations Controller
**Mục đích**: Hệ thống chat và tư vấn trực tuyến

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `POST` | `/api/conversations` | Tạo cuộc trò chuyện mới | Chat initiation |
| `GET` | `/api/conversations` | Danh sách cuộc trò chuyện | Chat management |
| `GET` | `/api/conversations/{conversationId}` | Thông tin chi tiết cuộc trò chuyện | Chat details |
| `PUT` | `/api/conversations/{conversationId}/status` | Cập nhật trạng thái cuộc trò chuyện | Chat status |
| `DELETE` | `/api/conversations/{conversationId}` | Xóa cuộc trò chuyện | Chat removal |
| `POST` | `/api/conversations/{conversationId}/messages` | Gửi tin nhắn | Message sending |
| `GET` | `/api/conversations/{conversationId}/messages` | Lấy danh sách tin nhắn | Message history |
| `DELETE` | `/api/conversations/messages/{messageId}` | Xóa tin nhắn | Message removal |
| `GET` | `/api/conversations/stats` | Thống kê chat | Chat analytics |
| `POST` | `/api/conversations/patient/{patientId}/messages` | Bệnh nhân gửi tin nhắn | Patient messaging |
| `GET` | `/api/conversations/patient/{patientId}` | Cuộc trò chuyện của bệnh nhân | Patient chats |
| `GET` | `/api/conversations/patient/{patientId}/conversations/{conversationId}/messages` | Tin nhắn của bệnh nhân | Patient message history |

## 📊 Dashboard Controller
**Mục đích**: Báo cáo và thống kê tổng quan

| Method | Endpoint | Chức năng | Sử dụng cho |
|--------|----------|-----------|-------------|
| `GET` | `/api/dashboard/overview` | Dashboard tổng quan | Main dashboard |
| `GET` | `/api/dashboard/charts` | Dữ liệu charts cho dashboard | Chart visualization |
| `GET` | `/api/dashboard/doctor` | Dashboard dành cho bác sĩ | Doctor dashboard |
| `GET` | `/api/dashboard/widgets` | Các widget cho dashboard | Dashboard widgets |
| `GET` | `/api/dashboard/revenue-analytics` | Analytics doanh thu | Revenue reporting |
| `GET` | `/api/dashboard/recent-orders` | Đơn đặt dịch vụ gần đây | Recent activity |

---

# 📈 Dashboard Analytics Features

## 🎯 KPI Cards
- **Tổng doanh thu**: 100 triệu VNĐ (+20.5% growth)
- **Bệnh nhân mới**: +2,000 (+15.3% growth)
- **Tổng đơn dịch vụ**: +500 (+8.7% growth)
- **Tỷ lệ bệnh nhân quay lại**: 75% (+2.1% growth)
- **Tổng tiền đơn quá hạn**: 25,000,000đ (-5.2% improvement)

## 📊 Charts & Visualizations
- **Line Chart**: Xu hướng doanh thu theo tháng
- **Bar Chart**: Top 5 dịch vụ được sử dụng nhiều nhất
- **Donut Chart**: Tỷ trọng doanh thu theo dịch vụ
- **Table**: Danh sách đơn đặt dịch vụ gần đây

## 🎨 Dashboard Types
- **Main Dashboard**: Tổng quan cho ClinicAdmin
- **Doctor Dashboard**: Chuyên biệt cho bác sĩ
- **Patient Dashboard**: Dành cho bệnh nhân (future)
- **System Dashboard**: Cho SystemAdmin (future)

---

# ⚙️ Setup & Installation

## 1. Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

## 2. Configuration

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SavePlusDB;Trusted_Connection=true"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-minimum-32-characters",
    "Issuer": "SavePlus_API", 
    "Audience": "SavePlus_Users"
  }
}
```

## 3. Database Setup

```bash
# Tạo và áp dụng migrations
dotnet ef database update

# Hoặc sử dụng database có sẵn với connection string
```

## 4. Run Application

```bash
# Clone repository
git clone https://github.com/CherishVN/SavePlus_API.git
cd SavePlus_API

# Restore packages
dotnet restore

# Run application  
dotnet run

# API sẽ chạy tại: https://localhost:7139
# Swagger UI: https://localhost:7139/swagger
```

---

# 📖 API Usage Examples

## Đăng ký bệnh nhân mới

```json
POST /api/patients/register
{
  "fullName": "Nguyễn Văn A",
  "primaryPhoneE164": "+84901234567",
  "gender": "M",
  "dateOfBirth": "1990-01-15", 
  "address": "123 Đường ABC, Quận 1, TP.HCM"
}
```

## Tạo staff user

```json
POST /api/users
{
  "fullName": "Dr. Nguyễn Thị B",
  "email": "doctor@clinic.com",
  "phoneE164": "+84901234568",
  "role": "Doctor",
  "tenantId": 1,
  "password": "SecurePassword123"
}
```

## OTP Login Flow

```json
// 1. Request OTP
POST /api/auth/request-otp
{
  "phoneNumber": "+84901234567",
  "purpose": "login"
}

// 2. Verify OTP  
POST /api/auth/verify-otp
{
  "phoneNumber": "+84901234567", 
  "otpCode": "123456",
  "purpose": "login"
}
```

## Đặt lịch hẹn

```json
POST /api/appointments
{
  "tenantId": 1,
  "patientId": 123,
  "doctorId": 456,
  "startAt": "2024-01-15T10:00:00",
  "endAt": "2024-01-15T11:00:00",
  "type": "Clinic",
  "channel": "App"
}
```

## Kê đơn thuốc

```json
POST /api/prescriptions
{
  "patientId": 123,
  "doctorId": 456,
  "status": "Active",
  "items": [
    {
      "drugName": "Paracetamol",
      "form": "Tablet",
      "strength": "500mg",
      "dose": "1 viên",
      "route": "Oral",
      "frequency": "TID",
      "startDate": "2024-01-15",
      "endDate": "2024-01-22",
      "instructions": "Uống sau ăn"
    }
  ]
}
```

## Lấy dashboard analytics

```json
GET /api/dashboard/revenue-analytics?fromDate=2024-01-01&toDate=2024-12-31

Response:
{
  "success": true,
  "data": {
    "totalRevenue": 100000000,
    "revenueGrowth": 20.5,
    "newPatients": 2000,
    "patientGrowth": 15.3,
    "revenueByMonth": {
      "Tháng 1": 15000000,
      "Tháng 2": 18000000,
      ...
    },
    "topServices": {
      "Dịch vụ 1": 150,
      "Dịch vụ 2": 120,
      ...
    }
  }
}
```

---

# 🚀 Phase 2 - Advanced Features (Roadmap)

## 💳 Payment Integration
- **Payment Gateway**: VNPay, MoMo, ZaloPay integration
- **Invoice Management**: Tạo và quản lý hóa đơn
- **Payment Tracking**: Theo dõi thanh toán và công nợ
- **Financial Reports**: Báo cáo tài chính chi tiết

## ⚡ Real-time Features
- **SignalR Hub**: Real-time chat và notifications
- **Live Dashboard**: Cập nhật dashboard theo thời gian thực
- **Push Notifications**: Firebase/APNs integration
- **Live Appointment Updates**: Cập nhật lịch hẹn real-time

## 🤖 AI & Analytics
- **Predictive Analytics**: Dự đoán xu hướng sức khỏe
- **AI Recommendations**: Gợi ý điều trị và chăm sóc
- **Health Risk Assessment**: Đánh giá rủi ro sức khỏe
- **Smart Reminders**: Nhắc nhở thông minh dựa trên AI

## 🔗 System Integrations
- **EMR Integration**: Kết nối với hệ thống EMR khác
- **Laboratory Systems**: Tích hợp với phòng xét nghiệm
- **Pharmacy Systems**: Kết nối với nhà thuốc
- **Medical Devices**: IoT device integration

## 🛡️ Advanced Security
- **Rate Limiting**: Giới hạn request rate
- **Advanced Audit Logging**: Log chi tiết hoạt động
- **Data Encryption**: Mã hóa dữ liệu nhạy cảm
- **HIPAA Compliance**: Tuân thủ quy định y tế

---

# 🎯 Kết Luận

SavePlus API đã hoàn thành **90% tính năng cốt lõi** cần thiết cho một hệ thống y tế hậu điều trị hoàn chỉnh. Với **13 modules chính**, **150+ API endpoints**, và **dashboard analytics** đầy đủ, hệ thống sẵn sàng để triển khai thương mại.

## ✅ Điểm Mạnh
- **Architecture vững chắc**: Multi-tenant, role-based security
- **API hoàn chỉnh**: Đầy đủ CRUD operations cho tất cả entities
- **Dashboard Analytics**: Báo cáo và thống kê chuyên nghiệp
- **Mobile-ready**: APIs tối ưu cho mobile app
- **Scalable**: Thiết kế có thể mở rộng

## 🔧 Cần Cải Thiện
- **Payment System**: Tích hợp thanh toán thực tế
- **Real-time Features**: SignalR cho chat/notifications
- **Email Service**: Hoàn thiện email system
- **Advanced Analytics**: AI và machine learning

**SavePlus API** - *Hệ sinh thái y tế hậu điều trị hoàn chỉnh cho tương lai số* 🏥✨

---

## 🤝 Contributing

Dự án này được phát triển bởi **Neosix Team** cho hệ sinh thái y tế hậu điều trị.

## 📝 License

Private project - All rights reserved by Neosix Team.

---

**🏥 Neosix - Revolutionizing Post-Treatment Healthcare** 🚀