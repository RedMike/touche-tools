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
        //load stream into stored memory stream we can store
        var memoryStream = new MemoryStream(); //not collected
        stream.CopyTo(memoryStream);
        var mainLoader = new MainLoader(memoryStream);
        mainLoader.Load(out var db);
        return new ModelContainer()
        {
            LoadedBuffer = memoryStream,
            DatabaseModel = db
        };
    }
}