using System.ComponentModel.DataAnnotations;
namespace TestFinal.Models;
#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations.Schema;


public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
      
    
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Request> Requests {get;set;} = new List<Request>();



    // public List<Like> Liked { get; set; } = new List<Like>(); 
    // Will not be mapped to your users table!
    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string Confirm { get; set; }
     [Required]
    public string About { get; set; }      
}
public class LoginUser
{
    [DataType(DataType.EmailAddress)]
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
