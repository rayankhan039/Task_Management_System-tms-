using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.Services;
using Task_Management_System.ViewModels;

namespace Task_Management_System.Controllers
{
   
    public class AdminController : Controller
    {
        private readonly AppDBContext context;
        private readonly MailService _mailService;

        public AdminController(AppDBContext _context, MailService mailService)
        {
            context = _context;
            _mailService = mailService;
        }

        [Authorize(Roles ="Admin")]
        public IActionResult AdminDashboard()
        {
            var query = context.tasks.Include(t => t.users).OrderByDescending(t=>t.CreatedAt).ToList();
            ViewBag.pendingtask = context.tasks.Where(t => t.Status == 0).Count();
            ViewBag.submitedtask = context.tasks.Where(t => t.Status == 1).Count();
            ViewBag.expiredtask = context.tasks.Where(t => t.Status == 2).Count();
            ViewBag.approvedtask = context.tasks.Where(t => t.Status == 3).Count();
            ViewBag.rejectedtask = context.tasks.Where(t => t.Status == 4).Count();

            return View(query);
        }

        //------------------------------------------User Related work Starts Here----------------------------------

        //Register User by Admin
        [Authorize(Roles = "Admin")]

        public IActionResult RegisterUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterUser(Users u)
        {
            
            if (ModelState.IsValid)
            {
                u.Role = "Employee";
                u.U_Wallet = 0;
                u.RegisteredAt = DateTime.Now;
                context.users.Add(u);
                context.SaveChanges();

                string subject = "Welcome To Smart Task System";
                string body = $@"
                         <h3> Hello {u.U_Name},</h3>

                          <p>Your Account has been Registered in our Company.</p>

                         <p><b>Your Username:</b> {u.U_Email}</p>

                         <p><b>Your Password:</b> {u.U_Password}</p>
                         <p><b>Note:</b> This is a type of temporary Password.You should change Your Password after 1st Login..</p>
                         </br><p>Regards,</br><b>Admin Team</b></p>

";
                await _mailService.SendEmailAsync(u.U_Email!,subject,body);
                return RedirectToAction("ViewUser", "Admin");
            }
            return View();
        }

        //View User by Admin
        [Authorize(Roles = "Admin")]

        public IActionResult ViewUser()
        {
            var query = context.users.Where(x=>x.Role=="Employee").OrderByDescending(x=>x.RegisteredAt).ToList();
            return View(query);

        }

        //Edit User by Admin
        [Authorize(Roles = "Admin")]

        public IActionResult EditUser(int id)
        {
            var query = context.users.SingleOrDefault(x => x.U_Id == id);
            return View(query);
        }

        [HttpPost]
        public IActionResult EditUser(Users u)
        {
            if (ModelState.IsValid)
            {
                u.Role = "Employee";
                u.U_Wallet = 0;
         
                context.users.Update(u);
                context.SaveChanges();
                return RedirectToAction("ViewUser", "Admin");
            }
            return View(u);
        }

        //Delete User by Admin
        [Authorize(Roles = "Admin")]

        public IActionResult DeleteUser(int id)
        {
            var query = context.users.SingleOrDefault(x => x.U_Id == id);
            context.users.Remove(query);
            context.SaveChanges();
            return RedirectToAction("ViewUser", "Admin");


        }
        //------------------------------------------User Related work Ends Here----------------------------------



        //------------------------------------------Task Related work Starts Here----------------------------------

        [Authorize(Roles = "Admin")]
        public IActionResult AssignTask()
        {
            ViewBag.Emp = context.users.Where(u => u.Role == "Employee").Select(Users => new SelectListItem
            {
                Value = Users.U_Id.ToString(),
                Text = Users.U_Name
            }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AssignTask(Tasks task)
        {
            if (ModelState.IsValid)
            {
                
                task.From = "Admin";
                task.Status = 0;
                task.CreatedAt = DateTime.Now;
                context.tasks.Add(task);
                context.SaveChanges();
                return RedirectToAction("AdminDashboard", "Admin");

            }
            ViewBag.Emp = context.users.Where(u => u.Role == "Employee").Select(Users => new SelectListItem
            {
                Value = Users.U_Id.ToString(),
                Text = Users.U_Name
            }).ToList();

            return View(task);
        }

        [Authorize(Roles = "Admin")]

        public IActionResult EditTask(int id)
        {
            var query = context.tasks.SingleOrDefault(x => x.Task_Id == id);

            ViewBag.Emp = context.users.Where(u => u.Role == "Employee").Select(Users => new SelectListItem
            {
                Value = Users.U_Id.ToString(),
                Text = Users.U_Name
            }).ToList();

            return View(query);
        }

        [HttpPost]
        public IActionResult EditTask(Tasks t)
        {
            
            if (ModelState.IsValid)
            {
                t.From = "Admin";
                t.UpdatedAt = DateTime.Now;
                context.tasks.Update(t);
                context.SaveChanges();
                return RedirectToAction("AdminDashboard", "Admin");
            }
            return View();
        }
        //------------------------------------------Task Related work Ends Here----------------------------------




        //------------------------------------------Approved & Reject Task work Starts from Here----------------------------------
        public IActionResult SubmittedTasks()
        {
            var showTasks = context.tasks.Include(u=>u.users).Where(t => t.Status == 1).OrderByDescending(t=>t.SubmitedAt).ToList();
            return View(showTasks);
        }


        public IActionResult ApproveTask(int id)
        {
            var approve = context.tasks.SingleOrDefault(x => x.Task_Id == id);

            if(approve==null || approve.Status != 1)
            {
                return NotFound();
            }

            return View(approve);
        }

        [HttpPost]
        public IActionResult ApproveTask(int id,string feedback)
        {
            var approve = context.tasks.SingleOrDefault(x => x.Task_Id == id);
            var employee = context.users.FirstOrDefault(e => e.U_Id == approve.Emp_Id);
            if(approve==null || employee==null)
            {
                return NotFound();
            }
            approve.Feedback = feedback;
            approve.Status = 3;
            approve.UpdatedAt = DateTime.Now;
            employee.U_Wallet += approve.Price;

            context.WalletTransactions.Add(new WalletTransaction
            {
                Emp_Id = employee.U_Id,
                Amount=approve.Price,
                TransactionType="Credit",
                Description=$"Task '{approve.Title}' Approved by Admin. Amount credited.",
                CreatedAt=DateTime.Now,
                IsRead=false
            });
            context.SaveChanges();

            return RedirectToAction("SubmittedTasks", "Admin");

        }

        public IActionResult RejectTask(int id)
        {
            var reject = context.tasks.SingleOrDefault(x => x.Task_Id == id);
            if(reject==null || reject.Status != 1)
            {
                return NotFound();
            }

            return View(reject);
        }

        [HttpPost]
        public IActionResult RejectTask(int id,string feedback)
        {
            var reject = context.tasks.SingleOrDefault(x => x.Task_Id == id);
            var employee = context.users.FirstOrDefault(e => e.U_Id == reject.Emp_Id);
            if (reject == null || employee == null)
            {
                return NotFound();
            }
            reject.Feedback = feedback;
            reject.Status = 4;
            reject.UpdatedAt = DateTime.Now;

            decimal penalty = reject.Price / 2;
            employee.U_Wallet -= penalty;
            if (employee.U_Wallet < 0)
                employee.U_Wallet = 0;

            context.WalletTransactions.Add(new WalletTransaction
            {
                Emp_Id = employee.U_Id,
                Amount = penalty,
                TransactionType = "Debit",
                Description = $"Task '{reject.Title}' Rejected by Admin. Penalty debited!.",
                CreatedAt = DateTime.Now,
                IsRead = false
            });
            context.SaveChanges();

            return RedirectToAction("SubmittedTasks", "Admin");
        }


        //------------------------------------------Approved & Reject Task work Ends Here----------------------------------


        //=======================================
    }
}
