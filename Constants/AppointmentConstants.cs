namespace SavePlus_API.Constants
{
    /// <summary>
    /// Các constants cho Appointment
    /// </summary>
    public static class AppointmentConstants
    {
        /// <summary>
        /// Loại cuộc hẹn
        /// </summary>
        public static class Types
        {
            /// <summary>
            /// Khám tại nhà
            /// </summary>
            public const string Home = "Home";
            
            /// <summary>
            /// Khám tại phòng khám
            /// </summary>
            public const string Clinic = "Clinic";
            
            /// <summary>
            /// Khám online/telemedicine
            /// </summary>
            public const string Online = "Online";
            
            /// <summary>
            /// Tư vấn qua điện thoại
            /// </summary>
            public const string Phone = "Phone";
            
            /// <summary>
            /// Danh sách tất cả các loại hợp lệ
            /// </summary>
            public static readonly string[] All = { Home, Clinic, Online, Phone };
        }
        
        /// <summary>
        /// Kênh đặt lịch hẹn
        /// </summary>
        public static class Channels
        {
            /// <summary>
            /// Đặt qua ứng dụng di động
            /// </summary>
            public const string App = "App";
            
            /// <summary>
            /// Đặt qua website
            /// </summary>
            public const string Web = "Web";
            
            /// <summary>
            /// Đặt qua điện thoại
            /// </summary>
            public const string Phone = "Phone";
            
            /// <summary>
            /// Đặt trực tiếp tại quầy
            /// </summary>
            public const string Counter = "Counter";
            
            /// <summary>
            /// Đặt bởi nhân viên y tế
            /// </summary>
            public const string Staff = "Staff";
            
            /// <summary>
            /// Danh sách tất cả các kênh hợp lệ
            /// </summary>
            public static readonly string[] All = { App, Web, Phone, Counter, Staff };
        }
        
        /// <summary>
        /// Trạng thái cuộc hẹn
        /// </summary>
        public static class Statuses
        {
            /// <summary>
            /// Đã đặt lịch, chờ xác nhận
            /// </summary>
            public const string Scheduled = "Scheduled";
            
            /// <summary>
            /// Đã xác nhận
            /// </summary>
            public const string Confirmed = "Confirmed";
            
            /// <summary>
            /// Đang diễn ra
            /// </summary>
            public const string InProgress = "InProgress";
            
            /// <summary>
            /// Đã hoàn thành
            /// </summary>
            public const string Completed = "Completed";
            
            /// <summary>
            /// Đã hủy
            /// </summary>
            public const string Cancelled = "Cancelled";
            
            /// <summary>
            /// Không đến (no-show)
            /// </summary>
            public const string NoShow = "NoShow";
            
            /// <summary>
            /// Đã hoãn
            /// </summary>
            public const string Rescheduled = "Rescheduled";
            
            /// <summary>
            /// Danh sách tất cả các trạng thái hợp lệ
            /// </summary>
            public static readonly string[] All = { Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow, Rescheduled };
        }
    }
}
