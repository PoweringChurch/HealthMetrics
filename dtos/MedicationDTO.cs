public class MedicationDTO
{
    //entry
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }

    //info
    public string? Name { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = still active
    public string? Notes { get; set; }
}