public class Medication
{
    //entry meta
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }

    //info
    public required string Name { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = still active
    public string? Notes { get; set; }
}