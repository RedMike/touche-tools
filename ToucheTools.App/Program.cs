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

var windows = new IWindow[]
{
    settingsWindow,
    roomViewWindow,
    spriteViewSettingsWindow
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
    if (windowSettings.SpriteViewOpen)
    {
        RenderSpriteView(activeData, spriteViewSettings);
    }
    if (windowSettings.ProgramViewOpen)
    {
        RenderProgramViewSettings(activeData, programViewSettings);
        RenderProgramView(activeData, programViewSettings, programViewState);
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

void RenderSpriteView(ActiveData viewModel, SpriteViewSettings viewSettings)
{
    var viewW = Constants.MainWindowWidth;
    var viewH = 600.0f;
    ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Sprite View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    var centerCursorPos = new Vector2(viewW / 2.0f, viewH / 2.0f);
    
    #region Room background
    if (viewSettings.ShowRoom)
    {
        var ox = viewSettings.RoomOffsetX;
        var oy = viewSettings.RoomOffsetY;
        var (viewId, roomWidth, roomHeight, bytes) = viewModel.RoomView;
    
        var roomTexture = window.RenderImage(viewId, roomWidth, roomHeight, bytes);
        var uv1 = new Vector2(ox / (float)roomWidth, oy / (float)roomHeight);
        var uv2 = new Vector2((ox+viewW) / (float)roomWidth, (oy+viewH) / (float)roomHeight);
        ImGui.SetCursorPos(new Vector2(0.0f, 0.0f));
        ImGui.Image(roomTexture, new Vector2(viewW, viewH), uv1, uv2);
    }
    #endregion
    
    var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = viewModel.SpriteView;
    var spriteTexture = window.RenderImage(spriteViewId, spriteWidth, spriteHeight, spriteBytes);

    foreach (var (frameIndex, destX, destY, hFlip, vFlip) in viewSettings.PartsView)
    {
        var tileWidthRatio = (float)spriteTileWidth / spriteWidth;
        var tileHeightRatio = (float)spriteTileHeight / spriteHeight;
        var tilesPerRow = (int)Math.Floor((float)spriteWidth / spriteTileWidth);
        var tileX = frameIndex % tilesPerRow;
        var tileY = (int)Math.Floor((float)frameIndex / tilesPerRow);
        var spriteUv1 = new Vector2(tileX * tileWidthRatio, tileY * tileHeightRatio);
        var spriteUv2 = new Vector2((tileX + 1) * tileWidthRatio, (tileY + 1) * tileHeightRatio);
        if (hFlip)
        {
            (spriteUv1.X, spriteUv2.X) = (spriteUv2.X, spriteUv1.X);
        }
        if (vFlip)
        {
            (spriteUv1.Y, spriteUv2.Y) = (spriteUv2.Y, spriteUv1.Y);
        }
        ImGui.SetCursorPos(centerCursorPos + new Vector2(destX, destY));
        ImGui.Image(spriteTexture, new Vector2(spriteTileWidth, spriteTileHeight), spriteUv1, spriteUv2);   
    }
    
    ImGui.End();
}

void RenderProgramViewSettings(ActiveData viewModel, ProgramViewSettings viewSettings)
{
    var originalProgramId = viewSettings.Programs.FindIndex(k => k == viewSettings.ActiveProgram);
    var curProgramId = originalProgramId;
    var programs = viewSettings.Programs.ToArray();
    
    ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f));
    ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

    ImGui.Combo("Program", ref curProgramId, programs.Select(k => k.ToString()).ToArray(), programs.Length);
    if (curProgramId != originalProgramId)
    {
        viewSettings.SetActiveProgram(programs[curProgramId]);
    }
    
    ImGui.End();
}

void RenderProgramView(ActiveData viewModel, ProgramViewSettings viewSettings, ProgramViewState state)
{
    var viewW = 400.0f;
    var viewH = 600.0f;
    ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Program View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    state.OffsetToIndex = new Dictionary<int, int>();
    state.OffsetYPos = new Dictionary<int, float>();
    var idx = 0;
    foreach (var (offset, instruction) in viewSettings.InstructionsView)
    {
        var evaluatedAlready = idx <= viewSettings.EvaluateUntil;
        if (evaluatedAlready)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.6f, 0.8f, 1.0f));
        }

        state.OffsetYPos[offset] = ImGui.GetCursorPosY();
        state.OffsetToIndex[offset] = idx;
        if (ImGui.Button($"{offset:D5}"))
        {
            viewSettings.SetEvaluateUntil(idx);
        }
        ImGui.SameLine();
        ImGui.Text($" - {instruction}");
        if (evaluatedAlready)
        {
            ImGui.PopStyleColor();
        }

        idx++;
    }

    var scrollTo = state.GetQueuedScroll();
    if (scrollTo != null)
    {
        var scrollBack = ImGui.GetWindowHeight() / 2.0f;
        ImGui.SetScrollY(scrollTo.Value - scrollBack);
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