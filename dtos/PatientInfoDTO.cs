//details advised as per NAHAM
using System.ComponentModel.DataAnnotations;

public class PatientInfoDTO
{
    //Basic info
    public string? LastName {get; set;}
    public string? MiddleName {get; set;}
    public string? FirstName {get; set;}
    public string? PreferredName {get; set;}


    public DateTime? DOB {get; set;}
    public Sex? Sex {get; set;}
    public string? Gender {get; set;}

    //Contact information
    public string? HomeAddress {get; set;}
    public string? PhoneNumber {get; set;}
    public string? AdditionalInfo {get; set;}
}