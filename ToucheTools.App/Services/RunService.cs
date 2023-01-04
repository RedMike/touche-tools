using System.Diagnostics;

namespace ToucheTools.App.Services;

public class RunService
{
    private readonly ConfigService _configService;

    public RunService(ConfigService configService)
    {
        _configService = configService;
    }

    public void Run(string publishFolder)
    {
        var config = _configService.LoadConfig();
        if (string.IsNullOrEmpty(config.ExecutablePath))
        {
            //TODO: warning
            return;
        }
        
        var absolutePublishFolder = Path.GetFullPath(publishFolder);
        if (!Directory.Exists(absolutePublishFolder))
        {
            throw new Exception("Missing publish folder");
        }

        var exePath = config.ExecutablePath;
        var escapedPath = '"' + //wrap in double quotes 
                                 absolutePublishFolder
                                     .Replace("\\", "\\\\") //escape path separators
                                     .Replace("\"", "\\\"") //escape double-quotes
                                 + '"' //wrap in double quotes
                                 ;
        var argFormatString = string.Format(config.ExecutableArgumentFormatString, escapedPath);

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = exePath,
                WorkingDirectory = absolutePublishFolder,
                Arguments = argFormatString,
            }
        };
        process.Start();
        process.WaitForExit();
    }
}