using Microsoft.Extensions.Hosting;
using Models;
using Statics;

namespace Services.Persistence;

public interface IGameStateService
{
    Task LoadSnapshotIntoMemory();
    Task<bool> SaveToSnapshot(EntityName entityName, object entity);
    IReadOnlyList<Player> GetPlayers();
}

public class GameStateService : IGameStateService
{
    private readonly ICloudflareKvService _kv;
    private readonly IHostEnvironment _env;
    private Snapshot _snapshot = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isKvAvailable = false;

    public GameStateService(ICloudflareKvService kv, IHostEnvironment env)
    {
        _kv = kv;
        _env = env;
    }

    public IReadOnlyList<Player> GetPlayers() => _snapshot.Players.AsReadOnly();

    public async Task LoadSnapshotIntoMemory()
    {
        try
        {
            var snapshot = await _kv.GetAsync<Snapshot>(GetKey());
            if (snapshot is not null)
            {
                _snapshot = snapshot;
                _isKvAvailable = true;
            }
        }
        catch (Exception)
        {
            _isKvAvailable = false;
            // TODO: log later
        }
    }

    public async Task<bool> SaveToSnapshot(EntityName entityName, object entity)
    {
        try
        {
            SaveEntity(entityName, entity);
        }
        catch (NotImplementedException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }

        return await PersistSnapshotToCloudflareKV(entityName);
    }

    private async Task<bool> PersistSnapshotToCloudflareKV(EntityName entityName)
    {
        if (!_isKvAvailable)
            return false;

        await _lock.WaitAsync();
        try
        {
            var saved = await _kv.SaveAsync(GetKey(), _snapshot);
            if (!saved)
                throw new Exception($"{Errors.SnapshotSaveFailed}{entityName}");
            return saved;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void SaveEntity(EntityName entityName, object entity)
    {
        switch (entityName)
        {
            case EntityName.Player:
                var player = entity as Player ?? throw new InvalidCastException($"{entityName}");
                var existing = _snapshot.Players.FirstOrDefault(p => p.Name == player.Name);
                if (existing is not null)
                    _snapshot.Players.Remove(existing);
                _snapshot.Players.Add(player);
                break;

            default:
                throw new NotImplementedException($"{Errors.EntityNameNotImplemented}{entityName}");
        }
    }

    private string GetKey() => _env.IsDevelopment() ? Helpers.SnapshotTest : Helpers.SnapshotProd;
}
