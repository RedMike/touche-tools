using ToucheTools.Web.Models;

namespace ToucheTools.Web.Services;

public interface IModelStorageService
{
    bool Exists(string sessionId);
    bool TryGetModels(string sessionId, out ModelContainer? container);
    string SaveNewSession(ModelContainer? container);
    void WipeSession(string sessionId);
}

public class ModelStorageService : IModelStorageService
{
    private const int MaxConcurrentEntries = 1000;
    private const int MaxConcurrentEntriesAfterClear = 900;
    private static readonly object Lock = new object();
    private readonly Dictionary<string, ModelContainer?> _storedSessions = new Dictionary<string, ModelContainer?>();
    
    private readonly ILogger _logger;

    public ModelStorageService(ILogger<ModelStorageService> logger)
    {
        _logger = logger;
    }

    public bool Exists(string sessionId)
    {
        return _storedSessions.TryGetValue(sessionId, out _);
    }

    public bool TryGetModels(string sessionId, out ModelContainer? container)
    {
        if (!_storedSessions.TryGetValue(sessionId, out container))
        {
            return false;
        }

        return true;
    }

    public string SaveNewSession(ModelContainer? container)
    {
        if (_storedSessions.Count > MaxConcurrentEntries)
        {
            lock (Lock)
            {
                //double-lock
                if (_storedSessions.Count > MaxConcurrentEntries)
                {
                    _logger.Log(LogLevel.Information, "Cleaning up stored sessions from {} to {}", 
                        _storedSessions.Count, MaxConcurrentEntriesAfterClear);
                    //wipe more than just the bare minimum to prevent more locking on next request
                    while (_storedSessions.Count > MaxConcurrentEntriesAfterClear)
                    {
                        var keyToRemove = _storedSessions.OrderBy(p => 
                            p.Value.UploadDate).First().Key;
                        _storedSessions.Remove(keyToRemove);
                    }
                    _logger.Log(LogLevel.Information, "Cleaned up stored sessions");
                }
            }
        }

        var id = Guid.NewGuid().ToString();
        _storedSessions[id] = container;
        _logger.Log(LogLevel.Information, "Saved new session {}", id);
        return id;
    }

    public void WipeSession(string sessionId)
    {
        _logger.Log(LogLevel.Information, "Wiped session {}", sessionId);
        _storedSessions.Remove(sessionId);
    }
}