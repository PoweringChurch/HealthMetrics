using System.ComponentModel.DataAnnotations;

public class Medication
{
    //entry meta
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }

    //info
    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Dosage is required")]
    public string? Dosage { get; set; }
    [Required(ErrorMessage = "Frequency is required")]
    public string? Frequency { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = still active
    public string? Notes { get; set; }

    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}