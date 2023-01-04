using Newtonsoft.Json;
using ToucheTools.App.Models;

namespace ToucheTools.App.ViewModels;

public class PackageRooms
{
    private readonly OpenedPackage _package;
    
    private Dictionary<string, RoomModel> _rooms = null!;

    public PackageRooms(OpenedPackage package)
    {
        _package = package;
        
        _package.Observe(Update);
        Update();
    }

    public RoomModel GetRoom(string path)
    {
        return _rooms[path];
    }

    public void SaveRoom(string path)
    {
        var room = _rooms[path];
        var data = JsonConvert.SerializeObject(room, Formatting.Indented);
        _package.WriteFile(path, data);
    }
    
    private void Update()
    {
        _rooms = new Dictionary<string, RoomModel>();
        if (!_package.IsLoaded())
        {
            return;
        }

        foreach (var path in _package.GetAllRooms())
        {
            var data = _package.LoadFile(path);
            var room = JsonConvert.DeserializeObject<RoomModel>(data) ??
                       throw new Exception("Failed to parse room data");
            _rooms[path] = room;
        }
    }
}