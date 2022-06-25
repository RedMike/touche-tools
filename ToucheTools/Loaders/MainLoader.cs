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
        
        //programs
        for (var i = 0; i < Resources.DataInfo[Resource.Program].Count; i++)
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
                if (!e.Message.Contains("Null offset"))
                {
                    _logger.Log(LogLevel.Warning, exception: e, "Exception when loading program {}", i);
                    db.FailedPrograms[i] = e.Message;
                }
            }
        }
        
        object lockObj = new object(); //for lazy loading
        
        //sprites
        for (var i = 0; i < Resources.DataInfo[Resource.SpriteImage].Count; i++)
        {
            try
            {
                //try to read the sprite offset first to check if the sprite exists
                _resourceLoader.Read(Resource.SpriteImage, i, false, out _, out _);
                var localI = i;
                db.Sprites[i] = new Lazy<SpriteImageDataModel>(() =>
                {
                    lock (lockObj)
                    {
                        _spriteImageLoader.Read(localI, out var sprite);
                        return sprite;
                    }
                });
            }
            catch (UnknownResourceException)
            {
                //non-issue
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Null offset"))
                {
                    _logger.Log(LogLevel.Warning, exception: e, "Exception when loading sprite {}", i);
                    db.FailedSprites[i] = e.Message;
                }
            }
        }
        
        //TODO: rooms
        for (var i = 1; i < Resources.DataInfo[Resource.RoomInfo].Count + 1; i++) //no room 0
        {
            try
            {
                _roomInfoLoader.Read(i, out var palette, out var roomImage);
                db.Palettes[i] = palette;
                //try to read the sprite offset first to check if the image exists
                _resourceLoader.Read(Resource.RoomImage, roomImage, false, out _, out _);
                db.RoomImages[i] = new Lazy<RoomImageDataModel>(() =>
                {
                    lock (lockObj)
                    {
                        _roomImageLoader.Read(roomImage, out var roomImageModel);
                        return roomImageModel;
                    }
                });
            }
            catch (UnknownResourceException)
            {
                //non-issue
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Null offset"))
                {
                    _logger.Log(LogLevel.Warning, exception: e, "Exception when loading room {}", i);
                    db.FailedRooms[i] = e.Message;
                }
            }
        }
    }
}