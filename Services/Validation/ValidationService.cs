using Models;
using Services.Persistence;
using Statics;
using System.ComponentModel.DataAnnotations;

namespace Services.Validation;

public interface IValidationsService
{
    #region player
    (Player? player, string? error) ValidateDoesPlayerExist(string name);
    (bool success, string? error) ValidateIsNewPlayer(string name);
    (bool success, string? error) ValidateIsPlayerCountBelowThreshold();
    (bool success, string? error) ValidateIsPasswordValid(string rawPassword, string hashedPassword);
    List<string> ValidateModel(object model);

    #endregion
}

public class ValidationsService : IValidationsService
{
    private readonly IGameStateService _gameState;

    public ValidationsService(IGameStateService gameState)
    {
        _gameState = gameState;
    }

    #region player
    public (Player? player, string? error) ValidateDoesPlayerExist(string name)
    {
        try
        {
            var player = _gameState.GetPlayers().FirstOrDefault(p => p.Name == name);

            if (player is null)
                return (null, $"{Errors.PlayerNotFound}{name}");

            return (player, null);
        }
        catch (Exception ex)
        {
            // TODO: log error
            return (null, ex.Message);
        }
    }

    public (bool success, string? error) ValidateIsNewPlayer(string name)
    {
        try
        {
            var (player, _) = ValidateDoesPlayerExist(name);
            if (player is not null)
                return (false, $"{Errors.PlayerAlreadyExists}{name}");

            return (true, null);
        }
        catch (Exception ex)
        {
            // TODO: log error
            return (false, ex.Message);
        }
    }

    public (bool success, string? error) ValidateIsPlayerCountBelowThreshold()
    {
        try
        {
            if (_gameState.GetPlayers().Count >= 10)
                return (false, Errors.PlayerCapReached);
            return (true, null);
        }
        catch (Exception ex)
        {
            // TODO: log error
            return (false, ex.Message);
        }
    }

    public (bool success, string? error) ValidateIsPasswordValid(string rawPassword, string hashedPassword)
    {
        try
        {
            var hash = HashPassword(rawPassword);
            if (hash != hashedPassword)
                return (false, Errors.InvalidPassword);
            return (true, null);
        }
        catch (Exception ex)
        {
            // TODO: log error
            return (false, ex.Message);
        }
    }

    public static string HashPassword(string password)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public List<string> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results.ConvertAll(r => r.ErrorMessage ?? string.Empty);
    }
    #endregion
}
