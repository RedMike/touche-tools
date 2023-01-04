using Newtonsoft.Json;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class PackageAnimations
{
    private readonly OpenedManifest _manifest;
    
    private Dictionary<string, SequenceDataModel> _animations = null!;

    public PackageAnimations(OpenedManifest manifest)
    {
        _manifest = manifest;
        
        _manifest.Observe(Update);
        Update();
    }

    public SequenceDataModel GetAnimation(string path)
    {
        return _animations[path];
    }

    public void SaveAnimation(string path)
    {
        var animation = _animations[path];
        var data = JsonConvert.SerializeObject(animation, Formatting.Indented);
        _manifest.WriteFile(path, data);
    }
    
    private void Update()
    {
        _animations = new Dictionary<string, SequenceDataModel>();
        if (!_manifest.IsLoaded())
        {
            return;
        }

        foreach (var path in _manifest.GetAllAnimations())
        {
            var data = _manifest.LoadFile(path);
            var sequence = JsonConvert.DeserializeObject<SequenceDataModel>(data) ??
                           throw new Exception("Failed to parse animation data");
            _animations[path] = sequence;
        }
    }
}