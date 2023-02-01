

using ToucheTools.Loaders;
using ToucheTools.Models;

Console.WriteLine($"ToucheTools.ModApp is a separate simplified editor application " +
                  $"which can be used to modify in-place a built TOUCHE.DAT file for specific " +
                  $"purposes: localisation, changing some graphics, etc.");
Console.WriteLine($"This application should only really be used when the source package is not available.");

Console.WriteLine("");

Console.Write("Enter path to TOUCHE.DAT file you want to modify (e.g. D:\\MyGame\\DATABASE\\TOUCHE.DAT): ");
var datPath = (Console.ReadLine() ?? "").Trim();
if (!File.Exists(datPath))
{
    Console.WriteLine("ERROR: Path does not exist.");
    Console.ReadKey();
    return;
}

DatabaseModel db;
try
{
    var datContents = File.ReadAllBytes(datPath);
    using var memStream = new MemoryStream(datContents);
    var mainLoader = new MainLoader(memStream);
    mainLoader.Load(out db);
}
catch (Exception e)
{
    Console.WriteLine("ERROR: Could not load DAT file.\nException: " + e);
    Console.ReadKey();
    return;
}

Console.WriteLine("Loaded TOUCHE.DAT.");
Console.WriteLine("");
Console.WriteLine("Please select from the following options (press number):");
Console.WriteLine("      1. Export the data from the file so it can be modified.");
Console.WriteLine("      2. Re-import the modified data and save a copy of the TOUCHE.DAT (your original file will not be overwritten).");
Console.WriteLine(""); 
bool? saving = null;
do
{
    var choice = Console.ReadKey();
    if (choice.KeyChar != '1' && choice.KeyChar != '2')
    {
        continue;
    }

    if (choice.KeyChar == '1')
    {
        saving = true;
    } else if (choice.KeyChar == '2')
    {
        saving = false;
    }
} while (saving == null);

if (saving == true)
{
    //step 1, save data as CSV/images/etc
    Console.WriteLine("");
    Console.WriteLine("The data in the TOUCHE.DAT will be saved into the same folder.");
    
    //TODO: save
    
    Console.WriteLine("Please modify the files as needed, then run this application again and choose option 2.");
    Console.ReadKey();
    return;
}
else
{
    //step 2, read in saved data, overwrite DatabaseModel fields, export
    Console.WriteLine("");
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
    Console.WriteLine("The data in the TOUCHE.DAT will be modified by the files in this folder, and a " +
                      $"TOUCHE_{timestamp}.DAT file will be exported. " +
                      "Your original TOUCHE.DAT file will not be changed.");
    
    //TODO: load
    //TODO: modify
    //TODO: export
    
    Console.WriteLine($"The TOUCHE_{timestamp}.DAT file was saved with your changes.");
    Console.ReadKey();
    return;
}