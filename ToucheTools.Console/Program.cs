using ToucheTools.Console;
using ToucheTools.Loaders;

const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
var resourceLoader = new ResourceDataLoader(stream);
var spriteImageLoader = new SpriteImageDataLoader(stream, resourceLoader);

mainDataLoader.Read(out var textData, out var backdrop);
spriteImageLoader.Read(18, false, out int w, out int h, out byte[,] bytes); //menu kit data

DebugImageSaver.Save(w, h, bytes, "menu_kit_data_debug");