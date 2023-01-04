using Newtonsoft.Json;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.ViewModels;

public class OpenedPackage : Observable<OpenedPackage.PackageMarker>
{
    public class PackageMarker
    {
        /// <summary>
        /// Version of the package, for future-proofing
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Name of the package, for display purposes only
        /// </summary>
        public string Name { get; set; } = "";

        //TODO: authors, homepages, etc
    }
    private const int CurrentVersion = 1;
    
    private readonly OpenedPath _openedPath;
    private bool _loaded = false;
    
    public OpenedPackage(OpenedPath openedPath)
    {
        _openedPath = openedPath;

        _openedPath.Observe(Load);
        Load();
    }

    public PackageMarker LoadedPackage => Value;
    
    public bool IsLoaded()
    {
        return _loaded;
    }

    public void Save()
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

        //save the marker
        var markerData = Value;
        markerData.Version = CurrentVersion;
        var markerJson = JsonConvert.SerializeObject(markerData, Formatting.Indented);
        File.WriteAllText(Path.Combine(path, "package.tpf"), markerJson);
    }

    private void Load()
    {
        _loaded = false;

        var path = _openedPath.LoadedPath;
        
        //try and see if it's a known project first
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var newMarker = new PackageMarker();
        try
        {
            var markerPath = Path.Combine(path, "package.tpf");
            if (File.Exists(markerPath))
            {
                var markerData = File.ReadAllText(markerPath);
                var marker = JsonConvert.DeserializeObject<PackageMarker>(markerData);
                if (marker != null)
                {
                    if (marker.Version != CurrentVersion)
                    {
                        throw new Exception($"Reading package of version {marker.Version} which is unknown; only version {CurrentVersion} can be read");
                        //TODO: log a warning and refuse to load instead
                    }
                    newMarker = marker;
                }
            }
            //if marker does not exist, assume it's a new project and OK to load, manifest load will fail if needed
        
            _loaded = true;
        }
        finally
        {
            SetValue(newMarker);            
        }
    }
}