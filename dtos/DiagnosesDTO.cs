public class DiagnosisDTO
{
    //entry meta
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }
    //info
    public string? Name { get; set; }
    public DateTime? DiagnosisDate { get; set; }
    public DateTime? ResolvedDate { get; set;}
    public DiagnosisStatus Status {get; set;} // active, resolved
    public string? Notes { get; set; }
}