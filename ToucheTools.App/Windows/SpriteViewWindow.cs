using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class SpriteViewWindow : IWindow
{
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;
    private readonly SpriteViewSettings _viewSettings;

    public SpriteViewWindow(RenderWindow render, WindowSettings windowSettings, ActiveData activeData, SpriteViewSettings viewSettings)
    {
        _render = render;
        _windowSettings = windowSettings;
        _activeData = activeData;
        _viewSettings = viewSettings;
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

        var centerCursorPos = new Vector2(viewW / 2.0f, viewH / 2.0f);
        
        #region Room background
        if (_viewSettings.ShowRoom)
        {
            var ox = _viewSettings.RoomOffsetX;
            var oy = _viewSettings.RoomOffsetY;
            var (viewId, roomWidth, roomHeight, bytes) = _activeData.RoomView;
        
            var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
            var uv1 = new Vector2(ox / (float)roomWidth, oy / (float)roomHeight);
            var uv2 = new Vector2((ox+viewW) / (float)roomWidth, (oy+viewH) / (float)roomHeight);
            ImGui.SetCursorPos(new Vector2(0.0f, 0.0f));
            ImGui.Image(roomTexture, new Vector2(viewW, viewH), uv1, uv2);
        }
        #endregion
        
        var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = _activeData.SpriteView;
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, spriteViewId, spriteWidth, spriteHeight, spriteBytes);

        foreach (var (frameIndex, destX, destY, hFlip, vFlip) in _viewSettings.PartsView)
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