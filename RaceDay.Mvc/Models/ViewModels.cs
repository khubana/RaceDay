using System.ComponentModel.DataAnnotations;
namespace RaceDay.Mvc.Models;
public class LoginVm {
  [Required,EmailAddress] 
  public string Email {get;set;}=""; 
  [Required]
  public string Password {get;set;}="";
}

public class RegisterVm { [Required] public string FirstName {get;set;}=""; [Required] public string LastName {get;set;}=""; [Required,EmailAddress] public string Email {get;set;}=""; [Required,MinLength(8)] public string Password {get;set;}=""; [Required] public string Phone {get;set;}=""; [Required] public string RoleName {get;set;}="Participant"; }
public class EventVm { public int EventID {get;set;} [Required] public string EventName {get;set;}=""; [Required] public string EventDescription {get;set;}=""; [Required] public DateTime EventDate {get;set;}=DateTime.Today.AddDays(30); [Required] public string EventLocation {get;set;}=""; public string EventStatus {get;set;}="open"; }
public class CategoryVm { public string CategoryName {get;set;}=""; public int MaxParticipants {get;set;}=100; public decimal EntryFee {get;set;}=50; }
public class ResultVm { public int Position {get;set;}=1; public string? FinishTime {get;set;} public decimal? Score {get;set;} }
public class ProfileVm { public string FirstName {get;set;}=""; public string LastName {get;set;}=""; public string Email {get;set;}=""; public string Phone {get;set;}=""; }
