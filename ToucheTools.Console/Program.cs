// See https://aka.ms/new-console-template for more information

using ToucheTools.Constants;
using ToucheTools.Loaders;

const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
var resourceLoader = new ResourceDataLoader(stream);


mainDataLoader.Read(out var textData, out var backdrop);
resourceLoader.Read(Resource.SpriteImage, 18, false, out var offset, out _); //menu kit data


Console.ReadKey();