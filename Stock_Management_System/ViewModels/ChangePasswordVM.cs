using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.ViewModels
{
    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password doesn't match with confirm password")]
        public string ConfirmPassword { get; set; }
    }

}
