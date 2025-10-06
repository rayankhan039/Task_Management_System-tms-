using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management_System.Models
{
    public class Tasks
    {
        [Key]
        public int Task_Id { get; set; }

        [DisplayName("Employee")]
        [Required (ErrorMessage ="Employee is Required")]
        public int Emp_Id { get; set; }

        [Required (ErrorMessage ="Title is Required")]
        public string? Title { get; set; }

        
        [Required (ErrorMessage ="Description is Required")]
        public string? Desc { get; set; }

        [DataType(DataType.Currency)]
        [Required (ErrorMessage ="Price is Required")]
        public decimal Price { get; set; }

        [DataType(DataType.DateTime)]
        [Required (ErrorMessage ="Deadline is Required")]
        public DateTime DeadLine { get; set; }

        required
        public byte Status { get; set; }
        public string? Feedback { get; set; } //by Admin

        public string? SubmittedWorkPath { get; set; }
        public DateTime? SubmitedAt { get; set; }
        public string? From { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        [NotMapped]
        public IFormFile? SubmittedWork { get; set; }

        [ForeignKey("Emp_Id")]
        public Users? users { get; set; }
    }
}
