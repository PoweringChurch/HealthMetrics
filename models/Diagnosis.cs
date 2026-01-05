using System.ComponentModel.DataAnnotations;

public enum DiagnosisStatus {Active, Resolved}
public class Diagnosis
{
    //entry meta
    public int Id { get; set; }
    [Required(ErrorMessage = "Associated patient id is required")]
    public int PatientId { get; set; }
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }
    //info
    [Required(ErrorMessage = "Diagnosis name is required")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Diagnosis date is required")]
    public DateTime DiagnosisDate { get; set; }
    public DateTime? ResolvedDate { get; set;}
    [Required(ErrorMessage = "Diagnosis status is required")]
    public DiagnosisStatus Status {get; set;} // active, resolved
    public string? Notes { get; set; }
}