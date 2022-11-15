using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToucheTools;
using ToucheTools.App;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.App.Windows;
using ToucheTools.Loaders;
using ToucheTools.Models;

#region Setup
IServiceCollection container = new ServiceCollection();
container.AddLogging(o => o
    .ClearProviders()
    .SetMinimumLevel(LogLevel.Information)
    .AddConsole()
);
#endregion

#region Data setup
container.AddSingleton<LogData>();

container.AddSingleton<ActivePalette>();
container.AddSingleton<ActiveRoom>();
container.AddSingleton<ActiveSprite>();
container.AddSingleton<ActiveProgram>();
container.AddSingleton<ActiveSequence>();
container.AddSingleton<ActiveCharacter>();
container.AddSingleton<ActiveAnimation>();
container.AddSingleton<ActiveDirection>();
container.AddSingleton<ActiveFrame>();
container.AddSingleton<ActiveProgram>();
container.AddSingleton<MultiActiveRects>();
container.AddSingleton<MultiActiveBackgrounds>();
container.AddSingleton<MultiActiveAreas>();

container.AddSingleton<WindowSettings>();
container.AddSingleton<RoomViewSettings>();
container.AddSingleton<SpriteViewSettings>();
container.AddSingleton<ProgramViewSettings>();

container.AddSingleton<ProgramViewState>();
container.AddSingleton<SpriteViewState>();
container.AddSingleton<ActiveProgramState>();
#endregion

#region Windows
var windowTypes = AppDomain.CurrentDomain
    .GetAssemblies().SelectMany(a => 
        a.GetTypes().Where(t => 
            t.IsClass &&
            !t.IsAbstract &&
            !t.ContainsGenericParameters &&
            typeof(IWindow).IsAssignableFrom(t)
        )
    );
foreach (var type in windowTypes)
{
    container.AddSingleton(type);
}
#endregion

#region Load data
container.AddSingleton<DatabaseModel>(provider =>
{
    var fileToLoad = "../../../../sample/TOUCHE.DAT";
    if (!File.Exists(fileToLoad))
    {
        throw new Exception("File not found: " + fileToLoad);
    }
    var fileBytes = File.ReadAllBytes(fileToLoad);
    var memStream = new MemoryStream(fileBytes); //not disposed
    
    Logging.SetUp(provider.GetService<ILoggerFactory>() ?? throw new InvalidOperationException());
    var mainLoader = new MainLoader(memStream);
    mainLoader.Load(out var db);
    return db;
});
#endregion

#region Render setup
using var window = new RenderWindow("ToucheTools", Constants.MainWindowWidth, Constants.MainWindowHeight);
container.AddSingleton(window);
#endregion

var serviceProvider = container.BuildServiceProvider();
var windows = container
    .Where(s => typeof(IWindow).IsAssignableFrom(s.ServiceType))
    .Select(s => (IWindow)(serviceProvider.GetService(s.ServiceType) ?? throw new Exception("Null service")))
    .ToList();
var errors = new List<string>();
var db = serviceProvider.GetService<DatabaseModel>() ?? throw new InvalidOperationException();
if (db.FailedPrograms.Any())
{
    errors.AddRange(db.FailedPrograms.Select(pair => $"Program {pair.Key} - {pair.Value}"));
}

if (db.FailedRooms.Any())
{
    errors.AddRange(db.FailedRooms.Select(pair => $"Room {pair.Key} - {pair.Value}"));
}

if (db.FailedSprites.Any())
{
    errors.AddRange(db.FailedSprites.Select(pair => $"Sprite {pair.Key} - {pair.Value}"));
}

if (db.FailedIcons.Any())
{
    errors.AddRange(db.FailedIcons.Select(pair => $"Icon {pair.Key} - {pair.Value}"));
}

var logData = serviceProvider.GetService<LogData>() ?? throw new InvalidOperationException();
foreach (var err in errors)
{
    logData.Error(err);
}
logData.Info($"Finished loading {db.Programs.Count} programs, {db.Rooms.Count} rooms, {db.Sprites.Count} sprites.");

while (window.IsOpen())
{
    window.ProcessInput();
    if (!window.IsOpen())
    {
        break;
    }
    
    foreach (var win in windows)
    {
        win.Render();
    }
    
    window.Render();
}