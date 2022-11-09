using System.Numerics;
using ImGuiNET;
using ToucheTools.App;
using ToucheTools.Loaders;

#region Load data
var fileToLoad = "../../../../sample/TOUCHE.DAT";
var filename = Path.GetFileName(fileToLoad);
if (!File.Exists(fileToLoad))
{
    throw new Exception("File not found: " + fileToLoad);
}
var bytes = File.ReadAllBytes(fileToLoad);
using var memStream = new MemoryStream(bytes);
var mainLoader = new MainLoader(memStream);
mainLoader.Load(out var db);
#endregion

const int width = 800;
const int height = 600;
Vector4 errorColour = new Vector4(0.9f, 0.1f, 0.2f, 1.0f);

using var window = new RenderWindow("ToucheTools", width, height);

while (window.IsOpen())
{
    window.ProcessInput();
    if (!window.IsOpen())
    {
        break;
    }
    
    //render IMGUI components
    #region Errors
    var errors = new List<string>();
    if (db.FailedPrograms.Any())
    {
        errors.AddRange(db.FailedPrograms.Select(pair => $"Program {pair.Key} - {pair.Value}"));
    }

    if (db.FailedRooms.Any())
    {
        errors.AddRange(db.FailedRooms.Select(pair => $"Room {pair.Key} - {pair.Value}"));
    }

    if (db.FailedSprites.Any())
    {
        errors.AddRange(db.FailedSprites.Select(pair => $"Sprite {pair.Key} - {pair.Value}"));
    }

    if (db.FailedIcons.Any())
    {
        errors.AddRange(db.FailedIcons.Select(pair => $"Icon {pair.Key} - {pair.Value}"));
    }
    RenderErrors(errors);
    #endregion
    #region File info
    RenderFileInfo(filename, db.Programs.Count, db.Palettes.Count, db.Rooms.Count, db.RoomImages.Count, 
        db.Sprites.Count, db.Icons.Count);
    #endregion

    window.Render();
}

void RenderErrors(List<string> errors)
{
    ImGui.SetNextWindowPos(new Vector2(0.0f, height-100.0f));
    ImGui.SetNextWindowSize(new Vector2(width, 100.0f));
    ImGui.Begin("Errors", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    ImGui.PushStyleColor(ImGuiCol.Text, errorColour);
    foreach (var error in errors)
    {
        ImGui.TextWrapped(error);
    }
    ImGui.PopStyleColor();
    ImGui.End();
}

void RenderFileInfo(string fileName, int programCount, int paletteCount, int roomCount, int roomImageCount, int spriteCount, int iconCount)
{
    ImGui.SetNextWindowPos(new Vector2(0.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(150.0f, 150.0f));
    ImGui.Begin("File Info", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    ImGui.Text($"File: {fileName}");
    ImGui.Text($"# Programs:    {programCount}");
    ImGui.Text($"# Palettes:    {paletteCount}");
    ImGui.Text($"# Rooms:       {roomCount}");
    ImGui.Text($"# Room images: {roomImageCount}");
    ImGui.Text($"# Sprites:     {spriteCount}");
    ImGui.Text($"# Icons:       {iconCount}");
    ImGui.End();
}