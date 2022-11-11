using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToucheTools;
using ToucheTools.App;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.Windows;
using ToucheTools.Loaders;

#region Setup
IServiceCollection container = new ServiceCollection();
var logData = new LogData();
var loggerFactory = LoggerFactory.Create(c => c
    .ClearProviders()
    .SetMinimumLevel(LogLevel.Information)
    .AddProvider(new LoggerProvider(LogLevel.Information, logData))
    .AddConsole()
);
Logging.SetUp(loggerFactory);

container.AddSingleton(logData);
container.AddSingleton(loggerFactory);
#endregion

#region Load data
var fileToLoad = "../../../../sample/TOUCHE.DAT";
var filename = Path.GetFileName(fileToLoad);
if (!File.Exists(fileToLoad))
{
    throw new Exception("File not found: " + fileToLoad);
}
var fileBytes = File.ReadAllBytes(fileToLoad);
using var memStream = new MemoryStream(fileBytes);
var mainLoader = new MainLoader(memStream);
mainLoader.Load(out var db);
container.AddSingleton(db);
#endregion

#region Render setup
using var window = new RenderWindow("ToucheTools", Constants.MainWindowWidth, Constants.MainWindowHeight);
container.AddSingleton(window);
#endregion

#region Data setup
container.AddSingleton<WindowSettings>();
container.AddSingleton<SpriteViewSettings>();
container.AddSingleton<ProgramViewSettings>();
container.AddSingleton<ActiveData>();

container.AddSingleton<ProgramViewState>();
#endregion

#region Windows
container.AddSingleton<LogWindow>();
container.AddSingleton<SettingsWindow>();
container.AddSingleton<ActiveObjectsWindow>();
container.AddSingleton<RoomViewWindow>();
container.AddSingleton<SpriteViewSettingsWindow>();
container.AddSingleton<SpriteViewWindow>();
container.AddSingleton<ProgramViewSettingsWindow>();
container.AddSingleton<ProgramViewWindow>();
container.AddSingleton<ProgramReferenceViewWindow>();
#endregion

var serviceProvider = container.BuildServiceProvider();
var windows = container
    .Where(s => typeof(IWindow).IsAssignableFrom(s.ServiceType))
    .Select(s => (IWindow)(serviceProvider.GetService(s.ServiceType) ?? throw new Exception("Null service")))
    .ToList();

var errors = new List<string>();
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

foreach (var err in errors)
{
    logData.Error(err);
}
logData.Info($"Finished loading {filename}, {db.Programs.Count} programs, {db.Rooms.Count} rooms, {db.Sprites.Count} sprites.");

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