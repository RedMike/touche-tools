using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.ViewModels;

public class OpenedPath : Observable<string>
{
    public string LoadedPath => Value;
    
    public void LoadFolder(string folderPath)
    {
        if (!Path.EndsInDirectorySeparator(folderPath))
        {
            folderPath += Path.DirectorySeparatorChar;
        }
        var path = Path.GetDirectoryName(folderPath);
        if (path == null)
        {
            throw new Exception("Invalid path");
        }

        if (!Directory.Exists(path))
        {
            throw new Exception("Path folder does not exist");
        }
        
        SetValue(path);
    }
    
    public void Clear()
    {
        SetValue("");
    }
}