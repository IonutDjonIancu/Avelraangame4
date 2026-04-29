using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Auth;

namespace Avelraangame4.Controllers;

[Controller]
[Route("account")]
public class AccountController : Controller
{
    private readonly IAuthenticationService _auth;

    public AccountController(IAuthenticationService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var errors = await _auth.Register(request);
        if (errors.Count > 0)
        {
            TempData["RegisterErrors"] = errors.ToArray();
            return LocalRedirect("/register");
        }
        return LocalRedirect("/login");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        var errors = await _auth.Login(request);
        if (errors.Count > 0)
        {
            TempData["LoginErrors"] = errors.ToArray();
            return LocalRedirect("/login");
        }
        return LocalRedirect("/");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _auth.Logout();
        return LocalRedirect("/login");
    }
}