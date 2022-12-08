using Newtonsoft.Json;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class PackageAnimations
{
    private readonly OpenedPackage _package;
    
    private Dictionary<string, SequenceDataModel> _animations = null!;

    public PackageAnimations(OpenedPackage package)
    {
        _package = package;
        
        _package.Observe(Update);
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
        File.WriteAllText(path, data);
    }
    
    private void Update()
    {
        _animations = new Dictionary<string, SequenceDataModel>();
        if (!_package.IsLoaded())
        {
            return;
        }

        foreach (var path in _package.GetAllAnimations())
        {
            var data = File.ReadAllText(path);
            var sequence = JsonConvert.DeserializeObject<SequenceDataModel>(data);
            _animations[path] = sequence;
        }
    }
}