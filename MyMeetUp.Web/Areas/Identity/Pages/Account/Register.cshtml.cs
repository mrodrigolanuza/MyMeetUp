using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;

namespace MyMeetUp.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Nombre")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Apellidos")]
            public string Surname { get; set; }

            [Required]
            [StringLength(9, MinimumLength = 9)]
            [RegularExpression(@"[0-9]{8}[A-Z]", ErrorMessage = "The DNI format is not valid.")]
            public string DNI { get; set; }

            [Phone]
            [Display(Name = "Teléfono")]
            public string Phone { get; set; }

            [Display(Name = "Ciudad")]
            public string City { get; set; }

            [Display(Name = "Pais")]
            public string Country { get; set; }

            [Display(Name = "LinkedIn")]
            [RegularExpression(@"^https:\/\/www\.linkedin\.com\/in\/.*", ErrorMessage = "The LinkedIn URL is not valid.")]
            public string LinkedIn { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { 
                    UserName = Input.Name, 
                    Name = Input.Name, 
                    Surname = Input.Surname,
                    DNI = Input.DNI,
                    Phone = Input.Phone,
                    PhoneNumber = Input.Phone,
                    City = Input.City,
                    Country = Input.Country,
                    LinkedIn = Input.LinkedIn,
                    Email = Input.Email,
                    EntryDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded) {
                    _logger.LogInformation("User created a new account with password.");

                    AssignRoleVisitorByDefault(user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void AssignRoleVisitorByDefault(ApplicationUser user) {
            _userManager.AddToRoleAsync(user, RolesData.Visitor).Wait();
        }
    }
}
