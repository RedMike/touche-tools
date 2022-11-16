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

    public GameViewWindow(DatabaseModel model, RenderWindow render, WindowSettings windowSettings, ActiveProgramState activeProgramState, RoomImageRenderer roomImageRenderer, SpriteSheetRenderer spriteSheetRenderer)
    {
        _model = model;
        _render = render;
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
        _roomImageRenderer = roomImageRenderer;
        _spriteSheetRenderer = spriteSheetRenderer;
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
        var offsetX = 0;
        if (_activeProgramState.CurrentState.Flags.ContainsKey(Flags.ScreenOffsetX))
        {
            offsetX = _activeProgramState.CurrentState.Flags[Flags.ScreenOffsetX];
        }
        var offsetY = 0;
        if (_activeProgramState.CurrentState.Flags.ContainsKey(Flags.ScreenOffsetY))
        {
            offsetY = _activeProgramState.CurrentState.Flags[Flags.ScreenOffsetY];
        }
        var w = Constants.GameScreenWidth;
        var h = Constants.RoomHeight;

        var roomImageId = _model.Rooms[activeRoom].RoomImageNum;
        var roomImage = _model.RoomImages[roomImageId].Value;
        var palette = _model.Palettes[activeRoom]; //TODO: palette shifting
        var program = _model.Programs[_activeProgramState.CurrentState.CurrentProgram];

        var (viewId, bytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, -offsetX, -offsetY, w, h);

        var roomFullTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, w, h, bytes);

        ImGui.SetCursorPos(offset + new Vector2(-offsetX, -offsetY));
        ImGui.Image(roomFullTexture, new Vector2(w, h));

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
                var x = ox - offsetX;
                var y = oy - offsetY;
                if (background.IsScaled)
                {
                    //TODO: scaling
                }

                var (areaViewId, areaBytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, background.SrcX, background.SrcY, background.Rect.W, background.Rect.H);

                var roomAreaTexture = _render.RenderImage(RenderWindow.RenderType.Room, areaViewId, background.Rect.W, background.Rect.H, areaBytes);

                ImGui.SetCursorPos(offset + new Vector2(x, y));
                ImGui.Image(roomAreaTexture, new Vector2(background.Rect.W, background.Rect.H));
            }

            idx++;
        }
    }

    private void RenderKeyChars(Vector2 offset)
    {
        var colIdx = 0;
        foreach (var (keyCharId, keyChar) in _activeProgramState.CurrentState.KeyChars)
        {
            if (!keyChar.Initialised)
            {
                continue;
            }
            var (colR, colG, colB) = _colours[colIdx];
            colIdx++;
            colIdx = colIdx % _colours.Count;
            
            var x = keyChar.PositionX ?? 0;
            var y = keyChar.PositionY ?? 0;
            var z = keyChar.PositionZ ?? 0;
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

            if (keyChar.SpriteIndex != null && keyChar.SequenceIndex != null && keyChar.Character != null && _activeProgramState.CurrentState.LoadedRoom != null)
            {
                var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
                var spriteNum = _activeProgramState.CurrentState.SpriteIndexToNum[keyChar.SpriteIndex.Value];
                var sprite = _model.Sprites[spriteNum].Value;
                var palette = _model.Palettes[activeRoom];
                
                var (viewId, bytes) = _spriteSheetRenderer.RenderSpriteSheet(spriteNum, sprite, activeRoom, palette);
                var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, viewId, sprite.Width, sprite.Height, bytes);

                var seqNum = _activeProgramState.CurrentState.SequenceIndexToNum[keyChar.SequenceIndex.Value];
                var sequence = _model.Sequences[seqNum];
                var ch = sequence.Characters[keyChar.Character.Value];
                
                //TODO: anim id
                var anim = ch.Animations.First().Value;
                //TODO: dir id
                var dir = anim.Directions.First().Value;
                //TODO: frame id
                var frame = dir.Frames.First();

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