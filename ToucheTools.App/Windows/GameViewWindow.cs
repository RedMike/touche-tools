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
    private readonly List<(byte, byte, byte)> _colours = new List<(byte, byte, byte)>()
    {
        (150, 70, 20),
        (70, 150, 20),
        (20, 70, 150),
        (150, 20, 150),
        (20, 20, 70),
        (20, 20, 20),
        (150, 250, 250),
        (250, 250, 50),
        (255, 0, 0),
        (0, 255, 0),
        (0, 0, 255),
        (50, 255, 150),
        (255, 0, 255),
        (200, 200, 200)
    };
    private const bool ShowDebug = true;
    private const bool ShowDebugAreaRects = ShowDebug && true;
    private const bool ShowDebugBackgroundRects = ShowDebug && true;
    private const bool ShowDebugPointRects = ShowDebug && true;
    private const bool ShowDebugWalkRects = ShowDebug && true;
    private const bool ShowDebugTalkRects = ShowDebug && true;
    private const bool ShowDebugKeyCharRects = ShowDebug && true;
    
    private readonly DatabaseModel _model;
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;
    private readonly RoomImageRenderer _roomImageRenderer;
    private readonly SpriteSheetRenderer _spriteSheetRenderer;
    private readonly LogData _log;

    public GameViewWindow(DatabaseModel model, RenderWindow render, WindowSettings windowSettings, ActiveProgramState activeProgramState, RoomImageRenderer roomImageRenderer, SpriteSheetRenderer spriteSheetRenderer, LogData log)
    {
        _model = model;
        _render = render;
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
        _roomImageRenderer = roomImageRenderer;
        _spriteSheetRenderer = spriteSheetRenderer;
        _log = log;
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

        RenderRoom(offset);
        RenderActiveAreas(offset); //after background areas
        RenderKeyChars(offset);
        RenderActiveWalkAreas(offset); //after key chars
        
        RenderBackgroundActiveAreas(offset); //after key chars and areas

        RenderPointsDebug(offset);

        RenderActiveTalkEntries(offset); //last

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private void RenderActiveAreas(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        
        ushort aIdx = 0;
        foreach (var areaId in _activeProgramState.CurrentState.ActiveRoomAreas)
        {
            foreach (var area in program.Areas.Where(a => a.Id == areaId))
            {
                RenderArea(offset, area, aIdx);

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
    
    private void RenderActiveWalkAreas(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();

        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
        
        foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars.Where(p =>
                     p.Value.Initialised && !p.Value.OffScreen && p.Value.LastWalk != null))
        {
            if (keyChar.LastWalk == null)
            {
                throw new Exception("Missing last walk somehow");
            }
            var walk = program.Walks[keyChar.LastWalk.Value];
            if (walk.Area1 != 0)
            {
                foreach (var area in program.Areas.Where(a => a.Id == walk.Area1))
                {
                    var ox = area.Rect.X;
                    var oy = area.Rect.Y;

                    var x = ox - offsetX;
                    var y = oy - offsetY;

                    RenderRoomImageSubsection(offset, x, y, area.SrcX, area.SrcY, area.Rect.W, area.Rect.H, true);

                    if (ShowDebugWalkRects)
                    {
                        RenderRectangle(offset, area.Rect.W, area.Rect.H, x, y, 
                            $"Walk {keyCharId} 1 ({area.Id})", 1, 
                            255, 255, 0, 50, 255, 255, 255, 150);
                    }
                }
            }
            if (walk.Area2 != 0)
            {
                foreach (var area in program.Areas.Where(a => a.Id == walk.Area2))
                {
                    var ox = area.Rect.X;
                    var oy = area.Rect.Y;

                    var x = ox - offsetX;
                    var y = oy - offsetY;

                    RenderRoomImageSubsection(offset, x, y, area.SrcX, area.SrcY, area.Rect.W, area.Rect.H, true);

                    if (ShowDebugWalkRects)
                    {
                        RenderRectangle(offset, area.Rect.W, area.Rect.H, x, y, 
                            $"Walk {keyCharId} 2 ({area.Id})", 1, 
                            255, 255, 0, 50, 255, 255, 255, 150);
                    }
                }
            }
        }
    }

    private void RenderActiveTalkEntries(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
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
            var palette = _model.Palettes[activeRoom]; //TODO: palette shifting
            var paletteCol1 = palette.Colors[col1];
            
            ImGui.SetCursorPos(offset + new Vector2(x, y));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(paletteCol1.R/255.0f, paletteCol1.G/255.0f, paletteCol1.B/255.0f, 1.0f));
            ImGui.PushTextWrapPos(tx);
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

    private void RenderArea(Vector2 offset, ProgramDataModel.Area area, ushort aIdx)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        var (ox, oy) = (area.Rect.X - offsetX, area.Rect.Y - offsetY);

        RenderRoomImageSubsection(offset, ox, oy, area.SrcX, area.SrcY, area.Rect.W, area.Rect.H, true);

        if (ShowDebugAreaRects)
        {
            RenderRectangle(offset, area.Rect.W, area.Rect.H, ox, oy, $"Area {aIdx} ({area.Id})", 1,
                255, 255, 0, 50, 255, 255, 255, 150);
        }
    }
    
    private void RenderKeyChars(Vector2 offset)
    {
        var colIdx = 0;
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
        var (offsetX, offsetY) = GetLoadedRoomOffset();
        
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];
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

            if (keyChar.LastWalk != null && keyChar.LastWalk.Value < program.Walks.Count)
            {
                var walk = program.Walks[keyChar.LastWalk.Value];
                var clipRect = program.Rects[walk.ClipRect];
                
                var windowOffset = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
                ImGui.PushClipRect(windowOffset + new Vector2(clipRect.X - offsetX,  clipRect.Y - offsetY),
                    windowOffset + new Vector2(clipRect.X - offsetX + clipRect.W, clipRect.Y - offsetY + clipRect.H), 
                    true);
            }
            var (colR, colG, colB) = _colours[colIdx];
            colIdx++;
            colIdx = colIdx % _colours.Count;
            
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

            var zFactor = Game.GetZFactor(z);
            
            var width = 70;
            var height = 100;

            if (keyChar.SpriteIndex != null && keyChar.SequenceIndex != null && keyChar.Character != null && 
                _activeProgramState.CurrentState.LoadedRoom != null &&
                _activeProgramState.LoadedSprites[keyChar.SpriteIndex.Value].SpriteNum != null &&
                _activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum != null &&
                _model.Sequences.ContainsKey(_activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum.Value) &&
                _model.Sequences[_activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum.Value].Characters.ContainsKey(keyChar.Character.Value)
               )
            {
                var spriteNum = _activeProgramState.LoadedSprites[keyChar.SpriteIndex.Value].SpriteNum.Value;
                var sprite = _model.Sprites[spriteNum].Value;
                var palette = _model.Palettes[activeRoom];
                
                var (viewId, bytes) = _spriteSheetRenderer.RenderSpriteSheet(spriteNum, sprite, activeRoom, palette);
                var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, viewId, sprite.Width, sprite.Height, bytes);

                var seqNum = _activeProgramState.LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum.Value;
                var sequence = _model.Sequences[seqNum];
                var ch = sequence.Characters[keyChar.Character.Value];
                
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
                    ImGui.Image(spriteTexture, new Vector2(sprite.SpriteWidth, sprite.SpriteHeight) * zFactor, spriteUv1, spriteUv2);   
                }
                width = highX - lowX;
                height = highY - lowY;
            }


            if (ShowDebugKeyCharRects)
            {
                width = (int)Math.Ceiling(width * zFactor);
                height = (int)Math.Ceiling(height * zFactor);
                var ox = (int)(x - width / 2.0f);
                var oy = y - height;
                RenderRectangle(offset, width, height, ox, oy, $"{keyCharId}", 1,
                    0, 0, 255, 50, 255, 255, 255, 150);
            }

            if (keyChar.LastWalk != null && keyChar.LastWalk.Value < program.Walks.Count)
            {
                ImGui.PopClipRect();
            }
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
    
    private void RenderRoomImageSubsection(Vector2 offset, int x, int y, int srcX, int srcY, int w, int h, bool transparency)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            throw new Exception("Room not loaded");
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;

        var roomImageId = _model.Rooms[activeRoom].RoomImageNum;
        var roomImage = _model.RoomImages[roomImageId].Value;
        var palette = _model.Palettes[activeRoom]; //TODO: palette shifting
        
        var (areaViewId, areaBytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette,
            srcX, srcY, w, h, transparency);

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