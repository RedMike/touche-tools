using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class SpriteViewWindow : IWindow
{
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly SpriteViewSettings _viewSettings;
    private readonly ActiveRoom _room;
    private readonly ActiveSprite _sprite;
    private readonly ActiveFrame _frame;

    public SpriteViewWindow(RenderWindow render, WindowSettings windowSettings, SpriteViewSettings viewSettings, ActiveRoom room, ActiveSprite sprite, ActiveFrame frame)
    {
        _render = render;
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _room = room;
        _sprite = sprite;
        _frame = frame;
    }

    public void Render()
    {
        if (!_windowSettings.SpriteViewOpen)
        {
            return;
        }
        
        var viewW = Constants.MainWindowWidth;
        var viewH = 600.0f;
        ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
        ImGui.Begin("Sprite View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var contentRegion = ImGui.GetWindowContentRegionMin();
        var centerCursorPos = contentRegion + new Vector2(viewW / 2.0f, viewH / 2.0f);
        
        #region Room background
        if (_viewSettings.ShowRoom)
        {
            var ox = _viewSettings.RoomOffsetX;
            var oy = _viewSettings.RoomOffsetY;
            var (viewId, roomWidth, roomHeight, bytes) = _room.RoomView;
        
            var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
            var uv1 = new Vector2(ox / (float)roomWidth, oy / (float)roomHeight);
            var uv2 = new Vector2((ox+viewW) / (float)roomWidth, (oy+viewH) / (float)roomHeight);
            //TODO: clip rect
            ImGui.SetCursorPos(contentRegion);
            ImGui.Image(roomTexture, new Vector2(viewW, viewH), uv1, uv2);
        }
        #endregion
        
        var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = _sprite.SpriteView;
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, spriteViewId, spriteWidth, spriteHeight, spriteBytes);

        foreach (var (frameIndex, destX, destY, hFlip, vFlip) in _frame.PartsView)
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
}