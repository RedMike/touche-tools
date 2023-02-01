using ToucheTools.Exporters;
using ToucheTools.Loaders;
using ToucheTools.Models;

const string doubleQuotes = "\\\"";
const string suffixSeparator = "__";
#if DEBUG
const bool debug = true;
#else
const bool debug = false;
#endif

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
    var memStream = new MemoryStream(datContents); //don't dispose so it's accessible later for lazy loading
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
    var id = $"{suffixSeparator}{datName}{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}";
    //step 1, save data as CSV/images/etc
    Console.WriteLine("");
    Console.WriteLine("The data in the TOUCHE.DAT will be saved into the same folder.");
    
    //save main game strings as CSV
    if (db.Text != null && db.Text.Strings.Count > 0)
    {
        var fileName = $"game-strings{id}.csv";
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
            var fileName = $"program-strings_{programId}{id}.csv";
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
    //step 2, read in saved data, overwrite DatabaseModel fields, export
    //pick which files to use
    var fullSuffixSeparator = $"{suffixSeparator}{datName}";
    var filesInFolder = Directory.EnumerateFiles(datPath).ToList();
    var validFiles = filesInFolder.Where(f => 
            (f.Trim().EndsWith(".csv") || f.Trim().EndsWith(".png")) &&
            f.Contains(fullSuffixSeparator)
        ).ToList();
    var validSuffixes = new HashSet<string>();
    foreach (var file in validFiles)
    {
        var potentialSuffix = file.Substring(
            file.LastIndexOf(fullSuffixSeparator, StringComparison.Ordinal) + fullSuffixSeparator.Length
            )
            .Replace(".csv", "")
            .Replace(".png", "");
        validSuffixes.Add(potentialSuffix);
    }

    if (validSuffixes.Count == 0)
    {
        Console.WriteLine("");
        Console.WriteLine("ERROR: no modification files in the folder, are you sure you ran step 1 first?");
        Console.ReadKey();
        return;
    }

    string fileSuffix = "";
    if (validSuffixes.Count == 1)
    {
        fileSuffix = validSuffixes.First();
    }
    else
    {
        if (validSuffixes.Count > 9)
        {
            Console.WriteLine("");
            Console.WriteLine("ERROR: too many different modification versions in the folder to " +
                              "display a menu, please clear them out and leave only your current changes" +
                              ", then try again");
            Console.ReadKey();
            return;
        }

        var orderedSuffixes = validSuffixes.OrderBy(s => s).ToList();
        Console.WriteLine("");
        Console.WriteLine("There are multiple sets of modification files in the folder, please select which one to use:");
        var i = 1;
        foreach (var suffix in orderedSuffixes)
        {
            Console.WriteLine($"    {i}. {suffix}");
            i++;
        }

        var chosenSuffixId = 0;
        do
        {
            var key = Console.ReadKey();
            if (!char.IsAsciiDigit(key.KeyChar))
            {
                continue;
            }

            var digit = int.Parse(key.KeyChar.ToString());
            if (digit == 0 || digit >= i)
            {
                continue;
            }

            chosenSuffixId = digit;
        } while (chosenSuffixId == 0);
        
        fileSuffix = orderedSuffixes[chosenSuffixId - 1];
    }
    
    var id = $"{fileSuffix}";
    var datExportFile = $"TOUCHE_{id}.DAT";
    Console.WriteLine("");
    Console.WriteLine($"The files marked {fileSuffix} will be used for modification.");
    Console.WriteLine("The data in the DAT file will be modified by the files in this folder, and a " +
                      $"new {datExportFile} file will be exported. " +
                      "Your original DAT file will not be changed.");
    
    //game strings
    if (db.Text != null)
    {
        var fileName = $"game-strings{fullSuffixSeparator}{id}.csv";
        var path = Path.Combine(datPath, fileName);
        if (File.Exists(path))
        {
            var gameStringsLines = File.ReadAllLines(path);
            foreach (var line in gameStringsLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!line.Contains(","))
                {
                    continue;
                }

                var stringId = int.Parse(line.Substring(0, line.IndexOf(',')));
                var stringVal = line.Substring(line.IndexOf(',') + 1);
                if (stringVal.StartsWith("\""))
                {
                    stringVal = stringVal.Substring(1);
                }
                if (stringVal.EndsWith("\""))
                {
                    stringVal = stringVal.Substring(0, stringVal.Length - 1);
                }
                stringVal = stringVal.Replace(doubleQuotes, "\"");

                if (stringVal != db.Text.Strings[stringId])
                {
                    if (debug)
                    {
                        Console.WriteLine($"    Found game string: {stringId} = '{stringVal}' instead of '{db.Text.Strings[stringId]}'");
                    }

                    db.Text.Strings[stringId] = stringVal;
                }
            }
        }
    }

    foreach (var (programId, program) in db.Programs)
    {
        if (program.Strings.Count == 0)
        {
            continue;
        }
        
        var fileName = $"program-strings_{programId}{fullSuffixSeparator}{id}.csv";
        var path = Path.Combine(datPath, fileName);
        if (File.Exists(path))
        {
            var programStringsLines = File.ReadAllLines(path);
            foreach (var line in programStringsLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!line.Contains(","))
                {
                    continue;
                }

                var stringId = int.Parse(line.Substring(0, line.IndexOf(',')));
                var stringVal = line.Substring(line.IndexOf(',') + 1);
                if (stringVal.StartsWith("\""))
                {
                    stringVal = stringVal.Substring(1);
                }
                if (stringVal.EndsWith("\""))
                {
                    stringVal = stringVal.Substring(0, stringVal.Length - 1);
                }
                stringVal = stringVal.Replace(doubleQuotes, "\"");

                if (stringVal != program.Strings[stringId])
                {
                    if (debug)
                    {
                        Console.WriteLine($"    Found program {programId} string: {stringId} = '{stringVal}' instead of '{program.Strings[stringId]}'");
                    }

                    program.Strings[stringId] = stringVal;
                }
            }
        }
    }

    Console.WriteLine("Got all overrides, ready to export.");
    if (debug)
    {
        Console.WriteLine("Debug mode, press enter to save");
        Console.ReadLine();
    }
    
    var memStream = new MemoryStream();
    var exporter = new MainExporter(memStream);
    exporter.Export(db);
    var bytes = memStream.ToArray();
    var datExportPath = Path.Join(datPath, datExportFile);
    File.WriteAllBytes(datExportPath, bytes);
    
    Console.WriteLine($"The {datExportFile} file was saved with your changes.");
    Console.ReadKey();
}