using Microsoft.Extensions.Logging;
using ToucheTools.Console;
using ToucheTools.Constants;
using ToucheTools.Loaders;

var logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(Program));
const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
const string outputPath = "output";
Directory.CreateDirectory(outputPath);

var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
var resourceLoader = new ResourceDataLoader(stream);
var spriteImageLoader = new SpriteImageDataLoader(stream, resourceLoader);
var roomInfoLoader = new RoomInfoDataLoader(stream, resourceLoader);
var roomImageLoader = new RoomImageDataLoader(stream, resourceLoader);
var programLoader = new ProgramDataLoader(stream, resourceLoader);

// for (var i = 0; i < 255; i++)
// {
//     try
//     {
//         roomInfoLoader.Read(i, out var p, out var roomImageNum);
//         DebugPaletteSaver.Save($"{outputPath}/palette_{i}", p);
//         roomImageLoader.Read(roomImageNum, false, out var w, out var h, out var bytes);
//         if (w == 0 || h == 0) continue;
//         DebugImageSaver.Save(w, h, bytes, $"{outputPath}/room_{i}", p);
//     }
//     catch (Exception e)
//     {
//         //logger.LogError("Failed to read room info", e);
//     }
// }
//
// roomInfoLoader.Read(12, out var palette, out _);
// for (var i = 0; i < 255; i++)
// {
//     try
//     {
//         spriteImageLoader.Read(i, true, out int w, out int h, out byte[,] bytes); //menu kit data
//         if (w == 0 || h == 0) continue;
//         DebugImageSaver.Save(w, h, bytes, $"{outputPath}/sprite_{i}", palette);
//     }
//     catch (Exception e)
//     {
//         //logger.LogError("Failed to read sprite: {}", e);
//     }
// }

// mainDataLoader.Read(out var textData, out var backdrop);

// {
//     spriteImageLoader.Read(Resources.MenuKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //menu kit data
//     DebugImageSaver.Save(w, h, bytes, "menu_kit_data_debug", palette);
// }
//
// {
//     spriteImageLoader.Read(Resources.ConversionKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //conv kit data
//     DebugImageSaver.Save(w, h, bytes, "conv_kit_data_debug", palette);
// }
// {
//     spriteImageLoader.Read(0, false, out var w, out var h, out byte[,] bytes);
//     DebugImageSaver.Save(w, h, bytes, "0", palette);
// }

// {
//     spriteImageLoader.Read(50, false, out var w, out var h, out byte[,] bytes);
//     DebugImageSaver.Save(w, h, bytes, "50", palette);
// }
//
// {
//     spriteImageLoader.Read(10, false, out var w, out var h, out byte[,] bytes);
//     DebugImageSaver.Save(w, h, bytes, "10", palette);
// }
//
// programLoader.Read(Game.StartupEpisode, out var instructions);
// File.WriteAllLines("startup_instructions.csv", instructions);
// logger.Log(LogLevel.Information, "Wrote program instructions");



logger.Log(LogLevel.Information, "Finished");
Console.Read();