//details advised as per NAHAM
using System;
using System.ComponentModel.DataAnnotations;

public enum Sex {Male, Female, Unknown}
public class PatientInfo
{
    //Meta
    public int Id { get; set; } 

    //Basic info
    [StringLength(100)]
    public string? LastName {get; set;}
    [StringLength(100)]
    public string? MiddleName {get; set;}

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be 1-100 characters")]
    public string? FirstName {get; set;}
    [StringLength(100)]
    public string? PreferredName {get; set;}
    [DataType(DataType.Date)] 
    public DateTime? DOB {get; set;} //Date of birth
    public int? Age 
    { 
        get
        {
            if (!DOB.HasValue) return null;
            
            var today = DateTime.Today;
            var age = today.Year - DOB.Value.Year;
            if (DOB.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
    [Range(0, 2, ErrorMessage = "Sex must be 0 (Male), 1 (Female), or 2 (Unknown / N.A.)")] 
    public Sex? Sex {get; set;}
    [StringLength(50)]
    public string? Gender {get; set;}

    //Contact information
    [StringLength(500)]
    public string? HomeAddress {get; set;}
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    public string? PhoneNumber {get; set;} //Phone numbers should be recorded following the North American Numbering Plan (NANP)
    [StringLength(1000)]
    public string? AdditionalInfo {get; set;}

    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}