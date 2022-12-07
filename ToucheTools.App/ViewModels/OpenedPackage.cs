using Newtonsoft.Json;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.ViewModels;

public class OpenedPackage : Observable<string>
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
    public Manifest LoadedManifest { get; set; } = null!;
    
    public string ManifestPath => Value + "/manifest.json";
    
    public OpenedPackage()
    {
        Observe(Update);
        
        SetValue("../../../../sample/assets");//TODO: different default value
    }
    
    public bool IsLoaded()
    {
        return !string.IsNullOrEmpty(Value);
    }

    public void SaveManifest()
    {
        if (!IsLoaded())
        {
            return;
        }

        if (!Directory.Exists(Value))
        {
            Directory.CreateDirectory(Value);
        }

        var manifestJson = JsonConvert.SerializeObject(LoadedManifest, Formatting.Indented);
        File.WriteAllText(ManifestPath, manifestJson);
    }

    public IEnumerable<string> GetAllImages()
    {
        return Files.Where(f => f.EndsWith(".png"));
    }

    public Dictionary<string, Image> GetIncludedImages()
    {
        return LoadedManifest.Images;
    }

    public void IncludeFile(string path)
    {
        LoadedManifest.IncludeFile(path);
        SaveManifest(); //TODO: shouldn't need to save?
        Update();
        
    }

    public void ExcludeFile(string path)
    {
        LoadedManifest.ExcludeFile(path);
        SaveManifest(); //TODO: shouldn't need to save?
        Update();
    }

    private void Update()
    {
        if (!IsLoaded())
        {
            return;
        }

        if (!Directory.Exists(Value))
        {
            return;
        }

        Files = Directory.EnumerateFiles(Value)
            .Where(FilterFiles)
            .ToHashSet();
        if (!File.Exists(ManifestPath))
        {
            //no manifest, generate one
            LoadedManifest = new Manifest()
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
            LoadedManifest = JsonConvert.DeserializeObject<Manifest>(manifestRaw);
        }
        
        //recalculate indexes
        var imageCounters = new Dictionary<ImageType, int>();
        foreach (var (_, image) in LoadedManifest.Images)
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