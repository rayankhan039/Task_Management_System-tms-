using Microsoft.AspNetCore.Mvc;
using Task_Management_System.Data;

namespace Task_Management_System.Controllers
{
    public class WalletController : Controller
    {
        private readonly AppDBContext context;
        public WalletController(AppDBContext _context)
        {
            context = _context;
            
        }
        public IActionResult Wallet()
        {
            var userid = int.Parse(User.FindFirst("User Id")?.Value);
            var user = context.users.FirstOrDefault(u => u.U_Id == userid);
            ViewBag.Balance = user?.U_Wallet ??0;

            var history = context.WalletTransactions.Where(w => w.Emp_Id == userid).OrderByDescending(w=>w.CreatedAt).ToList();
            ViewBag.Unread = context.WalletTransactions.Where(w => w.Emp_Id == userid && w.IsRead == false).ToList();

            foreach (var n in context.WalletTransactions.Where(w => w.Emp_Id == userid && !w.IsRead))
            {
                n.IsRead = true;
                context.SaveChanges();

            }


            return View(history);
        }
    }
}
