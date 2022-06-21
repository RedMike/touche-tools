using ToucheTools.Console;
using ToucheTools.Constants;
using ToucheTools.Loaders;

const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
var resourceLoader = new ResourceDataLoader(stream);
var spriteImageLoader = new SpriteImageDataLoader(stream, resourceLoader);
var programLoader = new ProgramDataLoader(stream, resourceLoader);

mainDataLoader.Read(out var textData, out var backdrop);

{
    spriteImageLoader.Read(Resources.MenuKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //menu kit data
    DebugImageSaver.Save(w, h, bytes, "menu_kit_data_debug");
}

{
    spriteImageLoader.Read(Resources.ConversionKitSpriteImage, false, out int w, out int h, out byte[,] bytes); //conv kit data
    DebugImageSaver.Save(w, h, bytes, "conv_kit_data_debug");
}

programLoader.Read(Game.StartupEpisode);
Console.Read();