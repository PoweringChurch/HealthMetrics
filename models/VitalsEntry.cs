using System.ComponentModel.DataAnnotations;

public class VitalsEntry
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    [Required(ErrorMessage = "Date taken is required")]
    public DateTime? DateTaken {get; set;} //vitals as of..
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }
    
    //info
    public float? SystolicBloodPressure {get; set;}
    public float? DiastolicBloodPressure {get; set;}
    public float? RestingHeartRate {get; set;} //beats per minute
    public float? OxygenSaturation { get; set; } 

    public float? Temperature {get; set;} //celcius
    public float? RestingRespirationRate {get; set;} //breaths per minute
    
    public float? Height {get; set;} //cm
    public float? Weight {get; set;} //kg

    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}