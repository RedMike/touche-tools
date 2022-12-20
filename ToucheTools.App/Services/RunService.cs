using System.Diagnostics;

namespace ToucheTools.App.Services;

public class RunService
{
    private readonly ConfigService _configService;

    public RunService(ConfigService configService)
    {
        _configService = configService;
    }

    public void Run(string datFilePath)
    {
        var config = _configService.LoadConfig();
        if (string.IsNullOrEmpty(config.ExecutablePath))
        {
            //TODO: warning
            return;
        }

        var exePath = config.ExecutablePath;
        var escapedDatFilePath = '"' + datFilePath.Replace("\"", "\\\"") + '"';
        var argFormatString = string.Format(config.ExecutableArgumentFormatString, escapedDatFilePath);

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = argFormatString,
            }
        };
        process.Start();
        process.WaitForExit();
    }
}