namespace SavePlus_API.Constants
{
    /// <summary>
    /// Các constants cho CarePlan
    /// </summary>
    public static class CarePlanConstants
    {
        /// <summary>
        /// Trạng thái kế hoạch chăm sóc
        /// </summary>
        public static class Statuses
        {
            /// <summary>
            /// Nháp, chưa kích hoạt
            /// </summary>
            public const string Draft = "Draft";
            
            /// <summary>
            /// Đang hoạt động
            /// </summary>
            public const string Active = "Active";
            
            /// <summary>
            /// Tạm dừng
            /// </summary>
            public const string Paused = "Paused";
            
            /// <summary>
            /// Đã hoàn thành
            /// </summary>
            public const string Completed = "Completed";
            
            /// <summary>
            /// Đã hủy
            /// </summary>
            public const string Cancelled = "Cancelled";
            
            /// <summary>
            /// Hết hạn
            /// </summary>
            public const string Expired = "Expired";
            
            /// <summary>
            /// Danh sách tất cả các trạng thái hợp lệ
            /// </summary>
            public static readonly string[] All = { Draft, Active, Paused, Completed, Cancelled, Expired };
        }
        
        /// <summary>
        /// Loại CarePlan Item
        /// </summary>
        public static class ItemTypes
        {
            /// <summary>
            /// Uống thuốc
            /// </summary>
            public const string Medication = "Medication";
            
            /// <summary>
            /// Đo chỉ số y tế
            /// </summary>
            public const string Measurement = "Measurement";
            
            /// <summary>
            /// Tập thể dục
            /// </summary>
            public const string Exercise = "Exercise";
            
            /// <summary>
            /// Chế độ ăn uống
            /// </summary>
            public const string Diet = "Diet";
            
            /// <summary>
            /// Theo dõi triệu chứng
            /// </summary>
            public const string Symptom = "Symptom";
            
            /// <summary>
            /// Ghi chú/nhắc nhở khác
            /// </summary>
            public const string Note = "Note";
            
            /// <summary>
            /// Danh sách tất cả các loại hợp lệ
            /// </summary>
            public static readonly string[] All = { Medication, Measurement, Exercise, Diet, Symptom, Note };
        }
        
        /// <summary>
        /// Tần suất thực hiện
        /// </summary>
        public static class Frequencies
        {
            /// <summary>
            /// Một lần
            /// </summary>
            public const string Once = "Once";
            
            /// <summary>
            /// Hàng ngày
            /// </summary>
            public const string Daily = "Daily";
            
            /// <summary>
            /// Mỗi tuần
            /// </summary>
            public const string Weekly = "Weekly";
            
            /// <summary>
            /// Mỗi tháng
            /// </summary>
            public const string Monthly = "Monthly";
            
            /// <summary>
            /// Theo yêu cầu
            /// </summary>
            public const string AsNeeded = "AsNeeded";
            
            /// <summary>
            /// Tùy chỉnh
            /// </summary>
            public const string Custom = "Custom";
            
            /// <summary>
            /// Danh sách tất cả các tần suất hợp lệ
            /// </summary>
            public static readonly string[] All = { Once, Daily, Weekly, Monthly, AsNeeded, Custom };
        }
    }
}
