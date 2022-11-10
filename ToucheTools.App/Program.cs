using System.Numerics;
using ImGuiNET;
using ToucheTools.App;
using ToucheTools.App.ViewModels;
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
const int width = 1024;
const int height = 900;
Vector4 errorColour = new Vector4(0.9f, 0.1f, 0.2f, 1.0f);
using var window = new RenderWindow("ToucheTools", width, height);
#endregion

#region Data setup
var windowSettings = new WindowSettings();
var spriteViewSettings = new SpriteViewSettings(db);
var programViewSettings = new ProgramViewSettings(db);
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
    #region Window Settings
    RenderWindowSettings(windowSettings);
    #endregion
    #region Active Objects
    if (windowSettings.RoomViewOpen || windowSettings.SpriteViewOpen)
    {
        RenderActiveObjects(activeData);
    }
    #endregion
    #region Windows
    if (windowSettings.RoomViewOpen)
    {
        RenderRoomView(activeData);
    }
    if (windowSettings.SpriteViewOpen)
    {
        RenderSpriteViewSettings(activeData, spriteViewSettings);
        RenderSpriteView(activeData, spriteViewSettings);
    }
    if (windowSettings.ProgramViewOpen)
    {
        RenderProgramViewSettings(activeData, programViewSettings);
        RenderProgramReferenceView(programViewSettings);
        RenderProgramView(activeData, programViewSettings);
    }
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

void RenderWindowSettings(WindowSettings settings)
{
    bool origRoomViewShown = settings.RoomViewOpen;
    bool roomViewShown = origRoomViewShown;

    bool origSpriteViewShown = settings.SpriteViewOpen;
    bool spriteViewShown = origSpriteViewShown;

    bool origProgramViewShown = settings.ProgramViewOpen;
    bool programViewShown = origProgramViewShown;
    
    ImGui.SetNextWindowPos(new Vector2(0.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(150.0f, 200.0f));
    ImGui.Begin("Windows", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    ImGui.Checkbox("Room View", ref roomViewShown);
    if (roomViewShown != origRoomViewShown)
    {
        settings.CloseAllViews();
        if (roomViewShown)
        {
            settings.OpenRoomView();
        }
    }
    ImGui.Checkbox("Sprite View", ref spriteViewShown);
    if (spriteViewShown != origSpriteViewShown)
    {
        settings.CloseAllViews();
        if (spriteViewShown)
        {
            settings.OpenSpriteView();
        }
    }
    ImGui.Checkbox("Program View", ref programViewShown);
    if (programViewShown != origProgramViewShown)
    {
        settings.CloseAllViews();
        if (programViewShown)
        {
            settings.OpenProgramView();
        }
    }
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

void RenderRoomView(ActiveData viewModel)
{
    ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(width, 600.0f));
    ImGui.Begin("Room View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    var (viewId, roomWidth, roomHeight, bytes) = viewModel.RoomView;
    
    var roomTexture = window.RenderImage(viewId, roomWidth, roomHeight, bytes);
    ImGui.Image(roomTexture, new Vector2(roomWidth, roomHeight));
    
    ImGui.End();
}

void RenderSpriteView(ActiveData viewModel, SpriteViewSettings viewSettings)
{
    var viewW = width;
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

void RenderSpriteViewSettings(ActiveData viewModel, SpriteViewSettings viewSettings)
{
    viewSettings.Tick(); //TODO: move
    
    var origShowRoom = viewSettings.ShowRoom;
    var showRoom = origShowRoom;

    var origAutoStep = viewSettings.AutoStepFrame;
    var autoStep = origAutoStep;
    
    var originalSequenceId = viewSettings.SequenceKeys.FindIndex(k => k == viewSettings.ActiveSequence);
    var curSequenceId = originalSequenceId;
    var sequences = viewSettings.SequenceKeys.ToArray();
    
    var originalCharacterId = viewSettings.Characters.FindIndex(k => k == viewSettings.ActiveCharacter);
    var curCharacterId = originalCharacterId;
    var characters = viewSettings.Characters.ToArray();
    
    var originalAnimationId = viewSettings.Animations.FindIndex(k => k == viewSettings.ActiveAnimation);
    var curAnimationId = originalAnimationId;
    var animations = viewSettings.Animations.ToArray();
    
    var originalDirectionId = viewSettings.Directions.FindIndex(k => k == viewSettings.ActiveDirection);
    var curDirectionId = originalDirectionId;
    var directions = viewSettings.Directions.ToArray();
    
    var originalFrameId = viewSettings.Frames.FindIndex(k => k == viewSettings.ActiveFrame);
    var curFrameId = originalFrameId;
    var frames = viewSettings.Frames.ToArray();
    
    ImGui.SetNextWindowPos(new Vector2(350.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(width-500.0f, 200.0f));
    ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

    ImGui.Combo("Sequence", ref curSequenceId, sequences.Select(k => k.ToString()).ToArray(), sequences.Length);
    if (curSequenceId != originalSequenceId)
    {
        viewSettings.SetActiveSequence(sequences[curSequenceId]);
    }
    
    ImGui.SetNextItemWidth((width-500.0f)/4.0f);
    ImGui.Combo("Character", ref curCharacterId, characters.Select(k => k.ToString()).ToArray(), characters.Length);
    if (curCharacterId != originalCharacterId)
    {
        viewSettings.SelectCharacter(characters[curCharacterId]);
    }

    ImGui.SetNextItemWidth((width-500.0f)/4.0f);
    ImGui.SameLine();
    ImGui.Combo("Animation", ref curAnimationId, animations.Select(k => k.ToString()).ToArray(), animations.Length);
    if (curAnimationId != originalAnimationId)
    {
        viewSettings.SelectAnimation(animations[curAnimationId]);
    }
    
    ImGui.SetNextItemWidth((width-500.0f)/4.0f);
    ImGui.Combo("Direction", ref curDirectionId, directions.Select(k => k.ToString()).ToArray(), directions.Length);
    if (curDirectionId != originalDirectionId)
    {
        viewSettings.SelectDirection(directions[curDirectionId]);
    }
    
    ImGui.SetNextItemWidth((width-500.0f)/4.0f);
    ImGui.SameLine();
    ImGui.Combo("Frame", ref curFrameId, frames.Select(k => k.ToString()).ToArray(), frames.Length);
    if (curFrameId != originalFrameId)
    {
        viewSettings.SelectFrame(frames[curFrameId]);
    }
    
    ImGui.Checkbox("Auto step", ref autoStep);
    if (autoStep != origAutoStep)
    {
        viewSettings.AutoStepFrame = autoStep;
    }
    
    
    ImGui.Checkbox("Room background", ref showRoom);
    if (showRoom != origShowRoom)
    {
        viewSettings.ShowRoom = showRoom;
    }

    if (showRoom)
    {
        var (_, roomW, roomH, _) = viewModel.RoomView;
        
        var origRoomX = viewSettings.RoomOffsetX;
        if (origRoomX > roomW)
        {
            origRoomX = roomW;
        }
        var roomX = origRoomX;
        ImGui.SliderInt("X offset", ref roomX, 0, roomW);
        if (roomX != origRoomX)
        {
            viewSettings.RoomOffsetX = roomX;
        }

        var origRoomY = viewSettings.RoomOffsetY;
        if (origRoomY > roomH)
        {
            origRoomY = roomH;
        }
        var roomY = origRoomY;
        ImGui.SliderInt("Y offset", ref roomY, 0, roomH);
        if (roomY != origRoomY)
        {
            viewSettings.RoomOffsetY = roomY;
        }
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

void RenderProgramView(ActiveData viewModel, ProgramViewSettings viewSettings)
{
    var viewW = 300.0f;
    var viewH = 600.0f;
    ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Program View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    var idx = 0;
    foreach (var instruction in viewSettings.InstructionsView)
    {
        var evaluatedAlready = idx <= viewSettings.EvaluateUntil;
        if (evaluatedAlready)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.6f, 0.8f, 1.0f));
        }
        if (ImGui.Button(instruction))
        {
            viewSettings.SetEvaluateUntil(idx);
        }
        if (evaluatedAlready)
        {
            ImGui.PopStyleColor();
        }

        idx++;
    }
    
    ImGui.End();
}

void RenderProgramReferenceView(ProgramViewSettings viewSettings)
{
    var viewW = 300.0f;
    var viewH = 600.0f;
    ImGui.SetNextWindowPos(new Vector2(300.0f, 200.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Program References", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    ImGui.LabelText("Rooms:", "");
    foreach (var room in viewSettings.ReferencedRoomsView)
    {
        if (ImGui.Button($"{room}"))
        {
            //TODO: navigate to room view
        }
        ImGui.SameLine();
    }
    
    ImGui.End();
}