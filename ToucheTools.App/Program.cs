﻿using System.Numerics;
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
var spriteViewSettings = new SpriteViewSettings();
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
    #region Window Settings
    RenderWindowSettings(windowSettings);
    #endregion
    #region Active Objects
    RenderActiveObjects(activeData);
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

void RenderWindowSettings(WindowSettings settings)
{
    bool origRoomViewShown = settings.RoomViewOpen;
    bool roomViewShown = origRoomViewShown;

    bool origSpriteViewShown = settings.SpriteViewOpen;
    bool spriteViewShown = origSpriteViewShown;

    bool origProgramViewShown = settings.ProgramViewOpen;
    bool programViewShown = origProgramViewShown;
    
    ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(150.0f, 150.0f));
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
    
    ImGui.SetNextWindowPos(new Vector2(300.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(200.0f, 150.0f));
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
    ImGui.SetNextWindowPos(new Vector2(0.0f, 150.0f));
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
    ImGui.SetNextWindowPos(new Vector2(0.0f, 150.0f));
    ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
    ImGui.Begin("Sprite View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

    var cursorPos = new Vector2(viewW / 2.0f, viewH / 2.0f);
    
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
    
    var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = viewModel.SpriteView;

    var spriteTexture = window.RenderImage(spriteViewId, spriteWidth, spriteHeight, spriteBytes);
    var spriteUv1 = new Vector2(0.0f, 0.0f);
    var spriteUv2 = new Vector2((float)spriteTileWidth / spriteWidth, (float)spriteTileHeight / spriteHeight);
    ImGui.SetCursorPos(cursorPos);
    ImGui.Image(spriteTexture, new Vector2(spriteTileWidth, spriteTileHeight), spriteUv1, spriteUv2);
    
    ImGui.End();
}

void RenderSpriteViewSettings(ActiveData activeData, SpriteViewSettings viewSettings)
{
    var origShowRoom = viewSettings.ShowRoom;
    var showRoom = origShowRoom;
    
    ImGui.SetNextWindowPos(new Vector2(500.0f, 0.0f));
    ImGui.SetNextWindowSize(new Vector2(width-500.0f, 150.0f));
    ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

    ImGui.Checkbox("Room background", ref showRoom);
    if (showRoom != origShowRoom)
    {
        viewSettings.ShowRoom = showRoom;
    }

    if (showRoom)
    {
        var (_, roomW, roomH, _) = activeData.RoomView;
        
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