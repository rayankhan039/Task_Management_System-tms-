using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Management_System.Models
{
    public class WalletTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [DisplayName("Employee")]
        
        public int Emp_Id { get; set; }

        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; }


        [ForeignKey("Emp_Id")]
        public Users? users { get; set; }
    }
}
