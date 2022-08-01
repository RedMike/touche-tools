using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ToucheTools.Web.Services;

public class ImagePackage
{
    public DateTime CreateDate { get; set; }
    public List<Rgb24> PotentialPalette { get; set; }
    public Image<Rgb24>? BackgroundImage { get; set; }
    public Image<Rgb24>? ProcessedBackgroundImage { get; set; }
    public Dictionary<string, Image<Rgb24>> OriginalGameImages { get; set; } = new Dictionary<string, Image<Rgb24>>();
    public Dictionary<string, Image<Rgb24>> ProcessedGameImages { get; set; } = new Dictionary<string, Image<Rgb24>>();
}

public interface IImagePackageStorageService
{
    bool TryGetPackage(string packageId, out ImagePackage? package);
    void UpdatePackage(string id, ImagePackage package);
    string SaveNewPackage(ImagePackage package);
}

public class ImagePackageStorageService : IImagePackageStorageService
{
    private const int MaxConcurrentEntries = 1000;
    private const int MaxConcurrentEntriesAfterClear = 900;
    private static readonly object Lock = new object();
    private readonly Dictionary<string, ImagePackage> _storedPackages = new Dictionary<string, ImagePackage>();
    
    private readonly ILogger _logger;

    public ImagePackageStorageService(ILogger<ImagePackageStorageService> logger)
    {
        _logger = logger;
    }

    public bool TryGetPackage(string packageId, out ImagePackage? package)
    {
        if (string.IsNullOrEmpty(packageId))
        {
            package = null;
            return false;
        }
        if (!_storedPackages.TryGetValue(packageId, out package))
        {
            return false;
        }

        return true;
    }

    public void UpdatePackage(string id, ImagePackage package)
    {
        if (!_storedPackages.ContainsKey(id))
        {
            throw new Exception("Unknown image package");
        }

        var createDate = _storedPackages[id].CreateDate;
        package.CreateDate = createDate;
        _storedPackages[id] = package;
    }

    public string SaveNewPackage(ImagePackage package)
    {
        if (_storedPackages.Count > MaxConcurrentEntries)
        {
            lock (Lock)
            {
                //double-lock
                if (_storedPackages.Count > MaxConcurrentEntries)
                {
                    _logger.Log(LogLevel.Information, "Cleaning up stored image packages from {} to {}", 
                        _storedPackages.Count, MaxConcurrentEntriesAfterClear);
                    //wipe more than just the bare minimum to prevent more locking on next request
                    while (_storedPackages.Count > MaxConcurrentEntriesAfterClear)
                    {
                        var keyToRemove = _storedPackages.OrderBy(p => 
                            p.Value.CreateDate).First().Key;
                        _storedPackages.Remove(keyToRemove);
                    }
                    _logger.Log(LogLevel.Information, "Cleaned up stored image packages");
                }
            }
        }

        var id = Guid.NewGuid().ToString();
        package.CreateDate = DateTime.UtcNow;
        _storedPackages[id] = package;
        _logger.Log(LogLevel.Information, "Saved new image package {}", id);
        return id;
    }
}