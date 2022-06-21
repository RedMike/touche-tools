// See https://aka.ms/new-console-template for more information

using ToucheTools.Loaders;

var mainDataLoader = new MainDataLoader(@"C:\Files\Projects\Decompiling\ToucheTools\sample\TOUCHE.DAT");
mainDataLoader.Read(out var textData, out var backdrop);

Console.ReadKey();