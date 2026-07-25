using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Infrastructure;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.Security.Claims;

namespace RealEstateApp.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IFileService _fileService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IAccountService accountService,
        IFileService fileService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _accountService = accountService;
        _fileService = fileService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                   ?? await _userManager.FindByNameAsync(model.UserNameOrEmail);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The access data is invalid.");
            return View(model);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            ModelState.AddModelError(string.Empty, "The access data is invalid.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "The user is inactive and cannot log in.");
            return View(model);
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "The user does not have a valid role assigned. Please contact an administrator.");
            return View(model);
        }

        if (roles.Contains("Developer"))
        {
            ModelState.AddModelError(string.Empty, "Developer users are not allowed to access the WebApp.");
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToRoleHome();
    }

    private IActionResult RedirectToRoleHome()
    {
        if (User.IsInRole("Client"))
            return RedirectToAction("Index", "Client");
        if (User.IsInRole("Agent"))
            return RedirectToAction("AgentHome", "AgentProperties");
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "AdminHome");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _accountService.ExistsByUserNameAsync(model.UserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "A user is already registered with this username.");
            return View(model);
        }

        if (await _accountService.ExistsByEmailAsync(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A user is already registered with this email address.");
            return View(model);
        }

        if (!_fileService.IsValidImage(model.ProfilePicture))
        {
            ModelState.AddModelError(nameof(model.ProfilePicture), "The selected file does not have a valid image format.");
            return View(model);
        }

        try
        {
            var picturePath = await _fileService.SaveImageAsync(model.ProfilePicture, "users");

            var dto = new RegisterClientAgentDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                ProfilePicturePath = picturePath,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                RoleName = model.RoleName
            };

            // Generar la URL base para la activación
            var activationBaseUrl = Url.Action(nameof(ActivateAccount), "Account", null, Request.Scheme);

            // Pasar ambos parámetros al servicio
            await _accountService.RegisterClientOrAgentAsync(dto, activationBaseUrl);

            if (model.RoleName == "Client")
            {
                TempData["SuccessMessage"] = "Your account has been created successfully. Please check your email to activate your account.";
            }
            else
            {
                TempData["SuccessMessage"] = "Your agent account has been created successfully. An administrator must activate your user before you can log in.";
            }

            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"It was not possible to complete the registration. {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateAccount(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Invalid activation link.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _accountService.ActivateAccountAsync(userId, token);

        if (result)
        {
            TempData["SuccessMessage"] = "Your account has been activated successfully. You can now log in.";
        }
        else
        {
            TempData["ErrorMessage"] = "Account activation failed or the account is already active.";
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View("~/Views/Shared/AccessDenied.cshtml");
    }
}