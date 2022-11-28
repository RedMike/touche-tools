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
    private int _selectedFileIdx; //for preview
    private readonly Dictionary<int, bool> _includeFiles = new Dictionary<int, bool>();
    private readonly Dictionary<int, FileType> _fileTypes = new Dictionary<int, FileType>();
    
    private readonly Dictionary<int, string> _fileMapping = new Dictionary<int, string>();
    private readonly Dictionary<int, (int, int, byte[])> _images = new Dictionary<int, (int, int, byte[])>();

    private Action _onUpdate = () => { };

    public PackageViewModel(string folder)
    {
        _files = Directory.EnumerateFiles(folder)
            .Where(p => p.EndsWith(".png"))
            .ToArray();
        for (var i = 0; i < _files.Length; i++)
        {
            _fileMapping[i] = _files[i];
            var stream = File.Open(_files[i], FileMode.Open);
            var bitmap = SKBitmap.Decode(stream);
            _images[i] = (bitmap.Width, bitmap.Height, bitmap.Pixels
                .SelectMany(p => new[] { p.Red, p.Green, p.Blue, p.Alpha })
                .ToArray());
        }
        _selectedFileIdx = 0;
    }

    public void RegisterForUpdate(Action action)
    {
        _onUpdate += action;
    }

    public int GetSelectedIndex()
    {
        return _selectedFileIdx;
    }
    
    public string[] GetImages() {
        return _files.Select(Path.GetFileName).ToArray()!;
    }

    public (int, int, byte[]) GetImage(int index)
    {
        return _images[index];
    }

    public bool IsFileIncluded(int id)
    {
        return _includeFiles.GetValueOrDefault(id, true);
    }

    public FileType GetFileType(int id)
    {
        var defaultType = FileType.Room;
        if (_fileMapping[id].Contains("char"))
        {
            defaultType = FileType.Sprite;
        }
        if (_fileMapping[id].Contains("icon"))
        {
            defaultType = FileType.Icon;
        }

        return _fileTypes.GetValueOrDefault(id, defaultType);
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

    public void SetSelectedIndex(int index)
    {
        _selectedFileIdx = index;
        _onUpdate();
    }
}