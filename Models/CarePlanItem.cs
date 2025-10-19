using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class CarePlanItem
{
    public int ItemId { get; set; }

    public int CarePlanId { get; set; }

    public string ItemType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? FrequencyCron { get; set; }

    public byte? TimesPerDay { get; set; }

    public string? DaysOfWeek { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual CarePlan CarePlan { get; set; } = null!;

    public virtual ICollection<CarePlanItemLog> CarePlanItemLogs { get; set; } = new List<CarePlanItemLog>();
}
