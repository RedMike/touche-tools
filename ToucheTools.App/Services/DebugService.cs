using ToucheTools.App.ViewModels;
using ToucheTools.Loaders;

namespace ToucheTools.App.Services;

public class DebugService
{
    private readonly LogData _log;
    private readonly DebuggingGame _game;

    public DebugService(LogData log, DebuggingGame game)
    {
        _log = log;
        _game = game;
    }

    public void Run(string publishFolder)
    {
        if (_game.IsLoaded())
        {
            _log.Error("Game already being debugged.");
            return;
        }
        
        _log.Info($"Loading in debugger: {publishFolder}");
        var datFile = Path.Combine(publishFolder, "DATABASE", "TOUCHE.DAT"); //TODO: custom filename
        if (!File.Exists(datFile))
        {
            _log.Error($"Failed to find DAT file: {datFile}");
            return;
        }

        var buffer = File.ReadAllBytes(datFile);
        var memStream = new MemoryStream(buffer.Length);
        memStream.Write(buffer);
        memStream.Seek(0, SeekOrigin.Begin);
        var mainLoader = new MainLoader(memStream);
        mainLoader.Load(out var db);

        var programCount = db.Programs.Count;
        var failedPrograms = db.FailedPrograms.Count;

        var spriteCount = db.Sprites.Count;
        var failedSprites = db.FailedSprites.Count;

        var roomCount = db.Rooms.Count;
        var failedRooms = db.FailedRooms;

        var iconCount = db.Icons.Count;
        var failedIcons = db.FailedIcons.Count;
        
        _log.Info($"Loaded game with: " +
                  $"\n{programCount} programs ({failedPrograms} failed)" +
                  $"\n{spriteCount} sprites ({failedSprites} failed)" +
                  $"\n{roomCount} rooms ({failedRooms.Count} failed)" +
                  $"\n{iconCount} icons ({failedIcons} failed)");
        
        _game.Load(db);
    }
}