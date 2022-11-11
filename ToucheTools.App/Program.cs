using Microsoft.Extensions.Logging;
using ToucheTools;
using ToucheTools.App;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.Windows;
using ToucheTools.Loaders;

#region Setup
var logData = new LogData();
var loggerFactory = LoggerFactory.Create(c => c
    .ClearProviders()
    .SetMinimumLevel(LogLevel.Information)
    .AddProvider(new LoggerProvider(LogLevel.Information, logData))
    .AddConsole()
);
Logging.SetUp(loggerFactory);
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
#endregion

#region Render setup
using var window = new RenderWindow("ToucheTools", Constants.MainWindowWidth, Constants.MainWindowHeight);
#endregion

#region Data setup
var windowSettings = new WindowSettings();
var spriteViewSettings = new SpriteViewSettings(db);
var programViewSettings = new ProgramViewSettings(db);
var activeData = new ActiveData(db);
#endregion

#region State setup
var programViewState = new ProgramViewState();
#endregion

#region Windows
var logWindow = new LogWindow(logData);
var settingsWindow = new SettingsWindow(windowSettings);
var activeObjectsWindow = new ActiveObjectsWindow(windowSettings, activeData);
var roomViewWindow = new RoomViewWindow(window, windowSettings, activeData);
var spriteViewSettingsWindow = new SpriteViewSettingsWindow(windowSettings, activeData, spriteViewSettings);
var spriteViewWindow = new SpriteViewWindow(window, windowSettings, activeData, spriteViewSettings);
var programViewSettingsWindow = new ProgramViewSettingsWindow(windowSettings, activeData, programViewSettings);
var programViewWindow = new ProgramViewWindow(windowSettings, activeData, programViewSettings, programViewState);
var programReferenceViewWindow = new ProgramReferenceViewWindow(windowSettings, activeData, spriteViewSettings,
    programViewSettings, programViewState);

var windows = new IWindow[]
{
    logWindow,
    settingsWindow,
    activeObjectsWindow,
    roomViewWindow,
    spriteViewSettingsWindow,
    spriteViewWindow,
    programViewSettingsWindow,
    programViewWindow,
    programReferenceViewWindow
};
#endregion

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