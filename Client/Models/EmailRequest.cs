using System.ComponentModel.DataAnnotations;

namespace PersonalStaticApp.Client.Models
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string? Body { get; set; }
    }
}
