using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
    }
}
