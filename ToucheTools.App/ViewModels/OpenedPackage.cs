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
        Main = 1, //runs as key char 0 in foreground (always)
        KeyChar = 2, //runs as a non-0 key char in background (when key char is init'ed)
        Action = 3, //runs as key char 0 in foreground (when an action is done)
        Conversation = 4, //runs as key char 0 in foreground (when a conversation is started/choice picked)
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
        public int[] Data { get; set; } = new int[0];
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
    public Manifest LoadedManifest => Value;

    private readonly OpenedPath _openedPath;
    private bool _loaded = false;
    
    public OpenedPackage(OpenedPath openedPath)
    {
        _openedPath = openedPath;

        _openedPath.Observe(Load);
        
        Observe(Update);
        Load();
    }
    
    public bool IsLoaded()
    {
        return _loaded;
    }

    public void SaveManifest()
    {
        if (!IsLoaded())
        {
            return;
        }

        var path = _openedPath.LoadedPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var manifestJson = JsonConvert.SerializeObject(Value, Formatting.Indented);
        File.WriteAllText(path + "/manifest.json", manifestJson);
    }

    public void ForceUpdate() //TODO: this should not be necessary
    {
        SetValue(Value);
    }

    public FileStream LoadFileStream(string path)
    {
        var basePath = _openedPath.LoadedPath;
        var adjustedPath = RevertFilePath(basePath, path);
        return File.Open(adjustedPath, FileMode.Open);
    }

    public string LoadFile(string path)
    {
        var basePath = _openedPath.LoadedPath;
        var adjustedPath = RevertFilePath(basePath, path);
        return File.ReadAllText(adjustedPath);
    }

    public string[] LoadFileLines(string path)
    {
        var basePath = _openedPath.LoadedPath;
        var adjustedPath = RevertFilePath(basePath, path);
        return File.ReadAllLines(adjustedPath);
    }

    public void WriteFile(string path, string data)
    {
        var basePath = _openedPath.LoadedPath;
        var adjustedPath = RevertFilePath(basePath, path);
        File.WriteAllText(adjustedPath, data);
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
        _loaded = false;
        Files = new HashSet<string>();

        var path = _openedPath.LoadedPath;

        var newManifest = new Manifest();
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (!Directory.Exists(path))
            {
                return;
            }
            Files = Directory.EnumerateFiles(path)
                .Select(p => AdjustFilePaths(path, p))
                .Where(FilterFiles)
                .ToHashSet();
            var manifestPath = path + "/manifest.json";
            if (!File.Exists(manifestPath))
            {
                //no manifest, generate one
                newManifest = new Manifest()
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
                var manifestRaw = File.ReadAllText(manifestPath);
                newManifest = JsonConvert.DeserializeObject<Manifest>(manifestRaw) ??
                           throw new Exception("Failed to parse manifest");

                foreach (var file in newManifest.IncludedFiles)
                {
                    if (!Files.Contains(file))
                    {
                        //TODO: warning about why
                        newManifest.ExcludeFile(file);
                    }
                }
            }
            _loaded = true;
        }
        finally
        {
            SetValue(newManifest);
        }
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

    private static string RevertFilePath(string basePath, string path)
    {
        var separatorPrefix = "";
        if (!basePath.EndsWith("/") && !basePath.EndsWith("\\"))
        {
            separatorPrefix = "/"; //TODO: pick the right one?
        }
        
        path = basePath +
               separatorPrefix +
               path;
        
        return path;
    }

    private static string AdjustFilePaths(string basePath, string path)
    {
        path = path
            .Replace(basePath, "") //remove the path to the package
            .TrimStart('\\').TrimStart('/') //remove folder prefix if any
            .Replace("\\", "/") //standardise paths to one separator type
            ;
        
        return path;
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