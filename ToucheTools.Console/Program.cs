// See https://aka.ms/new-console-template for more information

using ToucheTools.Loaders;

const string path = @"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT";
var stream = File.OpenRead(path);
var mainDataLoader = new MainDataLoader(stream);
mainDataLoader.Read(out var textData, out var backdrop);

Console.ReadKey();