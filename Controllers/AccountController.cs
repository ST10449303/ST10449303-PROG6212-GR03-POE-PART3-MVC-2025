using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                // Assign role to user
                await _userManager.AddToRoleAsync(user, model.Role);

                // Sign in user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect based on role
                return model.Role switch
                {
                    "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                    "Coordinator" => RedirectToAction("Dashboard", "Coordinator"),
                    "Manager" => RedirectToAction("Dashboard", "Manager"),
                    "HR" => RedirectToAction("Dashboard", "HR"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            // Add errors to model state
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("Register", model);
        }

        // GET: Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("Login");
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // Redirect based on role
                    if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                        return RedirectToAction("Dashboard", "Lecturer");
                    if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                        return RedirectToAction("Dashboard", "Coordinator");
                    if (await _userManager.IsInRoleAsync(user, "Manager"))
                        return RedirectToAction("Dashboard", "Manager");
                    if (await _userManager.IsInRoleAsync(user, "HR"))
                        return RedirectToAction("Dashboard", "HR");

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View("Login", model);
        }

        // POST: Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: Access Denied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
