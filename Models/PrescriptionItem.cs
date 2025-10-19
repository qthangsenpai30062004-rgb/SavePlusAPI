using System;
using System.Collections.Generic;

namespace SavePlus_API.Models;

public partial class PrescriptionItem
{
    public int ItemId { get; set; }

    public int PrescriptionId { get; set; }

    public string DrugName { get; set; } = null!;

    public string? Form { get; set; }

    public string? Strength { get; set; }

    public string? Dose { get; set; }

    public string? Route { get; set; }

    public string? Frequency { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Instructions { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;
}
