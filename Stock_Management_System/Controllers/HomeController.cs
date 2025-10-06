using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Management_System.Models;
using Task_Management_System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Task_Management_System.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDBContext context;

    public HomeController(ILogger<HomeController> logger, AppDBContext _context)
    {
        _logger = logger;
        context = _context;
    }
    //======================================================= Main Dashboard =================================================//
    [Authorize (Roles ="Employee")]
    public IActionResult Index()
    {
       
        var userId = User.FindFirst("User Id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        int ParseUserId = int.Parse(userId);
        var tasks = context.tasks.Include(u => u.users).Where(t => t.Emp_Id == ParseUserId && t.Status==0).OrderByDescending(t=>t.CreatedAt).ToList();
        var employee = context.users.SingleOrDefault(u => u.U_Id == ParseUserId);
        ViewBag.pendingtask = context.tasks.Where(t => t.Status == 0 && t.Emp_Id==ParseUserId).Count();
        ViewBag.submitedtask = context.tasks.Where(t => t.Status == 1 && t.Emp_Id == ParseUserId).Count();
        ViewBag.expiredtask = context.tasks.Where(t => t.Status == 2 && t.Emp_Id == ParseUserId).Count();
        ViewBag.approvedtask = context.tasks.Where(t => t.Status == 3 && t.Emp_Id == ParseUserId).Count();
        ViewBag.rejectedtask = context.tasks.Where(t => t.Status == 4 && t.Emp_Id == ParseUserId).Count();

        foreach(var task in tasks)
        {
            if(task.DeadLine<DateTime.Now && task.Status == 0)
            {
                task.Status = 2;
                decimal penalty = task.Price;
                employee.U_Wallet -= penalty;
                if (employee.U_Wallet < 0) employee.U_Wallet = 0;
                context.WalletTransactions.Add(new WalletTransaction
                {
                    Emp_Id = employee.U_Id,
                    Amount = penalty,
                    TransactionType = "Debit",
                    Description = $"Task '{task.Title}' is Expired!. Penalty debited!.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
                context.SaveChanges();
            }
        }

        return View(tasks);
    }
    //======================================================= Main Dashboard =================================================//


    //======================================================= Task Submission Page =================================================//

    public IActionResult SubmitTask(int id)
    {
        var FindTask = context.tasks.SingleOrDefault(t => t.Task_Id == id);
        
        if(FindTask==null || FindTask.Status != 0)
        {
            return NotFound();
        }

        if (FindTask.DeadLine < DateTime.Now)
        {
            FindTask.Status = 2;
            context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        return View(FindTask);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTask(int id, IFormFile WorkFile)
    {
        var FindTask = context.tasks.SingleOrDefault(t => t.Task_Id == id);
        
        if(FindTask==null || FindTask.Status != 0)
        {
            return NotFound();
        }

        if (FindTask.DeadLine < DateTime.Now)
        {
            FindTask.Status = 2;
            context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        if(WorkFile != null && WorkFile.Length > 0)
        {
            //Upload Submitted Task in Folder
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SubmittedTasks");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            //For Unique File Name
            var uniqueFileName = $"{Guid.NewGuid()}_{WorkFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);


            using(var stream= new FileStream(filePath, FileMode.Create))
            {
                await WorkFile.CopyToAsync(stream);
            }

            //update record in Database
            FindTask.SubmittedWorkPath = "/SubmittedTasks/" + uniqueFileName;
            FindTask.SubmitedAt = DateTime.Now;
            FindTask.Status = 1;

            context.SaveChanges();


        }

        return RedirectToAction("Index","Home");
    }
    //======================================================= Task Submission Page =================================================//



    //======================================================= Submitted,Approved/Rejected Tasks Pages =================================================//

    public IActionResult MySubmittedTasks()
    {
        var UserId = User.FindFirst("User Id")?.Value;
        if (string.IsNullOrEmpty(UserId))
        {
            return Unauthorized();
        }
        int ParseId = int.Parse(UserId);

        var SubmittedTasks = context.tasks.Include(u => u.users).Where(t => t.Emp_Id == ParseId && t.Status == 1).OrderByDescending(t=>t.SubmitedAt).ToList();

        return View(SubmittedTasks);
    }


    public IActionResult CompletedTasks()
    {
        var UserId = User.FindFirst("User Id")?.Value;
        if(string.IsNullOrEmpty(UserId))
        {
            return Unauthorized();
        }
        int ParseId = int.Parse(UserId);

        var completeTasks = context.tasks.Include(u => u.users).Where(t => t.Emp_Id == ParseId && t.Status==2 || t.Status == 3 || t.Status == 4).OrderByDescending(t=>t.UpdatedAt).ToList();

        return View(completeTasks);
    }


    //======================================================= Approved & Rejected Tasks Pages =================================================//





    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
