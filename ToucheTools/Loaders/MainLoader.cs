using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class MainLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(MainLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    
    private readonly MainDataLoader _mainDataLoader;
    private readonly ResourceDataLoader _resourceLoader;
    private readonly SpriteImageDataLoader _spriteImageLoader;
    private readonly RoomInfoDataLoader _roomInfoLoader;
    private readonly RoomImageDataLoader _roomImageLoader;
    private readonly ProgramDataLoader _programLoader;

    public MainLoader(Stream stream)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _mainDataLoader = new MainDataLoader(stream);
        _resourceLoader = new ResourceDataLoader(stream);
        _spriteImageLoader = new SpriteImageDataLoader(stream, _resourceLoader);
        _roomInfoLoader = new RoomInfoDataLoader(stream, _resourceLoader);
        _roomImageLoader = new RoomImageDataLoader(stream, _resourceLoader);
        _programLoader = new ProgramDataLoader(stream, _resourceLoader);
    }

    public void Load(out DatabaseModel db)
    {
        //TODO: load everything
        db = new DatabaseModel();
        for (var i = 0; i < 255; i++)
        {
            try
            {
                _programLoader.Read(i, out var program);
                db.Programs[i] = program;
            }
            catch (UnknownResourceException)
            {
                // non-issue
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning, exception: e, "Exception when loading program {}", i);
                db.FailedPrograms[i] = e.Message;
            }
        }
    }
}