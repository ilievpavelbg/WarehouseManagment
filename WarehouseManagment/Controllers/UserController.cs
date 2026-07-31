using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using WarehouseManagment.Data;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Models.User;

namespace WarehouseManagment.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReCaptchaSettings _reCaptchaSettings;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoginHistoryService loginHistoryService,
            IHttpContextAccessor httpContextAccessor,
            ReCaptchaSettings reCaptchaSettings,
            ILogger<UserController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loginHistoryService = loginHistoryService;
            _httpContextAccessor = httpContextAccessor;
            _reCaptchaSettings = reCaptchaSettings;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["RenderCaptcha"] = ShouldValidateCaptcha();

            var model = new RegisterViewModel
            {
                captchaSettings = _reCaptchaSettings
            };

            return View(model);
        }
        [Authorize(Policy = ApplicationPolicies.RequireAdministrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, [Bind(Prefix = "g-recaptcha-response")] string? recaptchaResponse)
        {
            ViewData["RenderCaptcha"] = ShouldValidateCaptcha();
            model.Response = recaptchaResponse;
            model.captchaSettings = _reCaptchaSettings;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ShouldValidateCaptcha() && !await VerifyRecaptcha(model.Response ?? string.Empty, model.captchaSettings.Secret))
            {
                ModelState.AddModelError("", "Трябва да потвърдите, че не сте робот.");
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
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ProtectedPageMessage"] = !string.IsNullOrWhiteSpace(returnUrl);
            ViewData["RenderCaptcha"] = ShouldValidateCaptcha();

            var model = new LoginViewModel
            {
                captchaSettings = _reCaptchaSettings
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl, [Bind(Prefix = "g-recaptcha-response")] string? recaptchaResponse)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ProtectedPageMessage"] = !string.IsNullOrWhiteSpace(returnUrl);
            ViewData["RenderCaptcha"] = ShouldValidateCaptcha();

            model.Response = recaptchaResponse;
            model.captchaSettings = _reCaptchaSettings;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ShouldValidateCaptcha() && !await VerifyRecaptcha(model.Response ?? string.Empty, model.captchaSettings.Secret))
            {
                ModelState.AddModelError("", "Трябва да потвърдите, че не сте робот.");
                return View(model);
            }

            var user = await FindUserForLoginAsync(model.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    await _loginHistoryService.UserLoginTime(user.Id);
                    _logger.LogInformation(
                        "User {UserName} logged in. Administrator role: {IsAdministrator}.",
                        user.UserName,
                        await _userManager.IsInRoleAsync(user, ApplicationRoles.Administrator));

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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _loginHistoryService.UserLogoutTime(userId);
            }

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home", new { area = "default" });
        }

        private async Task<ApplicationUser?> FindUserForLoginAsync(string userNameOrEmail)
        {
            var user = await _userManager.FindByNameAsync(userNameOrEmail);

            if (user != null)
            {
                return user;
            }

            user = await _userManager.FindByEmailAsync(userNameOrEmail);

            if (user != null)
            {
                return user;
            }

            var normalizedValue = userNameOrEmail.ToUpperInvariant();

            return _userManager.Users.FirstOrDefault(x =>
                x.UserName != null && x.UserName.ToUpper() == normalizedValue ||
                x.Email != null && x.Email.ToUpper() == normalizedValue ||
                x.NormalizedUserName != null && x.NormalizedUserName == normalizedValue ||
                x.NormalizedEmail != null && x.NormalizedEmail == normalizedValue);
        }

        private bool ShouldValidateCaptcha()
        {
            return !_webHostEnvironment.IsDevelopment();
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

