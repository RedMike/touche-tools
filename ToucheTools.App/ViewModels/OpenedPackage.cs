using Newtonsoft.Json;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.ViewModels;

public class OpenedPackage : Observable<OpenedPackage.Manifest>
{
    public enum ImageType
    {
        Unknown = 0,
        Sprite = 1,
        Room = 2,
        Icon = 3,
    }

    public static List<string> ImageTypeAsList()
    {
        return Enum.GetValues<ImageType>()
            .OrderBy(i => i)
            .Select(i => i.ToString("G"))
            .ToList();
    }
    
    public class Image
    {
        public ImageType Type { get; set; } = ImageType.Unknown;
        public int Index { get; set; } = -1;
    }

    public class Manifest
    {
        public HashSet<string> IncludedFiles { get; set; } = new HashSet<string>();
        public Dictionary<string, Image> Images { get; set; } = new Dictionary<string, Image>();

        public void ExcludeFile(string path)
        {
            IncludedFiles.Remove(path);
            if (Images.ContainsKey(path))
            {
                Images.Remove(path);
            }
        }

        public void IncludeFile(string path)
        {
            IncludedFiles.Add(path);
            if (path.EndsWith(".png"))
            {
                Images.Add(path, new Image()
                {
                    Type = GetDefaultType(path),
                    Index = -1
                });
            }
        }
    }

    public HashSet<string> Files { get; set; } = null!;
    
    private readonly string _path = "../../../../sample/assets"; //TODO: different default value
    public string ManifestPath => _path + "/manifest.json";
    
    public OpenedPackage()
    {
        Observe(Update);
        Load();
    }
    
    public bool IsLoaded()
    {
        return true;
    }

    public string GetLoadedPath()
    {
        return _path;
    }

    public void SaveManifest()
    {
        if (!IsLoaded())
        {
            return;
        }

        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        var manifestJson = JsonConvert.SerializeObject(Value, Formatting.Indented);
        File.WriteAllText(ManifestPath, manifestJson);
    }

    public IEnumerable<string> GetAllImages()
    {
        return Files.Where(f => f.EndsWith(".png"));
    }

    public Dictionary<string, Image> GetIncludedImages()
    {
        return Value.Images;
    }

    public void IncludeFile(string path)
    {
        Value.IncludeFile(path);
        SetValue(Value);
        
    }

    public void ExcludeFile(string path)
    {
        Value.ExcludeFile(path);
        SetValue(Value);
    }

    private void Load()
    {
        if (!IsLoaded())
        {
            return;
        }

        if (!Directory.Exists(_path))
        {
            return;
        }

        Files = Directory.EnumerateFiles(_path)
            .Where(FilterFiles)
            .ToHashSet();
        Manifest manifest;
        if (!File.Exists(ManifestPath))
        {
            //no manifest, generate one
            manifest = new Manifest()
            {
                IncludedFiles = Files,
                Images = Files
                    .Where(f => f.EndsWith(".png"))
                    .ToDictionary(
                        f => f,
                        f => new Image()
                        {
                            Type = GetDefaultType(f),
                            Index = -1
                        }
                    )
            };
        }
        else
        {
            //manifest exists, load it
            var manifestRaw = File.ReadAllText(ManifestPath);
            manifest = JsonConvert.DeserializeObject<Manifest>(manifestRaw);
        }
        
        SetValue(manifest);
    }

    private void Update()
    {
        //recalculate indexes
        var imageCounters = new Dictionary<ImageType, int>();
        foreach (var (_, image) in Value.Images)
        {
            if (!imageCounters.ContainsKey(image.Type))
            {
                imageCounters[image.Type] = 1;
            }
                
            if (image.Index == -1)
            {
                image.Index = imageCounters[image.Type];
            }
                
            imageCounters[image.Type]++;
        }
    }

    private static bool FilterFiles(string path)
    {
        //images
        if (path.EndsWith(".png"))
        {
            return true;
        }

        return false;
    }

    private static ImageType GetDefaultType(string path)
    {
        if (path.Contains("room"))
        {
            return ImageType.Room;
        }

        if (path.Contains("sprite"))
        {
            return ImageType.Sprite;
        }
        if (path.Contains("char"))
        {
            return ImageType.Sprite;
        }

        if (path.Contains("icon"))
        {
            return ImageType.Icon;
        }

        return ImageType.Unknown;
    }
}