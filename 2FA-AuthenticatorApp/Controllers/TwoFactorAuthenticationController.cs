using _TwoFA_AuthenticatorApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TwoFA_AuthenticatorApp.Models;

namespace _TwoFA_AuthenticatorApp.Controllers
{
    public class TwoFactorAuthenticationController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public TwoFactorAuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Handle the null user scenario (e.g., redirect to login page or show an error)
                return RedirectToAction("Login", "Account");
            }
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if(string.IsNullOrEmpty(authenticatorKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            var sharedKey = FormatKey(authenticatorKey);
            var qrCodeUri = GenerateQrCodeUri(user.Email, sharedKey);
            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = sharedKey,
                AuthenticatorUri = qrCodeUri
            };

            return View(model);
        }
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);

            if (!isValid)
            {
                ModelState.AddModelError("Code", "Invalid verification code.");
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["SuccessMessage"] = "Two-factor authentication has been enabled.";

            return RedirectToAction("Index", "Home");
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return $"otpauth://totp/MyApp:{email}?secret={unformattedKey}&issuer=MyApp&digits=6";
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;

            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
