using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class GameViewWindow : BaseWindow
{
    private const bool ShowDebug = true;
    private const bool ShowDebugAreaRects = ShowDebug && false;
    private const bool ShowDebugBackgroundRects = ShowDebug && false;
    private const bool ShowDebugPointRects = ShowDebug && false;
    private const bool ShowDebugWalkRects = ShowDebug && false;
    private const bool ShowDebugTalkRects = ShowDebug && false;
    private const bool ShowDebugKeyCharRects = ShowDebug && false;
    private const bool ShowDebugHitboxRects = ShowDebug && false;
    private const bool ShowDebugInventoryRects = ShowDebug && false;
    
    private readonly DatabaseModel _model;
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;
    private readonly RoomImageRenderer _roomImageRenderer;
    private readonly SpriteSheetRenderer _spriteSheetRenderer;
    private readonly IconImageRenderer _iconImageRenderer;
    private readonly LogData _log;
    private readonly GameViewState _viewState;

    public GameViewWindow(DatabaseModel model, RenderWindow render, WindowSettings windowSettings, ActiveProgramState activeProgramState, RoomImageRenderer roomImageRenderer, SpriteSheetRenderer spriteSheetRenderer, LogData log, GameViewState viewState, IconImageRenderer iconImageRenderer)
    {
        _model = model;
        _render = render;
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
        _roomImageRenderer = roomImageRenderer;
        _spriteSheetRenderer = spriteSheetRenderer;
        _log = log;
        _viewState = viewState;
        _iconImageRenderer = iconImageRenderer;
    }

    public override void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        var frameHeight = ImGui.GetFrameHeight();
        var offset = new Vector2(1.0f, 1.0f + frameHeight);
        ImGui.SetNextWindowPos(new Vector2(750.0f, 200.0f), ImGuiCond.Once);
        ImGui.SetNextWindowSize(new Vector2(Constants.GameScreenWidth + 2, Constants.GameScreenHeight + 2 + frameHeight));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.Begin("Game View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var screenMousePos = ImGui.GetMousePos() - ImGui.GetWindowPos() - offset;
        _viewState.ScreenMousePos = screenMousePos;
        var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos() - offset + new Vector2(offsetX, offsetY);
        _viewState.MousePos = mousePos;
        var leftCount = ImGui.GetMouseClickedCount(ImGuiMouseButton.Left);
        var rightCount = ImGui.GetMouseClickedCount(ImGuiMouseButton.Right);
        if (leftCount != 0)
        {
            //TODO: is double-click necessary?
            _viewState.LeftClicked = true;
        }
        if (rightCount != 0)
        {
            _viewState.RightClicked = true;
        }
        
        RenderRoom(offset);
        RenderActiveAreas(offset); //after background
        RenderKeyChars(offset);
        
        RenderBackgroundActiveAreas(offset); //after key chars and areas

        RenderPointsDebug(offset);
        RenderHitboxesDebug(offset);

        RenderHitboxes(offset);
        RenderActiveTalkEntries(offset); //just before inventory

        RenderInventory(offset); //last

        ImGui.End();
        ImGui.PopStyleVar();

        if (_viewState.LeftClicked)
        {
            _viewState.LeftClicked = false;
            //TODO: check that the click was on the right window
            _activeProgramState.LeftClicked((int)screenMousePos.X, (int)screenMousePos.Y, (int)mousePos.X, (int)mousePos.Y);
        }

        _viewState.RightClicked = false;
    }

    private void RenderActiveAreas(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        
        ushort aIdx = 0;
        foreach (var (areaId, areaState) in _activeProgramState.CurrentState.ActiveRoomAreas)
        {
            foreach (var area in program.Areas.Where(a => a.Id == areaId))
            {
                RenderArea(offset, area, $"Area {aIdx} {areaState:G} ({area.Id})", ShowDebugAreaRects);

                aIdx++;
            }
        }
    }

    private void RenderBackgroundActiveAreas(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        
        ushort idx = 0;
        foreach (var background in program.Backgrounds)
        {
            var ox = background.Rect.X;
            var oy = background.Rect.Y;
            if (_activeProgramState.CurrentState.BackgroundOffsets.ContainsKey(idx))
            {
                (ox, oy) = _activeProgramState.CurrentState.BackgroundOffsets[idx];
            }
            if (oy != 20000)
            {
                if (background.IsScaled)
                {
                    var dx = background.Offset - Constants.GameScreenWidth / 2 - _activeProgramState.GetFlag(Flags.Known.RoomScrollX);
                    dx *= background.ScaleMul;
                    dx /= background.ScaleDiv;
                    ox += dx;
                }
                var x = ox - offsetX;
                var y = oy - offsetY;
                
                RenderRoomImageSubsection(offset, x, y, background.SrcX, background.SrcY, background.Rect.W, background.Rect.H, true);
                
                if (ShowDebugBackgroundRects)
                {
                    RenderRectangle(offset, background.Rect.W, background.Rect.H, background.Rect.X, background.Rect.Y,
                        $"BG Area {idx}", 1,
                        255, 0, 255, 50, 255, 255, 255, 150);
                }
            }

            idx++;
        }
    }

    private void RenderActiveTalkEntries(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        
        ushort tIdx = 0;
        foreach (var talkEntry in _activeProgramState.TalkEntries)
        {
            var keyChar = _activeProgramState.GetKeyChar(talkEntry.TalkingKeyChar);
            var (charX, charY) = (keyChar.PositionX, keyChar.PositionY - (keyChar.PositionZ / 2 + 16));
            
            var entryTextSize = ImGui.CalcTextSize(talkEntry.Text);
            var tx = entryTextSize.X;
            if (tx > 200)
            {
                tx = 200;
            }
            var ty = entryTextSize.Y;
            
            var x = charX - offsetX - tx/2;
            if (x < 0)
            {
                x = 0;
            }
            if (x + tx >= Constants.GameScreenWidth)
            {
                x = Constants.GameScreenWidth - tx - 1;
            }
            var y = charY - offsetY - ty;
            if (y < 0)
            {
                y = 1;
            } else if (y > Constants.RoomHeight)
            {
                y = Constants.RoomHeight - 16;
            }

            var col = keyChar.TextColour;
            var col1 = (int)(col & 0xFF);
            var paletteCol1 = _activeProgramState.GetLoadedColour(col1);
            
            ImGui.SetCursorPos(offset + new Vector2(x, y));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(paletteCol1.R/255.0f, paletteCol1.G/255.0f, paletteCol1.B/255.0f, 1.0f));
            ImGui.PushTextWrapPos(offset.X + x + tx);
            ImGui.Text(talkEntry.Text);
            ImGui.PopTextWrapPos();
            ImGui.PopStyleColor();

            if (ShowDebugTalkRects)
            {
                RenderRectangle(offset, (int)tx, (int)ty * 2, (int)x, (int)y,
                    $"Text {tIdx}", 1,
                    255, 0, 0, 50, 255, 255, 255, 150);
            }

            tIdx++;
        }
    }

    private void RenderInventory(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        if (_activeProgramState.GetFlag(Flags.Known.DisableInventoryDraw) != 0)
        {
            return;
        }

        var keyCharId = _activeProgramState.CurrentKeyChar;
        if (keyCharId > 1)
        {
            keyCharId = 1; //prevents showing the valid last inventory list, but game engine code does this
        }

        var inventoryList = _activeProgramState.InventoryLists[keyCharId];
        var palette = _activeProgramState.GetLoadedPalette();

        var ox = 0;
        var oy = Game.RoomHeight;
        //draw background
        DrawEntireSpriteSheet(offset, ox, oy, 12 + keyCharId);

        //draw items
        var x = ox + 245;
        var y = 353;
        for (var i = 0; i < 6; i++)
        {
            var item = inventoryList.Items[inventoryList.DisplayOffset + i];
            if (item == -1)
            {
                break;
            }

            if (item != 0)
            {
                var iconImage = _model.Icons[item].Value;
                var (iconImageId, bytes) = _iconImageRenderer.RenderIconImage(item, iconImage, palette);
                
                var iconTexture = _render.RenderImage(RenderWindow.RenderType.Icon, iconImageId, iconImage.Width, iconImage.Height, bytes);

                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(iconTexture, new Vector2(iconImage.Width, iconImage.Height));

                if (ShowDebugInventoryRects)
                {
                    RenderRectangle(offset, 32, 32, x, y, "Icon {item}", 1,
                        255, 255, 0, 50, 255, 255, 255, 150);
                }
            }

            x += 58;
        }
        
        //draw money
        var keyChar = _activeProgramState.GetKeyChar(0); //engine always looks up from key char 0
        var bgCol = _activeProgramState.GetLoadedColour(210); //from game code
        var textCol = _activeProgramState.GetLoadedColour(217); //from game code
        //blank out background
        RenderRectangle(offset, 40, 16, 74, 354, "", 0, 
            bgCol.R, bgCol.G, bgCol.B, 255, 0, 0, 0, 0
            );
        
        //draw text
        ImGui.SetCursorPos(offset + new Vector2(94, 355));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(textCol.R/255.0f, textCol.G/255.0f, textCol.B/255.0f, 1.0f));
        ImGui.Text($"{keyChar.Money:D}");
        ImGui.PopStyleColor();

        if (_activeProgramState.GlobalMoney != 0)
        {
            //draw icon
            var iconImage = _model.Icons[1].Value;
            var (iconImageId, bytes) = _iconImageRenderer.RenderIconImage(1, iconImage, palette);
                
            var iconTexture = _render.RenderImage(RenderWindow.RenderType.Icon, iconImageId, iconImage.Width, iconImage.Height, bytes);

            ImGui.SetCursorPos(offset + new Vector2(141, 348));
            ImGui.Image(iconTexture, new Vector2(iconImage.Width, iconImage.Height));
            
            //draw text
            ImGui.SetCursorPos(offset + new Vector2(170, 378));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(textCol.R/255.0f, textCol.G/255.0f, textCol.B/255.0f, 1.0f));
            ImGui.Text($"{_activeProgramState.GlobalMoney:D}");
            ImGui.PopStyleColor();
        }
    }

    private void RenderPointsDebug(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();

        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        
        ushort pIdx = 0;
        foreach (var point in program.Points)
        {
            var ox = point.X;
            var oy = point.Y;
            var oz = point.Z;

            var zFactor = Game.GetZFactor(oz);
            
            var x = ox - offsetX;
            var y = oy - offsetY;
            var pointSize = 40;
            var pointWidth = (int)(pointSize * zFactor);
            if (pointWidth < 3)
            {
                pointWidth = 3;
            }

            if (ShowDebugPointRects)
            {
                RenderRectangle(offset, pointWidth, pointWidth, x - pointWidth/2, y - pointWidth/2, 
                    $"P{pIdx}", 1,
                    255, 255, 255, 50, 255, 255, 255, 150);
            }
            
            pIdx++;
        }
    }
    
    private void RenderHitboxesDebug(Vector2 offset)
    {
        return;
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        
        ushort pIdx = 0;
        foreach (var hitbox in program.Hitboxes)
        {
            if (ShowDebugHitboxRects)
            {
                if (!hitbox.IsDrawable)
                {
                    continue;
                }
                RenderHitbox(offset, hitbox, pIdx.ToString());
            }
            
            pIdx++;
        }
    }

    private void RenderRoom(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var (w, h) = GetGameScreenSize();

        RenderRoomImageSubsection(offset, 0, 0, offsetX, offsetY, w, h, false);
    }

    private void RenderKeyChars(Vector2 offset)
    {
        var colIdx = 0;
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        
        foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars
                     .OrderBy(k => k.Value.PositionZ))
        {
            if (!keyChar.Initialised)
            {
                continue;
            }
            if (keyChar.OffScreen)
            {
                continue;
            }
            
            var x = keyChar.PositionX - offsetX;
            var y = keyChar.PositionY - offsetY;
            var z = keyChar.PositionZ;
            if (z < Game.ZDepthMin)
            {
                z = Game.ZDepthMin;
            }

            if (z > Game.ZDepthMax)
            {
                z = Game.ZDepthMax;
            }

            RenderKeyChar(offset, keyCharId, x, y, z);

            //render active walk areas
            if (keyChar.LastWalk != null)
            {
                if (keyChar.LastWalk.Value >= program.Walks.Count)
                {
                    continue;
                }

                var walk = program.Walks[keyChar.LastWalk.Value];
                if (walk.Area1 != 0)
                {
                    foreach (var area in program.Areas.Where(a => a.Id == walk.Area1))
                    {
                        RenderArea(offset, area, $"Walk {keyCharId} 1 ({area.Id})", ShowDebugWalkRects);
                    }
                }

                if (walk.Area2 != 0)
                {
                    foreach (var area in program.Areas.Where(a => a.Id == walk.Area2))
                    {
                        RenderArea(offset, area, $"Walk {keyCharId} 1 ({area.Id})", ShowDebugWalkRects);
                    }
                }
            }
        }
    }

    private void RenderKeyChar(Vector2 offset, int keyCharId, int x, int y, int z)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        var keyChar = _activeProgramState.KeyChars[keyCharId];

        if (keyChar.SequenceIndex == null || keyChar.SpriteIndex == null || keyChar.Character == null)
        {
            return;
        }

        if (keyChar.SpriteIndex.Value >= _activeProgramState.LoadedSprites.Length ||
            keyChar.SequenceIndex.Value >= _activeProgramState.LoadedSprites.Length ||
            _activeProgramState.LoadedSprites[keyChar.SpriteIndex.Value].SpriteNum == null ||
            _activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum == null
           )
        {
            return;
        }

        var spriteNum = _activeProgramState.LoadedSprites[keyChar.SpriteIndex.Value].SpriteNum;
        var sequenceNum = _activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum;
        if (spriteNum == null || sequenceNum == null)
        {
            return;
        }

        if (!_model.Sequences.ContainsKey(sequenceNum.Value) ||
            !_model.Sprites.ContainsKey(spriteNum.Value))
        {
            return;
        }

        var character = keyChar.Character.Value;
        if (!_model.Sequences[sequenceNum.Value].Characters.ContainsKey(character))
        {
            return;
        }

        var zFactor = Game.GetZFactor(z);

        if (keyChar.LastWalk != null && keyChar.LastWalk.Value < program.Walks.Count)
        {
            var walk = program.Walks[keyChar.LastWalk.Value];
            var clipRect = program.Rects[walk.ClipRect];

            var windowOffset = ImGui.GetWindowPos() + offset;
            ImGui.PushClipRect(windowOffset + new Vector2(clipRect.X - offsetX, clipRect.Y - offsetY),
                windowOffset + new Vector2(clipRect.X - offsetX + clipRect.W, clipRect.Y - offsetY + clipRect.H),
                true);
        }

        var ch = _model.Sequences[sequenceNum.Value].Characters[character];
        var animId = keyChar.CurrentAnim;
        if (!ch.Animations.ContainsKey(animId))
        {
            throw new Exception($"Could not find current animation: {animId}");
        }

        var anim = ch.Animations[animId];
        var dirId = keyChar.CurrentDirection;
        if (!anim.Directions.ContainsKey(dirId))
        {
            throw new Exception($"Could not find current direction: {dirId}");
        }

        var dir = anim.Directions[dirId];
        var frameId = keyChar.CurrentAnimCounter;
        if (frameId >= dir.Frames.Count || frameId < 0)
        {
            throw new Exception($"Could not find current frame: {frameId}");
        }

        DrawSpriteImage(offset, x, y, spriteNum.Value, sequenceNum.Value, character, animId, dirId, frameId, zFactor, out var width, out var height);

        var ox = (int)(x - width / 2.0f);
        var oy = y - height;
        width = (int)Math.Ceiling(width * zFactor);
        height = (int)Math.Ceiling(height * zFactor);
        if (ShowDebugKeyCharRects)
        {
            RenderRectangle(offset, width, height, ox, oy, $"{keyCharId}", 1,
                0, 0, 255, 50, 255, 255, 255, 150);
        }
        
        if (keyChar.LastWalk != null && keyChar.LastWalk.Value < program.Walks.Count)
        {
            ImGui.PopClipRect();
        }

        _viewState.KeyCharRenderedRects[keyCharId] = (ox, oy, width, height);
    }
    
    private void RenderHitboxes(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var mousePos = _viewState.MousePos;
        var screenMousePos = _viewState.ScreenMousePos;
        
        ushort pIdx = 0;
        foreach (var hitbox in program.Hitboxes)
        {
            if (!hitbox.IsDrawable)
            {
                continue;
            }
            if (hitbox.IsInventory)
            {
                //TODO: check if the inventory item is populated
                continue;
            }

            var x = hitbox.Rect1.X;
            var y = hitbox.Rect1.Y;
            var w = hitbox.Rect1.W;
            var h = hitbox.Rect1.H;
            
            var s = "";
            if (hitbox.IsCharacter)
            {
                var keyChar = _activeProgramState.GetKeyChar(hitbox.KeyChar);
                if (keyChar.Initialised)
                {
                    //TODO: if keychar had hitbox set change text
                    (x, y, w, h) = _viewState.KeyCharRenderedRects[hitbox.KeyChar];
                    x += offsetX;
                    y += offsetY;
                }
            }
            

            if (w != 0 && h != 0)
            {
                if (ShowDebugHitboxRects)
                {
                    RenderHitbox(offset, hitbox, pIdx.ToString(), s, x, y, w, h);
                }
                
                if (mousePos.X >= x && mousePos.X <= x + w &&
                    mousePos.Y >= y && mousePos.Y <= y + h)
                {
                    if (_viewState.LeftClicked)
                    {
                        _viewState.LeftClicked = false;
                        _activeProgramState.LeftClicked((int)screenMousePos.X, (int)screenMousePos.Y, (int)mousePos.X, (int)mousePos.Y, hitbox.Item);
                    }
                    RenderHitbox(offset, hitbox, pIdx.ToString(), s, x, y, w, h);
                }
            }
            
            pIdx++;
        }
    }

    #region Generic methods    
    private (int, int) GetLoadedRoomOffset()
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return (0, 0);
        }
        var offsetX = _activeProgramState.GetFlag(Flags.Known.RoomScrollX);
        var offsetY = _activeProgramState.GetFlag(Flags.Known.RoomScrollY);
        if (_activeProgramState.GetFlag(Flags.Known.DisableRoomScroll) != 0)
        {
            offsetX = 0;
            offsetY = 0;
        }

        return (offsetX, offsetY);
    }
    
    private (int, int) GetGameScreenSize()
    {
        var w = Constants.GameScreenWidth;
        var h = Constants.RoomHeight;
        if (_activeProgramState.GetFlag(Flags.Known.DisableInventoryDraw) != 0)
        {
            h = Constants.GameScreenHeight;
        }

        return (w, h);
    }

    private void RenderHitbox(Vector2 offset, ProgramDataModel.Hitbox hitbox, string id, string str = "", int? x = null, int? y = null, int? w = null, int? h = null)
    {
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var s = str;
        if (string.IsNullOrEmpty(str))
        {
            s = _activeProgramState.GetString(hitbox.String);
        }

        var clickAso = program.ActionScriptOffsets.Count(aso =>
            aso.Action == -49 && aso.Object1 == hitbox.Item && aso.Object2 == 0) > 0;

        var type = "";
        if (hitbox.IsInventory)
        {
            type += "Inventory ";
        }
        if (hitbox.IsDisabled)
        {
            type += "OneOff ";
        }
        if (hitbox.IsCharacter)
        {
            type += "Character ";
        }

        if (clickAso)
        {
            type += "\nClick";
        }
        var msg = $"HB {id}\n{type}\n{s}";

        x ??= hitbox.Rect1.X;
        y ??= hitbox.Rect1.Y;
        w ??= hitbox.Rect1.W;
        h ??= hitbox.Rect1.H;

        RenderRectangle(offset, w.Value, h.Value, 
            x.Value - offsetX, y.Value - offsetY,
            "Hover\n" + msg, 1,
            255, 0, 255, 50, 255, 255, 255, 150);
        
        if (hitbox.Rect2.W != 0 && hitbox.Rect2.H != 0 &&
            (x != hitbox.Rect2.X || 
             y != hitbox.Rect2.Y ||
             w != hitbox.Rect2.W ||
             h != hitbox.Rect2.H
            )
           )
        {
            RenderRectangle(offset, hitbox.Rect2.W, hitbox.Rect2.H, hitbox.Rect2.X - offsetX,
                hitbox.Rect2.Y - offsetY,
                "Redraw\n" + msg, 1,
                255, 0, 255, 50, 255, 255, 255, 150);
        }
    }
    
    private void RenderArea(Vector2 offset, ProgramDataModel.Area area, string message, bool renderRect)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var (ox, oy) = (area.Rect.X - offsetX, area.Rect.Y - offsetY);

        RenderRoomImageSubsection(offset, ox, oy, area.SrcX, area.SrcY, area.Rect.W, area.Rect.H, true);

        if (renderRect)
        {
            RenderRectangle(offset, area.Rect.W, area.Rect.H, ox, oy, message, 1,
                255, 255, 0, 50, 255, 255, 255, 150);
        }
    }

    private void DrawEntireSpriteSheet(Vector2 offset, int x, int y, int spriteNum)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
        var sprite = _model.Sprites[spriteNum].Value;
        var palette = _activeProgramState.GetLoadedPalette();

        var (viewId, bytes) = _spriteSheetRenderer.RenderSpriteSheet(spriteNum, sprite, activeRoom, palette);
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, viewId, sprite.Width, sprite.Height, bytes);
        var spritePosition = offset + new Vector2(x, y);
        
        ImGui.SetCursorPos(spritePosition);
        ImGui.Image(spriteTexture, new Vector2(sprite.Width, sprite.Height));
    }
    
    private void DrawSpriteImage(Vector2 offset, int x, int y, 
        int spriteNum, int sequenceNum, int charId, 
        int animId, int dirId, int frameId,
        float zFactor, 
        out int width, out int height)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
        var sprite = _model.Sprites[spriteNum].Value;
        var palette = _activeProgramState.GetLoadedPalette();

        var (viewId, bytes) = _spriteSheetRenderer.RenderSpriteSheet(spriteNum, sprite, activeRoom, palette);
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, viewId, sprite.Width, sprite.Height, bytes);

        var sequence = _model.Sequences[sequenceNum];
        var ch = sequence.Characters[charId];
        var anim = ch.Animations[animId];
        var dir = anim.Directions[dirId];
        var frame = dir.Frames[frameId];

        var spritePosition = offset + new Vector2(x, y);
        var lowX = 0;
        var lowY = 0;
        var highX = 0;
        var highY = 0;
        foreach (var part in frame.Parts)
        {
            var (frameIndex, destX, destY, hFlip, vFlip) = (part.FrameIndex, part.DestX, part.DestY,
                part.HFlipped, part.VFlipped);
            if (destX < lowX)
            {
                lowX = destX;
            }

            if (destY < lowY)
            {
                lowY = destY;
            }

            if (destX + sprite.SpriteWidth > highX)
            {
                highX = destX + sprite.SpriteWidth;
            }

            if (destY + sprite.SpriteHeight > highY)
            {
                highY = destY + sprite.SpriteHeight;
            }

            var tileWidthRatio = (float)sprite.SpriteWidth / sprite.Width;
            var tileHeightRatio = (float)sprite.SpriteHeight / sprite.Height;
            var tilesPerRow = (int)Math.Floor((float)sprite.Width / sprite.SpriteWidth);
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

            var dirX = 0;
            if (dirId == 3)
            {
                dirX = -sprite.SpriteWidth;
            }

            ImGui.SetCursorPos(spritePosition + new Vector2(dirX + destX, destY) * zFactor);
            ImGui.Image(spriteTexture, new Vector2(sprite.SpriteWidth, sprite.SpriteHeight) * zFactor, spriteUv1,
                spriteUv2);
        }

        width = highX - lowX;
        height = highY - lowY;
    }
    
    private void RenderRoomImageSubsection(Vector2 offset, int x, int y, int srcX, int srcY, int w, int h, bool transparency)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;

        var roomImageId = _model.Rooms[activeRoom].RoomImageNum;
        var roomImage = _model.RoomImages[roomImageId].Value;
        var palette = _activeProgramState.GetLoadedPalette();

        var activeRoomSprites = _activeProgramState.CurrentState.ActiveRoomSprites.Select(roomSprite =>
        {
            var spriteNum = _activeProgramState.LoadedSprites[roomSprite.Item1].SpriteNum;
            if (spriteNum == null)
            {
                throw new Exception("Sprite not loaded");
            }

            var sprite = _model.Sprites[spriteNum.Value].Value;
            return (roomSprite.Item1, sprite, roomSprite.Item2, roomSprite.Item3);
        }).ToList();
        
        var (areaViewId, areaBytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, palette,
            activeRoomSprites, srcX, srcY, w, h, transparency);
        
        var roomAreaTexture = _render.RenderImage(RenderWindow.RenderType.Room, areaViewId, w, h, areaBytes);

        ImGui.SetCursorPos(offset + new Vector2(x, y));
        ImGui.Image(roomAreaTexture, new Vector2(w, h));
    }

    private void RenderRectangle(Vector2 offset, int w, int h, int x, int y, string message, 
        int borderWidth,
        byte fillR, byte fillG, byte fillB, byte fillA, 
        byte borderR, byte borderG, byte borderB, byte borderA)
    {
        var roomAreaRectTexture = _render.RenderRectangle(borderWidth, w, h,
            (fillR, fillG, fillB, fillA), (borderR, borderG, borderB, borderA));
        ImGui.SetCursorPos(offset + new Vector2(x, y));
        ImGui.Image(roomAreaRectTexture, new Vector2(w, h));

        var textSize = ImGui.CalcTextSize(message);
        ImGui.SetCursorPos(offset + new Vector2(x + w - textSize.X - 2, y + h - textSize.Y - 2));
        ImGui.Text(message);
    }
#endregion
}