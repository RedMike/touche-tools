using System.Text;
using Microsoft.Extensions.Logging;

namespace ToucheTools.Models;

public class TextDataModel
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(TextDataModel));
    
    private readonly int _size;
    private readonly string _data;

    public TextDataModel(int size, byte[] data)
    {
        _size = size;
        _data = Encoding.ASCII.GetString(data);

        if (_data.Length > _size)
        {
            _logger.Log(LogLevel.Warning, "Got text data of length {} but was supposed to be {}", _data.Length, _size);
        }
        _logger.Log(LogLevel.Information, "Text Data (size {}): {}", _size, _data);
    }
}