using Microsoft.Extensions.Logging;
using ToucheTools.Console;
using ToucheTools.Constants;
using ToucheTools.Loaders;

var logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(Program));
const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
var resourceLoader = new ResourceDataLoader(stream);
var spriteImageLoader = new SpriteImageDataLoader(stream, resourceLoader);
var roomInfoLoader = new RoomInfoDataLoader(stream, resourceLoader);
var programLoader = new ProgramDataLoader(stream, resourceLoader);

roomInfoLoader.Read(55, out var palette);
DebugPaletteSaver.Save("55", palette);

// mainDataLoader.Read(out var textData, out var backdrop);

{
    spriteImageLoader.Read(Resources.MenuKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //menu kit data
    DebugImageSaver.Save(w, h, bytes, "menu_kit_data_debug", palette);
}

{
    spriteImageLoader.Read(Resources.ConversionKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //conv kit data
    DebugImageSaver.Save(w, h, bytes, "conv_kit_data_debug", palette);
}
{
    spriteImageLoader.Read(0, false, out var w, out var h, out byte[,] bytes);
    DebugImageSaver.Save(w, h, bytes, "0", palette);
}

{
    spriteImageLoader.Read(50, false, out var w, out var h, out byte[,] bytes);
    DebugImageSaver.Save(w, h, bytes, "50", palette);
}

{
    spriteImageLoader.Read(10, false, out var w, out var h, out byte[,] bytes);
    DebugImageSaver.Save(w, h, bytes, "10", palette);
}
//
// programLoader.Read(Game.StartupEpisode, out var instructions);
// File.WriteAllLines("startup_instructions.csv", instructions);
// logger.Log(LogLevel.Information, "Wrote program instructions");



logger.Log(LogLevel.Information, "Finished");
Console.Read();