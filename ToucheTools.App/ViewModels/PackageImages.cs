using SkiaSharp;

namespace ToucheTools.App.ViewModels;

public class PackageImages
{
    private readonly OpenedManifest _manifest;

    private Dictionary<string, (int, int, byte[])> _images = null!;

    public PackageImages(OpenedManifest manifest)
    {
        _manifest = manifest;

        _manifest.Observe(Update);
        Update();
    }

    private void Update()
    {
        _images = new Dictionary<string, (int, int, byte[])>();
        if (!_manifest.IsLoaded())
        {
            return;
        }

        foreach (var path in _manifest.GetAllImages())
        {
            using var stream = _manifest.LoadFileStream(path);
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