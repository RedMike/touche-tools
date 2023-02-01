using ToucheTools.Loaders;
using ToucheTools.Models;

const string doubleQuotes = "@@@";

Console.WriteLine($"ToucheTools.ModApp is a separate simplified editor application " +
                  $"which can be used to modify in-place a built TOUCHE.DAT file for specific " +
                  $"purposes: localisation, changing some graphics, etc.");
Console.WriteLine($"This application should only really be used when the source package is not available.");

Console.WriteLine("");

Console.Write("Enter path to TOUCHE.DAT file you want to modify (e.g. D:\\MyGame\\DATABASE\\TOUCHE.DAT): ");
var datFilePath = (Console.ReadLine() ?? "").Trim();
if (!File.Exists(datFilePath))
{
    Console.WriteLine("ERROR: Path does not exist.");
    Console.ReadKey();
    return;
}

var datName = Path.GetFileName(datFilePath);
var datPath = Path.GetFullPath(Path.GetDirectoryName(datFilePath) ?? "");
DatabaseModel db;
try
{
    var datContents = File.ReadAllBytes(datFilePath);
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

Console.WriteLine($"Loaded DAT file from: {datPath}");
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
    var id = $"{datName}-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}";
    //step 1, save data as CSV/images/etc
    Console.WriteLine("");
    Console.WriteLine("The data in the TOUCHE.DAT will be saved into the same folder.");
    
    //save main game strings as CSV
    if (db.Text != null && db.Text.Strings.Count > 0)
    {
        var fileName = $"game-strings_{id}.csv";
        var filePath = Path.Combine(datPath, fileName);
        var gameStrings = db.Text.Strings;
        var lines = gameStrings.Select(p => $"{p.Key},\"{p.Value.Replace("\"", doubleQuotes)}\"");
        File.WriteAllLines(filePath, lines);
        
        Console.WriteLine($"  Saved game-wide strings to: {fileName}");
    }

    if (db.Programs.Count > 0)
    {
        foreach (var (programId, program) in db.Programs)
        {
            if (program.Strings.Count == 0)
            {
                continue;
            }
            var fileName = $"program-strings_{programId}_{id}.csv";
            var filePath = Path.Combine(datPath, fileName);
            var programStrings = program.Strings;
            var lines = programStrings.Select(p => $"{p.Key},\"{p.Value.Replace("\"", doubleQuotes)}\"");
            File.WriteAllLines(filePath, lines);
        
            Console.WriteLine($"  Saved program {programId} strings to: {fileName}");
        }
    }

    Console.WriteLine("Please modify the files as needed, then run this application again and choose option 2.");
    Console.ReadKey();
    return;
}
else
{
    //pick which files to use
    //TODO: list files and determine valid suffixes
    var fileSuffix = "temp";
    
    var id = $"{fileSuffix}";
    //step 2, read in saved data, overwrite DatabaseModel fields, export
    Console.WriteLine("");
    Console.WriteLine("The data in the TOUCHE.DAT will be modified by the files in this folder, and a " +
                      $"TOUCHE_{id}.DAT file will be exported. " +
                      "Your original TOUCHE.DAT file will not be changed.");
    
    //TODO: load
    //TODO: modify
    //TODO: export
    
    Console.WriteLine($"The TOUCHE_{id}.DAT file was saved with your changes.");
    Console.ReadKey();
    return;
}