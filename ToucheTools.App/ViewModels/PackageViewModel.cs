using SkiaSharp;

namespace ToucheTools.App.ViewModels;

public class PackageViewModel
{
    public enum FileType
    {
        Sprite = 0,
        Room = 1,
        Icon = 2
    }

    private readonly string[] _files;
    private int _selectedFileId; //for preview
    private readonly Dictionary<int, bool> _includeFiles = new Dictionary<int, bool>();
    private readonly Dictionary<int, FileType> _fileTypes = new Dictionary<int, FileType>();
    private readonly Dictionary<FileType, int> _fileTypeCounter = new Dictionary<FileType, int>();
    private readonly Dictionary<int, int> _fileIndexes = new Dictionary<int, int>();
    
    private readonly Dictionary<int, string> _fileMapping = new Dictionary<int, string>();
    private readonly Dictionary<int, (int, int, byte[])> _images = new Dictionary<int, (int, int, byte[])>();

    private Action _onUpdate = () => { };

    public PackageViewModel(string folder)
    {
        _files = Directory.EnumerateFiles(folder)
            .Where(p => p.EndsWith(".png"))
            .ToArray();
        _fileTypeCounter = new Dictionary<FileType, int>();
        for (var i = 0; i < _files.Length; i++)
        {
            _fileMapping[i] = _files[i];
            var defaultType = FileType.Room;
            if (_fileMapping[i].Contains("char"))
            {
                defaultType = FileType.Sprite;
            }
            if (_fileMapping[i].Contains("icon"))
            {
                defaultType = FileType.Icon;
            }
            if (!_fileTypeCounter.ContainsKey(defaultType))
            {
                _fileTypeCounter[defaultType] = 0;
            }
            _fileIndexes[i] = _fileTypeCounter[defaultType];
            _fileTypeCounter[defaultType]++;
            _fileTypes[i] = defaultType;
            
            var stream = File.Open(_files[i], FileMode.Open);
            var bitmap = SKBitmap.Decode(stream);
            _images[i] = (bitmap.Width, bitmap.Height, bitmap.Pixels
                .SelectMany(p => new[] { p.Red, p.Green, p.Blue, p.Alpha })
                .ToArray());
        }
        _selectedFileId = 0;
    }

    public void RegisterForUpdate(Action action)
    {
        _onUpdate += action;
    }

    public int GetSelectedIndex()
    {
        return _selectedFileId;
    }
    
    public string[] GetImages() {
        return _files.Select(Path.GetFileName).ToArray()!;
    }

    public (int, int, byte[]) GetImage(int id)
    {
        return _images[id];
    }

    public bool IsFileIncluded(int id)
    {
        return _includeFiles.GetValueOrDefault(id, true);
    }

    public FileType GetFileType(int id)
    {
        return _fileTypes[id];
    }

    public int GetFileIndex(int id)
    {
        return _fileIndexes[id];
    }

    public Dictionary<FileType, int> GetFileTypeCounts()
    {
        return _fileTypeCounter;
    }

    public void SetFileIncluded(int id, bool included)
    {
        _includeFiles[id] = included;
        _onUpdate();
    }

    public void SetFileType(int id, FileType type)
    {
        _fileTypes[id] = type;
        _onUpdate();
    }

    public void SetSelectedIndex(int id)
    {
        _selectedFileId = id;
        _onUpdate();
    }

    public void SetFileIndex(int id, int newIndex)
    {
        var fileType = _fileTypes[id];
        var otherFiles = _fileTypes.Where(p => p.Value == fileType).Select(p => p.Key);
        if (otherFiles.Select(f => _fileIndexes[f]).Contains(newIndex))
        {
            throw new Exception("Setting index to already-used index");
        }
        _fileIndexes[id] = newIndex;
        _onUpdate();
    }
}