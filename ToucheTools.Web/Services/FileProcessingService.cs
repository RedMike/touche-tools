using ToucheTools.Loaders;
using ToucheTools.Web.Models;

namespace ToucheTools.Web.Services;

public interface IFileProcessingService
{
    ModelContainer? Process(Stream stream);
}

public class FileProcessingService : IFileProcessingService
{
    public ModelContainer? Process(Stream stream)
    {
        var mainLoader = new MainLoader(stream);
        mainLoader.Load(out var db);
        return new ModelContainer()
        {
            DatabaseModel = db
        };
    }
}