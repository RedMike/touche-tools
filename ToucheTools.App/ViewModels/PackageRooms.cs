using Newtonsoft.Json;
using ToucheTools.App.Models;

namespace ToucheTools.App.ViewModels;

public class PackageRooms
{
    private readonly OpenedManifest _manifest;
    
    private Dictionary<string, RoomModel> _rooms = null!;

    public PackageRooms(OpenedManifest manifest)
    {
        _manifest = manifest;
        
        _manifest.Observe(Update);
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
        _manifest.WriteFile(path, data);
    }
    
    private void Update()
    {
        _rooms = new Dictionary<string, RoomModel>();
        if (!_manifest.IsLoaded())
        {
            return;
        }

        foreach (var path in _manifest.GetAllRooms())
        {
            var data = _manifest.LoadFile(path);
            var room = JsonConvert.DeserializeObject<RoomModel>(data) ??
                       throw new Exception("Failed to parse room data");
            _rooms[path] = room;
        }
    }
}