using System.Numerics;
using ImGuiNET;
using ToucheTools.App;
using ToucheTools.App.ViewModels;
using ToucheTools.Loaders;
using ToucheTools.Models;

#region Load data
var fileToLoad = "../../../../sample/TOUCHE.DAT";
var filename = Path.GetFileName(fileToLoad);
if (!File.Exists(fileToLoad))
{
    throw new Exception("File not found: " + fileToLoad);
}
var fileBytes = File.ReadAllBytes(fileToLoad);
using var memStream = new MemoryStream(fileBytes);
var mainLoader = new MainLoader(memStream);
mainLoader.Load(out var db);
#endregion

#region Render setup
const int width = 1024;
const int height = 900;
Vector4 errorColour = new Vector4(0.9f, 0.1f, 0.2f, 1.0f);
using var window = new RenderWindow("ToucheTools", width, height);
#endregion

#region Data setup
var activeData = new ActiveData(db);
#endregion

while (window.IsOpen())
{
    #region Boilerplate
    window.ProcessInput();
    if (!window.IsOpen())
    {
        break;
    }
    #endregion
    
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
    #region Active Objects
    RenderActiveObjects(activeData);
    #endregion
    #region Room View
    RenderRoomView(activeData);
    #endregion
    
    #region Boilerplate
    window.Render();
    #endregion
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
    ImGui.Begin("File Info", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs);
    ImGui.Text($"File: {fileName}");
    ImGui.Text($"# Programs:    {programCount}");
    ImGui.Text($"# Palettes:    {paletteCount}");
    ImGui.Text($"# Rooms:       {roomCount}");
    ImGui.Text($"# Room images: {roomImageCount}");
    ImGui.Text($"# Sprites:     {spriteCount}");
    ImGui.Text($"# Icons:       {iconCount}");
    ImGui.End();
}

void RenderActiveObjects(ActiveData viewModel)
{
    var originalPaletteId = viewModel.PaletteKeys.FindIndex(k => k == viewModel.ActivePalette); 
    var curPaletteId = originalPaletteId;
    var palettes = viewModel.PaletteKeys.ToArray();

    var originalRoomId = viewModel.RoomKeys.FindIndex(k => k == viewModel.ActiveRoom);
    var curRoomId = originalRoomId;
    var rooms = viewModel.RoomKeys.ToArray();
    
    ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(250.0f, 150.0f));
    ImGui.Begin("Active", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    
    ImGui.Combo("Palette", ref curPaletteId, palettes.Select(k => k.ToString()).ToArray(), palettes.Length);
    if (curPaletteId != originalPaletteId)
    {
        viewModel.SetActivePalette(palettes[curPaletteId]);
    }
    
    ImGui.Combo("Room", ref curRoomId, rooms.Select(k => k.ToString()).ToArray(), rooms.Length);
    if (curRoomId != originalRoomId)
    {
        viewModel.SetActiveRoom(rooms[curRoomId]);
    }
    
    ImGui.End();
}

void RenderRoomView(ActiveData viewModel)
{
    ImGui.SetNextWindowPos(new Vector2(0.0f, 150.0f));
    ImGui.SetNextWindowSize(new Vector2(width, 600.0f));
    ImGui.Begin("Room View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    var (viewId, roomWidth, roomHeight, bytes) = viewModel.RoomView;
    
    var roomTexture = window.RenderImage(viewId, roomWidth, roomHeight, bytes);
    ImGui.Image(roomTexture, new Vector2(roomWidth, roomHeight));
    
    ImGui.End();
}