using Models;

namespace Services.Persistence;

public interface IGameStateService
{
    Snapshot GetSnapshot();
    Task Load(string key);
    Task<bool> Persist(string key);
}

public class GameStateService : IGameStateService
{
    private readonly ICloudflareKvService _kv;
    private Snapshot _snapshot = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public GameStateService(ICloudflareKvService kv)
    {
        _kv = kv;
    }

    public Snapshot GetSnapshot() => _snapshot;

    public async Task Load(string key)
    {
        var snapshot = await _kv.GetAsync<Snapshot>(key);
        if (snapshot is not null)
            _snapshot = snapshot;
    }

    public async Task<bool> Persist(string key)
    {
        await _lock.WaitAsync();
        try
        {
            return await _kv.SaveAsync(key, _snapshot);
        }
        finally
        {
            _lock.Release();
        }
    }
}
