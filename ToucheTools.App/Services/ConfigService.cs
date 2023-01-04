using Newtonsoft.Json;
using ToucheTools.App.Config;

namespace ToucheTools.App.Services;

public class ConfigService
{
    private const string Path = "config.json";

    private int _cacheId = 0;
    private RunConfig? _cachedConfig = null;
    
    public RunConfig LoadConfig()
    {
        var fileContents = "";
        Exception? failed = null;
        try
        {
            fileContents = File.ReadAllText(Path);
        }
        catch (Exception e)
        {
            failed = e;
        }

        var cacheId = fileContents.GetHashCode(); //different between restarts but otherwise fairly consistent
        if (_cachedConfig != null && _cacheId == cacheId)
        {
            return _cachedConfig;
        }

        if (failed != null)
        {
            throw new Exception("Failed to load run config file", failed);
        }

        try
        {
            var obj = JsonConvert.DeserializeObject<RunConfig>(fileContents);
            if (obj == null)
            {
                throw new Exception("Run config null");
            }

            _cachedConfig = obj;
            _cacheId = cacheId;
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception("Unable to deserialise run config file", e);
        }
    }

    public void SaveConfig(RunConfig runConfig)
    {
        var json = JsonConvert.SerializeObject(runConfig);
        File.WriteAllText(Path, json);
    }
}