using ToucheTools.Models;

namespace ToucheTools.Web.Models;

public class ModelContainer
{
    public string InitialFilename { get; set; }
    public DateTime UploadDate { get; set; }
    
    public DatabaseModel DatabaseModel { get; set; }
}