using Newtonsoft.Json;
using ToucheTools.App.Config;

namespace ToucheTools.App.Services;

public class ConfigService
{
    private const string Path = "config.json";
    
    public RunConfig LoadConfig()
    {
        if (!File.Exists(Path))
        {
            return new RunConfig();
        }

        var json = File.ReadAllText(Path);
        var obj = JsonConvert.DeserializeObject<RunConfig>(json);
        if (obj == null)
        {
            throw new Exception("Unable to deserialise run config");
        }

        return obj;
    }

    public void SaveConfig(RunConfig runConfig)
    {
        var json = JsonConvert.SerializeObject(runConfig);
        File.WriteAllText(Path, json);
    }
}