﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Castle;
using Castle.Messages.Requests;
using CastleDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace CastleDemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly CastleClient _castleClient;

        public LoginModel(
            SignInManager<IdentityUser> signInManager, 
            ILogger<LoginModel> logger,
            ApplicationDbContext context,
            CastleClient castleClient)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _castleClient = castleClient;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);                

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Castle Authenticate $login.succeeded
                    _castleClient.Authenticate(CreateCastleActionRequest(Castle.Events.LoginSucceeded)).Forget();

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // Castle Track $login.failed
                    _castleClient.Track(CreateCastleActionRequest(Castle.Events.LoginFailed)).Forget();

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ActionRequest CreateCastleActionRequest(string castleEvent)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == Input.Email);

            return new ActionRequest()
            {
                Event = castleEvent,
                UserId = user?.Id,
                UserTraits = new Dictionary<string, string>()
                {
                    ["email"] = Input.Email
                    // We should also include "registered_at", but the template web app doesn't save that information
                },
                Context = Castle.Context.FromHttpRequest(Request)
            };
        }
    }
}