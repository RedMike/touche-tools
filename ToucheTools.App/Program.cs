using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToucheTools.App;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.App.Windows;

//correctly allow (int, int, ...) tuples to be serialised/deserialised
TypeDescriptor.AddAttributes(typeof((Int32, Int32)), new TypeConverterAttribute(typeof(ValueTupleConverter<Int32, Int32>)));
TypeDescriptor.AddAttributes(typeof((Int32, Int32, Int32)), new TypeConverterAttribute(typeof(ValueTupleConverter<Int32, Int32, Int32>)));
TypeDescriptor.AddAttributes(typeof((Int32, Int32, Int32, Int32)), new TypeConverterAttribute(typeof(ValueTupleConverter<Int32, Int32, Int32, Int32>)));

IServiceCollection container = new ServiceCollection();
container.AddLogging(o => o
    .ClearProviders()
    .SetMinimumLevel(LogLevel.Information)
    .AddConsole()
);
container.AddSingleton<OpenedPath>();
container.AddSingleton<OpenedPackage>();
container.AddSingleton<OpenedManifest>();
container.AddSingleton<DebuggingGame>();

container.AddSingleton<ConfigService>();
container.AddSingleton<RunService>();

container.AddSingleton<PackagePublishService>();
container.AddSingleton<SpriteSheetRenderer>();

container.AddSingleton<MainWindowState>();
container.AddSingleton<ImageManagementState>();
container.AddSingleton<AnimationManagementState>();
container.AddSingleton<RoomManagementState>();
container.AddSingleton<ProgramManagementState>();
container.AddSingleton<GameManagementState>();
container.AddSingleton<PackageImages>();
container.AddSingleton<PackagePalettes>();
container.AddSingleton<PackageAnimations>();
container.AddSingleton<PackageRooms>();
container.AddSingleton<PackagePrograms>();

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
container.AddSingleton<MultiActiveRects>();
container.AddSingleton<MultiActiveBackgrounds>();
container.AddSingleton<MultiActiveAreas>();
container.AddSingleton<MultiActivePoints>();

container.AddSingleton<WindowSettings>();
container.AddSingleton<RoomViewSettings>();
container.AddSingleton<SpriteViewSettings>();
container.AddSingleton<ProgramViewSettings>();

container.AddSingleton<ProgramViewState>();
container.AddSingleton<SpriteViewState>();
container.AddSingleton<ActiveProgramState>();
container.AddSingleton<GameViewState>();

container.AddSingleton<RoomImageRenderer>();
container.AddSingleton<SpriteSheetRenderer>();
container.AddSingleton<IconImageRenderer>();

#region Windows
var windowTypes = AppDomain.CurrentDomain
    .GetAssemblies().SelectMany(a =>
        a.GetTypes().Where(t =>
            t.IsClass &&
            !t.IsAbstract &&
            !t.ContainsGenericParameters &&
            typeof(IWindow).IsAssignableFrom(t)
        )
    )
    .ToList();
foreach (var type in windowTypes)
{
    container.AddSingleton(type);
}
#endregion

using var window = new RenderWindow("ToucheTools", Constants.MainWindowWidth, Constants.MainWindowHeight);
container.AddSingleton(window);

var sp = container.BuildServiceProvider();
var windows = windowTypes
    .Select(s => (IWindow)(sp.GetService(s) ?? throw new Exception("Null service")))
    .ToList();

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
