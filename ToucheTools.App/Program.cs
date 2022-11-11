using System.Numerics;
using ImGuiNET;
using ToucheTools.App;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.Windows;
using ToucheTools.Loaders;

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
Vector4 errorColour = new Vector4(0.9f, 0.1f, 0.2f, 1.0f);
using var window = new RenderWindow("ToucheTools", Constants.MainWindowWidth, Constants.MainWindowHeight);
#endregion

#region Data setup
var windowSettings = new WindowSettings();
var spriteViewSettings = new SpriteViewSettings(db);
var programViewSettings = new ProgramViewSettings(db);
var activeData = new ActiveData(db);
#endregion

#region State setup
var programViewState = new ProgramViewState();
#endregion

#region Windows

var settingsWindow = new SettingsWindow(windowSettings);
var roomViewWindow = new RoomViewWindow(window, windowSettings, activeData);
var spriteViewSettingsWindow = new SpriteViewSettingsWindow(windowSettings, activeData, spriteViewSettings);
var spriteViewWindow = new SpriteViewWindow(window, windowSettings, activeData, spriteViewSettings);
var programViewSettingsWindow = new ProgramViewSettingsWindow(windowSettings, activeData, programViewSettings);
var programViewWindow = new ProgramViewWindow(windowSettings, activeData, programViewSettings, programViewState);

var windows = new IWindow[]
{
    settingsWindow,
    roomViewWindow,
    spriteViewSettingsWindow,
    spriteViewWindow,
    programViewSettingsWindow,
    programViewWindow
};
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

    foreach (var win in windows)
    {
        win.Render();
    }
    #region Active Objects
    if (windowSettings.RoomViewOpen || windowSettings.SpriteViewOpen)
    {
        RenderActiveObjects(activeData);
    }
    #endregion
    #region Windows
    if (windowSettings.ProgramViewOpen)
    {
        RenderProgramReferenceView(windowSettings, activeData, spriteViewSettings, programViewSettings, programViewState);
    }
    #endregion
    
    #region Boilerplate
    window.Render();
    #endregion
}

void RenderErrors(List<string> errors)
{
    ImGui.SetNextWindowPos(new Vector2(0.0f, Constants.MainWindowHeight-100.0f));
    ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth, 100.0f));
    ImGui.Begin("Errors", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    ImGui.PushStyleColor(ImGuiCol.Text, errorColour);
    foreach (var error in errors)
    {
        ImGui.TextWrapped(error);
    }
    ImGui.PopStyleColor();
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

    var originalSpriteId = viewModel.SpriteKeys.FindIndex(k => k == viewModel.ActiveSprite);
    var curSpriteId = originalSpriteId;
    var sprites = viewModel.SpriteKeys.ToArray();

    ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(200.0f, 200.0f));
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
    
    ImGui.Combo("Sprite", ref curSpriteId, sprites.Select(k => k.ToString()).ToArray(), sprites.Length);
    if (curSpriteId != originalSpriteId)
    {
        viewModel.SetActiveSprite(sprites[curSpriteId]);
    }
    
    ImGui.End();
}

void RenderProgramReferenceView(WindowSettings winSettings, ActiveData active, SpriteViewSettings spriteView, ProgramViewSettings viewSettings, ProgramViewState state)
{
    var viewW = 350.0f;
    var viewH = 600.0f;
    ImGui.SetNextWindowPos(new Vector2(400.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Program References", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    ImGui.Text("Rooms:");
    foreach (var room in viewSettings.ReferencedRoomsView)
    {
        ImGui.SameLine();
        if (ImGui.Button($"{room}"))
        {
            active.SetActiveRoom(room);
            active.SetActivePalette(room);
            winSettings.OpenRoomView();
        }
    }
    ImGui.Separator();
    ImGui.Text("Sprites (Sequence, Character, Animation):");
    foreach (var pair in viewSettings.ReferencedSpritesView.OrderBy(p => p.Key))
    {
        if (ImGui.TreeNodeEx(pair.Key.ToString(), ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var (seqId, charId, animId) in pair.Value)
            {
                if (ImGui.Button($"({seqId}, {charId}, {animId})"))
                {
                    active.SetActiveSprite(pair.Key);
                    spriteView.SetActiveSequence(seqId);
                    spriteView.SelectCharacter(charId);
                    spriteView.SelectAnimation(animId);
                    winSettings.OpenSpriteView();
                }
            }
            ImGui.TreePop();
        }
    }
    
    ImGui.Separator();
    ImGui.Text("Character Script Offsets:");
    foreach (var pair in viewSettings.CharacterScriptOffsetView.OrderBy(p => p.Key))
    {
        if (ImGui.Button($"{pair.Key} - {pair.Value:D5}"))
        {
            state.QueueScrollToOffset(pair.Value);
            programViewSettings.SetEvaluateUntil(state.OffsetToIndex[pair.Value]);
        }
    }
        
    ImGui.Separator();
    ImGui.Text("Action Script Offsets (Action, Obj1, Obj2):");
    foreach (var pair in viewSettings.ActionScriptOffsetView
                 .OrderBy(p => p.Key.Item1)
                 .ThenBy(p => p.Key.Item2)
                 .ThenBy(p => p.Key.Item3)
             )
    {
        if (ImGui.Button($"({pair.Key.Item1}, {pair.Key.Item2}, {pair.Key.Item3}) - {pair.Value:D5}"))
        {
            state.QueueScrollToOffset(pair.Value);
            programViewSettings.SetEvaluateUntil(state.OffsetToIndex[pair.Value]);
        }
    }
    
    ImGui.End();
}