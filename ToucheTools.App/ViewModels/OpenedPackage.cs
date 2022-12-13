using Newtonsoft.Json;
using ToucheTools.App.Models;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.ViewModels;

public class OpenedPackage : Observable<OpenedPackage.Manifest>
{
    #region Images
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
    
    public IEnumerable<string> GetAllImages()
    {
        return Files.Where(f => f.EndsWith(".png"));
    }

    public Dictionary<string, Image> GetIncludedImages()
    {
        return Value.Images;
    }
    
    private static ImageType GetDefaultImageType(string path)
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
    #endregion

    #region Animations
    public class Animation
    {
        public int Index { get; set; } = -1;
    }
    
    public IEnumerable<string> GetAllAnimations()
    {
        return Files.Where(f => f.EndsWith(".anim.json"));
    }

    public Dictionary<string, Animation> GetIncludedAnimations()
    {
        return Value.Animations;
    }
    #endregion

    #region Rooms
    public class Room
    {
        public int Index { get; set; } = -1;
    }
    public IEnumerable<string> GetAllRooms()
    {
        return Files.Where(f => f.EndsWith(".room.json"));
    }

    public Dictionary<string, Room> GetIncludedRooms()
    {
        return Value.Rooms;
    }
    #endregion

    #region Programs

    public enum ProgramType
    {
        Unknown = 0,
        Main = 1, //runs as key char 0 in foreground
        KeyChar = 2, //runs as a non-0 key char in background
        Action = 3, //runs in response to an action in foreground
    }
    
    public static List<string> ProgramTypeAsList()
    {
        return Enum.GetValues<ProgramType>()
            .OrderBy(i => i)
            .Select(i => i.ToString("G"))
            .ToList();
    }

    public class Program
    {
        public ProgramType Type { get; set; } = ProgramType.Unknown;
        public int Index { get; set; } = -1;
        public int Target { get; set; } = -1;
        public int Data { get; set; } = -1;
    }
    
    public IEnumerable<string> GetAllPrograms()
    {
        return Files.Where(f => f.EndsWith(".c.tsf"));
    }

    public Dictionary<string, Program> GetIncludedPrograms()
    {
        return Value.Programs;
    }
    
    private static ProgramType GetDefaultProgramType(string path)
    {
        if (path.Contains(".main."))
        {
            return ProgramType.Main;
        }

        if (path.Contains(".char."))
        {
            return ProgramType.KeyChar;
        }

        if (path.Contains(".action."))
        {
            return ProgramType.Action;
        }

        return ProgramType.Unknown;
    }
    #endregion
    
    #region Game
    public GameModel GetGame()
    {
        return Value.Game;
    }
    #endregion
    
    public class Manifest
    {
        public GameModel Game { get; set; } = new GameModel();
        public HashSet<string> IncludedFiles { get; set; } = new HashSet<string>();
        public Dictionary<string, Image> Images { get; set; } = new Dictionary<string, Image>();
        public Dictionary<string, Animation> Animations { get; set; } = new Dictionary<string, Animation>();
        public Dictionary<string, Room> Rooms { get; set; } = new Dictionary<string, Room>();
        public Dictionary<string, Program> Programs { get; set; } = new Dictionary<string, Program>();

        public void ExcludeFile(string path)
        {
            IncludedFiles.Remove(path);
            if (Images.ContainsKey(path))
            {
                Images.Remove(path);
            }
            if (Animations.ContainsKey(path))
            {
                Animations.Remove(path);
            }
            if (Rooms.ContainsKey(path))
            {
                Rooms.Remove(path);
            }
            if (Programs.ContainsKey(path))
            {
                Programs.Remove(path);
            }
        }

        public void IncludeFile(string path)
        {
            IncludedFiles.Add(path);
            if (path.EndsWith(".png"))
            {
                Images.Add(path, new Image()
                {
                    Type = GetDefaultImageType(path),
                    Index = -1
                });
            }
            if (path.EndsWith(".anim.json"))
            {
                Animations.Add(path, new Animation()
                {
                    Index = -1
                });
            }
            if (path.EndsWith(".room.json"))
            {
                Rooms.Add(path, new Room()
                {
                    Index = -1
                });
            }

            if (path.EndsWith(".c.tsf"))
            {
                Programs.Add(path, new Program()
                {
                    Type = GetDefaultProgramType(path),
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

    public void ForceUpdate() //TODO: this should not be necessary
    {
        SetValue(Value);
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
                            Type = GetDefaultImageType(f),
                            Index = -1
                        }
                    ),
                Animations = Files
                    .Where(f => f.EndsWith(".anim.json"))
                    .ToDictionary(
                        f => f,
                        f => new Animation()
                        {
                            Index = -1
                        }
                    ),
                Rooms = Files
                    .Where(f => f.EndsWith(".room.json"))
                    .ToDictionary(
                        f => f,
                        f => new Room()
                        {
                            Index = -1
                        }
                    ),
                Programs = Files
                    .Where(f => f.EndsWith(".c.tsf"))
                    .ToDictionary(
                        f => f,
                        f => new Program()
                        {
                            Type = GetDefaultProgramType(f),
                            Index = -1
                        }
                    ),
            };
        }
        else
        {
            //manifest exists, load it
            var manifestRaw = File.ReadAllText(ManifestPath);
            manifest = JsonConvert.DeserializeObject<Manifest>(manifestRaw) ??
                       throw new Exception("Failed to parse manifest");

            foreach (var file in manifest.IncludedFiles)
            {
                if (!Files.Contains(file))
                {
                    //TODO: warning about why
                    manifest.ExcludeFile(file);
                }
            }
        }
        
        SetValue(manifest);
    }

    private void Update()
    {
        //recalculate indexes
        //images
        var imageCounters = new Dictionary<ImageType, int>();
        foreach (var (_, image) in Value.Images)
        {
            if (!imageCounters.ContainsKey(image.Type))
            {
                imageCounters[image.Type] = 1;
                if (Value.Images.Any(i => i.Value.Type == image.Type && i.Value.Index >= 0))
                {
                    imageCounters[image.Type] = Value.Images.Where(i => i.Value.Type == image.Type && i.Value.Index >= 0)
                        .Select(i => i.Value.Index)
                        .Max();
                }
            }

            if (image.Index < 0)
            {
                image.Index = imageCounters[image.Type];
                imageCounters[image.Type]++;
            }
        }

        //animations
        var animCounter = 1;
        if (Value.Animations.Any(a => a.Value.Index >= 0))
        {
            animCounter = Value.Animations.Select(a => a.Value.Index).Max() + 1;
        }
        foreach (var (_, anim) in Value.Animations)
        {
            if (anim.Index < 0)
            {
                anim.Index = animCounter;
                animCounter++;
            }
        }
        
        //rooms
        var roomCounter = 1;
        if (Value.Rooms.Any(a => a.Value.Index >= 0))
        {
            roomCounter = Value.Rooms.Select(a => a.Value.Index).Max() + 1;
        }
        foreach (var (_, room) in Value.Rooms)
        {
            if (room.Index < 0)
            {
                room.Index = roomCounter;
                roomCounter++;
            }
        }
        
        //programs
        var programCounters = new Dictionary<ProgramType, int>();
        foreach (var (_, program) in Value.Programs)
        {
            if (!programCounters.ContainsKey(program.Type))
            {
                programCounters[program.Type] = 1;
                if (Value.Programs.Any(i => i.Value.Type == program.Type && i.Value.Index >= 0))
                {
                    programCounters[program.Type] = Value.Programs.Where(i => i.Value.Type == program.Type && i.Value.Index >= 0)
                        .Select(i => i.Value.Index)
                        .Max();
                }
            }

            if (program.Index < 0)
            {
                program.Index = programCounters[program.Type];
                programCounters[program.Type]++;
            }
        }
    }

    private static bool FilterFiles(string path)
    {
        //images
        if (path.EndsWith(".png"))
        {
            return true;
        }
        //animations
        if (path.EndsWith(".anim.json"))
        {
            return true;
        }
        //rooms
        if (path.EndsWith(".room.json"))
        {
            return true;
        }
        //programs
        if (path.EndsWith(".c.tsf"))
        {
            return true;
        }

        return false;
    }
}