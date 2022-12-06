using SkiaSharp;

namespace ToucheTools.App.ViewModels;

public class PackageImages
{
    private readonly OpenedPackage _package;

    private Dictionary<string, (int, int, byte[])> _images = null!;

    public PackageImages(OpenedPackage package)
    {
        _package = package;

        _package.Observe(Update);
        Update();
    }

    public void Update()
    {
        _images = new Dictionary<string, (int, int, byte[])>();

        foreach (var (path, _) in _package.LoadedManifest.Images)
        {
            var stream = File.Open(path, FileMode.Open);
            var bitmap = SKBitmap.Decode(stream);
            _images[path] = (bitmap.Width, bitmap.Height, bitmap.Pixels
                .SelectMany(p => new[] { p.Red, p.Green, p.Blue, p.Alpha })
                .ToArray());
        }
    }

    public (int, int, byte[]) GetImage(string path)
    {
        return _images[path];
    }
}