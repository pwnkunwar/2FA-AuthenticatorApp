﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TwoFA_AuthenticatorApp.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        [BindProperty]
        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public string VerificationCode { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostEnable2FaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, VerificationCode);
            if(!isValid)
            {
                ModelState.AddModelError("VerificationCode", "Verification code is invalid.");
                return Page();
            }
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            StatusMessage = "Two-factor authentication has been disabled.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostGenerateRecoveryCodeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound($"Unable to load user eith ID '{_userManager.GetUserId(User)}'.");  
            }
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodesLeft = recoveryCodes.Count();
            StatusMessage = "Recovery codes have been generated.";
            return RedirectToPage(new {recoveryCodes});
        }

        private string FormatKey(string key)
            {
                return string.Join(" ", key?.Chunk(4) ?? Array.Empty<char[]>());
            }

    }
}
