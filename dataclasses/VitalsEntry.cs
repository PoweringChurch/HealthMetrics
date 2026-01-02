public class VitalsEntry
{
    //entry meta
    public DateTime EntryDate {get; set;} //vitals as of..
    public string? Location { get; set; }
    public string? RecordedBy { get; set; }
    public int Id { get; set; }
    public int PatientId { get; set; }
    
    //info
    public float? SystolicBloodPressure {get; set;}
    public float? DiastolicBloodPressure {get; set;}
    public float? RestingHeartRate {get; set;} //beats per minute
    public float? OxygenSaturation { get; set; } 

    public float? Temperature {get; set;} //celcius
    public float? RestingRespirationRate {get; set;} //breaths per minute

    public float? Height {get; set;} //cm
    public float? Weight {get; set;} //kg
}