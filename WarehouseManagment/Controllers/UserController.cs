using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models.User;

namespace WarehouseManagment.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoginHistoryService loginHistoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loginHistoryService = loginHistoryService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {

            var model = new RegisterViewModel();

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existUser = await _userManager.FindByEmailAsync(model.Email);

            if (existUser != null)
            {
                if (existUser.Email == model.Email)
                {
                    ModelState.AddModelError("", "Already exist user with this email");

                    return View(model);
                }
            }

            var user = new ApplicationUser()
            {
                Email = model.Email,
                FirstName = model.FirstNane,
                LastName = model.LastNane,
                PhoneNumber = model.PhoneNumber,
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "User");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            var model = new LoginViewModel();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl, [Bind(Prefix = "g-recaptcha-response")] string recaptchaResponse)
        {

            model.Response = recaptchaResponse;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await VerifyRecaptcha(model.Response, model.Secret))
            {
                ModelState.AddModelError("", "Трябва да потвърдите, че не сте робот.");
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);

            //var userRole = await _userManager.GetRolesAsync(user);
            await _loginHistoryService.UserLoginTime(user.Id);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login");

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            await _loginHistoryService.UserLogoutTime(userId);

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home", new { area = "default" });
        }

        private async Task<bool> VerifyRecaptcha(string recaptchaResponse, string secret)
        {

            using (var httpClient = new HttpClient())
            {
                var postData = new Dictionary<string, string>
            {
                { "secret", secret },
                { "response", recaptchaResponse }
            };

                var content = new FormUrlEncodedContent(postData);

                var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                response.EnsureSuccessStatusCode();

                string responseString = await response.Content.ReadAsStringAsync();
                var recaptchaResult = JsonConvert.DeserializeObject<LoginViewModel>(responseString);

                if (recaptchaResult != null && recaptchaResult.Success)
                {
                    return true;
                }

                return false;
            }
        }


    }
} 

