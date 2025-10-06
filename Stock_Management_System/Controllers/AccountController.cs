using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Task_Management_System.Data;
using Task_Management_System.Models;
using Task_Management_System.ViewModels;

namespace Task_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext context;

        public AccountController(AppDBContext _context)
        {
            context = _context;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
      public async Task<IActionResult> Login(LoginVM u)
        {
            if (ModelState.IsValid)
            {
                
            var query = context.users.FirstOrDefault(x => x.U_Email == u.Email && x.U_Password == u.Password);

            if (query != null)
            {


                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, query.U_Name!),
                    new Claim(ClaimTypes.Role, query.Role!),
                    new Claim("User Id", query.U_Id.ToString()),

                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);


                if (query.Role == "Admin")
                {
                   
                    return RedirectToAction("AdminDashboard", "Admin");

                }
                else
                {
                        return RedirectToAction("Index", "Home");
                }
             
            }
                ModelState.AddModelError("", "Invalid Email or Paswword!");
            }
            return View(u);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");

        }



        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Get logged-in userId from claims
            var userId = User.FindFirst("User Id")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch user from DB
            var user = context.users.FirstOrDefault(u => u.U_Id == int.Parse(userId));
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            // Check current password
            if (user.U_Password != model.CurrentPassword) 
            {
                ModelState.AddModelError("", "Incorrect current password");
                return View(model);
            }

            // Check new password vs confirm password
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "New password doesn't match with confirm password");
                return View(model);
            }

            // Update password
            user.U_Password = model.NewPassword;
            context.SaveChanges();

            ViewBag.Message = "Password changed successfully";
            return View();
        }


        //===================================
    }
}
