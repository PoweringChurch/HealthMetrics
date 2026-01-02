public enum DiagnosisStatus {Active, Resolved}
public class Diagnosis
{
    //entry meta
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }
    //info
    public required string Name { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? ResolvedDate { get; set;}
    public DiagnosisStatus Status {get; set;} // active, resolved
    public string? Notes { get; set; }
}