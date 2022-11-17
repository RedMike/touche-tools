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
        RenderKeyChars(offset);

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private void RenderRoom(Vector2 offset)
    {
        if (_activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }
        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
        var offsetX = _activeProgramState.GetFlag(Flags.Known.RoomScrollX);
        var offsetY = _activeProgramState.GetFlag(Flags.Known.RoomScrollY);
        if (_activeProgramState.GetFlag(Flags.Known.DisableRoomScroll) != 0)
        {
            offsetX = 0;
            offsetY = 0;
        }
        var w = Constants.GameScreenWidth;
        var h = Constants.RoomHeight;
        if (_activeProgramState.GetFlag(Flags.Known.DisableInventoryDraw) != 0)
        {
            h = Constants.GameScreenHeight;
        }

        var roomImageId = _model.Rooms[activeRoom].RoomImageNum;
        var roomImage = _model.RoomImages[roomImageId].Value;
        var palette = _model.Palettes[activeRoom]; //TODO: palette shifting
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];

        var (viewId, bytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, offsetX, offsetY, w, h, false);

        var roomFullTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, w, h, bytes);

        ImGui.SetCursorPos(offset + new Vector2(0.0f, 0.0f));
        ImGui.Image(roomFullTexture, new Vector2(w, h));

        #region Areas
        ushort aIdx = 0;
        foreach (var areaId in _activeProgramState.CurrentState.RoomAreas)
        {
            foreach (var area in program.Areas.Where(a => a.Id == areaId))
            {
                var ox = area.Rect.X;
                var oy = area.Rect.Y;

                var x = ox - offsetX;
                var y = oy - offsetY;
                
                var (areaViewId, areaBytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, area.SrcX, area.SrcY, area.Rect.W, area.Rect.H);

                var roomAreaTexture = _render.RenderImage(RenderWindow.RenderType.Room, areaViewId, area.Rect.W, area.Rect.H, areaBytes);

                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(roomAreaTexture, new Vector2(area.Rect.W, area.Rect.H));


                var roomAreaRectTexture = _render.RenderRectangle(1, area.Rect.W, area.Rect.H,
                    (255, 255, 0, 50), (255, 255, 255, 150));
                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(roomAreaRectTexture, new Vector2(area.Rect.W, area.Rect.H));

                var text = $"Area {aIdx} ({area.Id})";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(offset +
                                   new Vector2(x + area.Rect.W - textSize.X - 2, y + area.Rect.H - textSize.Y - 2));
                ImGui.Text(text);

                aIdx++;
            }
        }
        #endregion
        
        #region Backgrounds
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

                var (areaViewId, areaBytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, background.SrcX, background.SrcY, background.Rect.W, background.Rect.H);

                var roomAreaTexture = _render.RenderImage(RenderWindow.RenderType.Room, areaViewId, background.Rect.W, background.Rect.H, areaBytes);

                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(roomAreaTexture, new Vector2(background.Rect.W, background.Rect.H));

                var roomAreaRectTexture = _render.RenderRectangle(1, background.Rect.W, background.Rect.H,
                    (255, 0, 255, 50), (255, 255, 255, 150));
                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(roomAreaRectTexture, new Vector2(background.Rect.W, background.Rect.H));
                
                var text = $"BG Area {idx}";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(offset + new Vector2(x + background.Rect.W - textSize.X - 2, y + background.Rect.H - textSize.Y - 2));
                ImGui.Text(text);
            }

            idx++;
        }
        #endregion
        
        #region Points
        ushort pIdx = 0;
        foreach (var point in program.Points)
        {
            var ox = point.X;
            var oy = point.Y;
            var oz = point.Z;
            if (oz < Game.ZDepthMin)
            {
                oz = Game.ZDepthMin;
            }
            if (oz > Game.ZDepthMax)
            {
                oz = Game.ZDepthMax;
            }
            var zFactor = 1.0f;
            if (oz < Game.ZDepthEven)
            {
                zFactor = (float)oz / Game.ZDepthEven;
            }

            if (oz > Game.ZDepthEven)
            {
                zFactor = (float)(Game.ZDepthMax - Game.ZDepthEven)/(Game.ZDepthMax - oz);
            }

            var x = ox - offsetX;
            var y = oy - offsetY;
            var pointWidth = 25 * zFactor;
            var pointHeight = 25 * zFactor;
            if (pointWidth < 1.0f)
            {
                pointWidth = 1.0f;
            }
            if (pointHeight < 1.0f)
            {
                pointHeight = 1.0f;
            }
            
            var pointRectTexture = _render.RenderRectangle(1, (int)(pointWidth), (int)(pointHeight),
                (255, 255, 255, 50), (255, 255, 255, 150));
            ImGui.SetCursorPos(offset + new Vector2(x, y)*zFactor);
            ImGui.Image(pointRectTexture, new Vector2(pointWidth, pointHeight));

            var text = $"P{pIdx}";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPos(offset + new Vector2(x * zFactor + pointWidth - textSize.X - 2, y * zFactor + pointHeight - textSize.Y - 2));
            ImGui.Text(text);
            
            pIdx++;
        }
        #endregion
        
        #region Walks
        ushort wIdx = 0;
        foreach (var walk in program.Walks)
        {
            continue;
            var point1 = walk.Point1;
            var point2 = walk.Point2;

            var p1 = program.Points[point1];
            var p2 = program.Points[point2];
            
            var ox = p1.X;
            var oy = p1.Y;
            var oz = p1.Z;
            if (oz < Game.ZDepthMin)
            {
                oz = Game.ZDepthMin;
            }
            if (oz > Game.ZDepthMax)
            {
                oz = Game.ZDepthMax;
            }
            var zFactor = 1.0f;
            if (oz < Game.ZDepthEven)
            {
                zFactor = (float)oz / Game.ZDepthEven;
            }

            if (oz > Game.ZDepthEven)
            {
                zFactor = (float)(Game.ZDepthMax - Game.ZDepthEven)/(Game.ZDepthMax - oz);
            }

            var x = ox - offsetX;
            var y = oy - offsetY;
            var wid = 10;
            var hei = 10;
            
            var pointRectTexture = _render.RenderRectangle(1, (int)(wid*zFactor), (int)(hei*zFactor),
                (255, 255, 255, 50), (255, 255, 255, 150));
            ImGui.SetCursorPos(offset + new Vector2(x, y)*zFactor);
            ImGui.Image(pointRectTexture, new Vector2(wid, hei)*zFactor);

            var text = $"P{wIdx}-1";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPos(offset + new Vector2(x * zFactor + wid * zFactor - textSize.X - 2, y * zFactor + hei * zFactor - textSize.Y - 2));
            ImGui.Text(text);

            wIdx++;
        }
        #endregion
        
        #region Talk Entries
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

            ImGui.SetCursorPos(offset + new Vector2(x, y));
            ImGui.Text(talkEntry.Text);

            var roomAreaRectTexture = _render.RenderRectangle(1, (int)tx, (int)ty*2,
                (255, 0, 0, 50), (255, 255, 255, 150));
            ImGui.SetCursorPos(offset + new Vector2(x, y));
            ImGui.Image(roomAreaRectTexture, new Vector2(tx, ty*2));
            
            var text = $"Text {tIdx}";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPos(offset + new Vector2(x + tx - textSize.X - 2, y + ty*2 - textSize.Y - 2));
            ImGui.Text(text);

            tIdx++;
        }
        #endregion
    }

    private void RenderKeyChars(Vector2 offset)
    {
        var colIdx = 0;
        foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars)
        {
            if (!keyChar.Initialised)
            {
                continue;
            }
            if (keyChar.OffScreen)
            {
                continue;
            }
            var (colR, colG, colB) = _colours[colIdx];
            colIdx++;
            colIdx = colIdx % _colours.Count;
            
            var x = keyChar.PositionX;
            var y = keyChar.PositionY;
            var z = keyChar.PositionZ;
            if (z < Game.ZDepthMin)
            {
                z = Game.ZDepthMin;
            }
            if (z > Game.ZDepthMax)
            {
                z = Game.ZDepthMax;
            }
            var zFactor = 1.0f;
            if (z < Game.ZDepthEven)
            {
                zFactor = (float)z / Game.ZDepthEven;
            }

            if (z > Game.ZDepthEven)
            {
                zFactor = (float)(Game.ZDepthMax - Game.ZDepthEven)/(Game.ZDepthMax - z);
            }

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
                var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
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
                    ImGui.SetCursorPos(spritePosition + new Vector2(destX, destY) * zFactor);
                    ImGui.Image(spriteTexture, new Vector2(sprite.SpriteWidth, sprite.SpriteHeight) * zFactor, spriteUv1, spriteUv2);   
                }
                width = highX - lowX;
                height = highY - lowY;
            }

            width = (int)Math.Ceiling(width * zFactor);
            height = (int)Math.Ceiling(height * zFactor);

            var rectTexture = _render.RenderRectangle(1, width, height, (colR, colG, colB, 50), (255, 255, 255, 150));

            var ox = x - width / 2.0f;
            var oy = y - height;
            ImGui.SetCursorPos(offset + new Vector2(ox , oy));
            ImGui.Image(rectTexture, new Vector2(width, height));

            var text = $"{keyCharId}";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPos(offset + new Vector2(ox + width - textSize.X - 2, oy + height - textSize.Y - 2));
            ImGui.Text(text);
        }
    }
}