using Microsoft.AspNetCore.Http;
using Models;
using Services.Persistence;
using Services.Validation;
using Statics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Services.Auth;

public interface IAuthenticationService
{
    Task<List<string>> Register(RegisterRequest request);
    Task<List<string>> Login(LoginRequest request);
    Task Logout();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IGameStateService _gameState;
    private readonly IValidationsService _validations;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        IGameStateService gameState, 
        IValidationsService validations,
        IHttpContextAccessor httpContextAccessor)
    {
        _gameState = gameState;
        _validations = validations;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<string>> Register(RegisterRequest request)
    {
        var errors = _validations.ValidateModel(request);
        if (errors.Count > 0) return errors;

        var (thresholdValid, capError) = _validations.ValidateIsPlayerCountBelowThreshold();
        if (!thresholdValid) errors.Add(capError!);

        var (newPlayerValid, newPlayerError) = _validations.ValidateIsNewPlayer(request.Name);
        if (!newPlayerValid) errors.Add(newPlayerError!);

        if (errors.Count > 0) return errors;

        var player = new Player
        {
            Name = request.Name,
            HashedPassword = ValidationsService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _gameState.SaveToSnapshot(EntityName.Player, player);
        if (!saved) errors.Add(Errors.SnapshotSaveFailed);

        return errors;
    }

    public async Task<List<string>> Login(LoginRequest request)
    {
        var errors = _validations.ValidateModel(request);
        if (errors.Count > 0) return errors;

        var (player, playerError) = _validations.ValidateDoesPlayerExist(request.Name);
        if (player is null)
        {
            errors.Add(playerError!);
            return errors;
        }

        var (passwordValid, passwordError) = _validations.ValidateIsPasswordValid(request.Password, player.HashedPassword);
        if (!passwordValid) errors.Add(passwordError!);

        if (errors.Count > 0) return errors;

        var claims = new List<Claim> { new(ClaimTypes.Name, player.Name) };
        var identity = new ClaimsIdentity(claims, "AvelraanCookie");
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync("AvelraanCookie", principal);
        return errors;
    }

    public async Task Logout()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync("AvelraanCookie");
    }
}
