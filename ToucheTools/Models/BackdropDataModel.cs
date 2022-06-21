using Microsoft.Extensions.Logging;

namespace ToucheTools.Models;

public class BackdropDataModel
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(BackdropDataModel));
    private readonly int _width;
    private readonly int _height;

    public BackdropDataModel(int width, int height)
    {
        _width = width;
        _height = height;
        
        _logger.Log(LogLevel.Information, "Backdrop size: {}x{}", _width, _height);
    }
}