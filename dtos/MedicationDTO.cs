public class MedicationDTO
{
    //entry
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }

    //info
    public required string Name { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }

    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = still active
    public string? Notes { get; set; }
}