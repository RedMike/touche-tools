﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class SpriteViewWindow : IWindow
{
    private readonly DebuggingGame _game;
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly SpriteViewSettings _viewSettings;
    private readonly ActiveRoom _room;
    private readonly ActiveSprite _sprite;
    private readonly ActiveFrame _frame;
    private readonly ActiveDirection _direction;
    private readonly SpriteViewState _viewState;

    public SpriteViewWindow(RenderWindow render, WindowSettings windowSettings, SpriteViewSettings viewSettings, ActiveRoom room, ActiveSprite sprite, ActiveFrame frame, ActiveDirection direction, SpriteViewState viewState, DebuggingGame game)
    {
        _render = render;
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _room = room;
        _sprite = sprite;
        _frame = frame;
        _direction = direction;
        _viewState = viewState;
        _game = game;
    }

    public void Render()
    {
        if (!_game.IsLoaded())
        {
            return;
        }
        if (!_windowSettings.SpriteViewOpen)
        {
            return;
        }
        
        var viewW = Constants.MainWindowWidth;
        var viewH = 600.0f;
        var movementOffset = Vector2.Zero;
        ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
        ImGui.Begin("Sprite View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        var contentRegion = ImGui.GetWindowContentRegionMin();
        var contentRegionMax = ImGui.GetWindowContentRegionMax();
        var blankRect = _render.RenderRectangle(0, (int)(contentRegionMax - contentRegion).X,
            (int)(contentRegionMax - contentRegion).Y,
            (70, 0, 70, 255), (0, 0, 0, 0)
        );
        ImGui.SetCursorPos(new Vector2(contentRegion.X, contentRegion.Y));
        ImGui.Image(blankRect, contentRegionMax-contentRegion);
        
        if (_viewSettings.ShowEntireSheet)
        {
            var spritePosition = contentRegion;
            
            var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = _sprite.SpriteView;
            var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, spriteViewId, spriteWidth, spriteHeight, spriteBytes);
            
            ImGui.SetCursorPos(spritePosition);
            ImGui.Image(spriteTexture, new Vector2(spriteWidth, spriteHeight));
        }
        else
        {
            var (movementOffsetX, movementOffsetY, movementOffsetZ) = _viewState.PositionOffset;
            movementOffset = new Vector2(movementOffsetX, movementOffsetY + movementOffsetZ);
            
            var spritePosition = contentRegion + new Vector2(viewW / 2.0f, 3 * viewH / 4.0f) + movementOffset;
            
            #region Room background
            if (_viewSettings.ShowRoom)
            {
                var ox = _viewSettings.RoomOffsetX;
                var oy = _viewSettings.RoomOffsetY;
                var (viewId, roomWidth, roomHeight, bytes) = _room.RoomView;
            
                var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
                var uv1 = new Vector2(ox / (float)roomWidth, oy / (float)roomHeight);
                var uv2 = new Vector2((ox+viewW) / (float)roomWidth, (oy+viewH) / (float)roomHeight);
                ImGui.SetCursorPos(contentRegion);
                var windowOffset = ImGui.GetWindowPos();
                ImGui.PushClipRect(windowOffset+contentRegion, windowOffset+contentRegion + new Vector2(roomWidth-ox, roomHeight-oy), false); 
                ImGui.Image(roomTexture, new Vector2(viewW, viewH), uv1, uv2);
                ImGui.PopClipRect();
            }
            #endregion
            
            var (spriteViewId, spriteWidth, spriteHeight, spriteTileWidth, spriteTileHeight, spriteBytes) = _sprite.SpriteView;
            var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, spriteViewId, spriteWidth, spriteHeight, spriteBytes);
            var direction = _direction.Elements[_direction.Active];
            
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

                //fix the position based on the direction
                var ox = 0;
                if (direction == 3)
                {
                    ox = -spriteTileWidth;
                }
                ImGui.SetCursorPos(spritePosition + new Vector2(destX + ox, destY));
                ImGui.Image(spriteTexture, new Vector2(spriteTileWidth, spriteTileHeight), spriteUv1, spriteUv2);
            }

            var anchorW = 10;
            var anchorRect = _render.RenderRectangle(1, anchorW, anchorW,
                (255, 255, 255, 50), (255, 255, 255, 255));
            ImGui.SetCursorPos(spritePosition + new Vector2(-anchorW/2.0f, -anchorW/2.0f));
            ImGui.Image(anchorRect, new Vector2(anchorW, anchorW));

            var ((dx, dy, dz), delay) = _frame.FrameView.Value;
            var text = $"Walk ({dx} X, {dy} Y, {dz} Z)\nDelay {delay}";
            ImGui.SetCursorPos(spritePosition + new Vector2(0, 50.0f) - movementOffset);
            ImGui.Text(text);
        }
        
        
        ImGui.End();
    }
}