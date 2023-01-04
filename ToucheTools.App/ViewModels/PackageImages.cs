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

    private void Update()
    {
        _images = new Dictionary<string, (int, int, byte[])>();
        if (!_package.IsLoaded())
        {
            return;
        }

        foreach (var path in _package.GetAllImages())
        {
            using var stream = _package.LoadFileStream(path);
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