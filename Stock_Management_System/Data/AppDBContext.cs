using Microsoft.EntityFrameworkCore;
using Task_Management_System.Models;
using Task_Management_System.Services;

namespace Task_Management_System.Data
{
    public class AppDBContext :DbContext
    {
        private readonly MailService _mailService;

        public AppDBContext(DbContextOptions<AppDBContext> options,MailService mailService) : base(options)
        {
            _mailService = mailService;
        }

        public DbSet<Users> users { get; set; }
        public DbSet<Tasks> tasks { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
    }
}
