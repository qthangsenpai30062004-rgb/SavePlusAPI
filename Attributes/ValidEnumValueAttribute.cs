using System.ComponentModel.DataAnnotations;

namespace SavePlus_API.Attributes
{
    /// <summary>
    /// Validation attribute để kiểm tra giá trị có nằm trong danh sách cho phép không
    /// </summary>
    public class ValidEnumValueAttribute : ValidationAttribute
    {
        private readonly string[] _validValues;
        private readonly string _enumName;

        public ValidEnumValueAttribute(string enumName, params string[] validValues)
        {
            _validValues = validValues;
            _enumName = enumName;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true; // Để Required attribute xử lý null

            var stringValue = value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return true; // Để Required attribute xử lý empty

            return _validValues.Contains(stringValue);
        }

        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                return $"{name} phải là một trong các giá trị: {string.Join(", ", _validValues)}";
            }
            return base.FormatErrorMessage(name);
        }
    }

    /// <summary>
    /// Validation attribute cho AppointmentType
    /// </summary>
    public class ValidAppointmentTypeAttribute : ValidEnumValueAttribute
    {
        public ValidAppointmentTypeAttribute() 
            : base("AppointmentType", Constants.AppointmentConstants.Types.All)
        {
        }
    }

    /// <summary>
    /// Validation attribute cho AppointmentChannel
    /// </summary>
    public class ValidAppointmentChannelAttribute : ValidEnumValueAttribute
    {
        public ValidAppointmentChannelAttribute() 
            : base("AppointmentChannel", Constants.AppointmentConstants.Channels.All)
        {
        }
    }

    /// <summary>
    /// Validation attribute cho AppointmentStatus
    /// </summary>
    public class ValidAppointmentStatusAttribute : ValidEnumValueAttribute
    {
        public ValidAppointmentStatusAttribute() 
            : base("AppointmentStatus", Constants.AppointmentConstants.Statuses.All)
        {
        }
    }

    /// <summary>
    /// Validation attribute cho CarePlanStatus
    /// </summary>
    public class ValidCarePlanStatusAttribute : ValidEnumValueAttribute
    {
        public ValidCarePlanStatusAttribute() 
            : base("CarePlanStatus", Constants.CarePlanConstants.Statuses.All)
        {
        }
    }

    /// <summary>
    /// Validation attribute cho CarePlanItemType
    /// </summary>
    public class ValidCarePlanItemTypeAttribute : ValidEnumValueAttribute
    {
        public ValidCarePlanItemTypeAttribute() 
            : base("CarePlanItemType", Constants.CarePlanConstants.ItemTypes.All)
        {
        }
    }

    /// <summary>
    /// Validation attribute cho CarePlanFrequency
    /// </summary>
    public class ValidCarePlanFrequencyAttribute : ValidEnumValueAttribute
    {
        public ValidCarePlanFrequencyAttribute() 
            : base("CarePlanFrequency", Constants.CarePlanConstants.Frequencies.All)
        {
        }
    }
}
