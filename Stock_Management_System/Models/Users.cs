using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Task_Management_System.Models
{
    public class Users
    {
        [Key]
        public int U_Id { get; set; }

        [Required (ErrorMessage ="Name is Required")]
        [DisplayName("Employee Name")]
        public string? U_Name { get; set; }

        [DataType(DataType.EmailAddress,ErrorMessage ="Email is Invalid!")]
        [Required (ErrorMessage ="Email is Required")]
        [DisplayName("Employee Email")]
        public string? U_Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is Required")]
        [DisplayName("Employee Password")]
        [StringLength(15,MinimumLength = 8,ErrorMessage ="Password must be 8 Characters")]
        public string? U_Password { get; set; }

        [DataType(DataType.Currency)]
        public decimal? U_Wallet { get; set; }

        required
        public string? Role { get; set; }

        public DateTime? RegisteredAt { get; set; }
        public ICollection<Tasks>? Tasks { get; set; }
    }
}
