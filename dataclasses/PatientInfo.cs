//details advised as per NAHAM
using System.ComponentModel.DataAnnotations;

public enum Sex {Male, Female, Unknown}
public class PatientInfo
{
    //Meta

    public int Id { get; set; } 

    //Basic info
    public string? LastName {get; set;}
    public string? MiddleName {get; set;}
    public string? FirstName {get; set;}
    public string? PreferredName {get; set;}

    public DateTime DOB {get; set;} //Date of birth
    public int Age => DateTime.Now.Year - DOB.Year - (DateTime.Now.DayOfYear < DOB.DayOfYear ? 1 : 0);

    public Sex Sex {get; set;}
    public string? Gender {get; set;}

    //Contact information
    public string? HomeAddress {get; set;} //Addresses should be recorded following the Unites States Postal Service (USPS) standards
    public string? PhoneNumber {get; set;} //Phone numbers should be recorded following the North American Numbering Plan (NANP)

    public string? AdditionalInfo {get; set;}

    //Medical history
    public virtual ICollection<VitalsEntry> ? VitalsHistory {get; set;} = new List<VitalsEntry>();
    public virtual ICollection<Medication>? MedicationHistory {get; set;} = new List<Medication>();
    public virtual ICollection<Diagnosis>? DiagnosesHistory {get; set;} = new List<Diagnosis>();
}