﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class RoomViewWindow : IWindow
{
    private readonly List<(byte, byte, byte)> _rectColours = new List<(byte, byte, byte)>()
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
    
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveRoom _room;
    private readonly RoomViewSettings _viewSettings;

    public RoomViewWindow(RenderWindow render, WindowSettings windowSettings, ActiveRoom room, RoomViewSettings viewSettings)
    {
        _render = render;
        _windowSettings = windowSettings;
        _room = room;
        _viewSettings = viewSettings;
    }

    public void Render()
    {
        if (!_windowSettings.RoomViewOpen)
        {
            return;
        }
        
        ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth, 600.0f));
        ImGui.Begin("Room View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var (viewId, roomWidth, roomHeight, bytes) = _room.RoomView;
    
        var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
        var contentRegion = ImGui.GetWindowContentRegionMin();
        ImGui.SetCursorPos(new Vector2(contentRegion.X, contentRegion.Y));
        ImGui.Image(roomTexture, new Vector2(roomWidth, roomHeight));

        if (_viewSettings.ShowRects)
        {
            if (_viewSettings.RectsView.Count > _rectColours.Count)
            {
                throw new Exception($"Not enough colours defined in code for number of rectangles, need {_viewSettings.RectsView.Count}");
            }

            var idx = 0;
            //TODO: hide rects based on checkboxes
            foreach (var (rectX, rectY, rectW, rectH) in _viewSettings.RectsView)
            {
                var (rectR, rectG, rectB) = _rectColours[idx];
                var borderR = (byte)Math.Min(255, rectR * 2.5f + 150);
                var borderG = (byte)Math.Min(255, rectG * 2.5f + 100);
                var borderB = (byte)Math.Min(255, rectB * 2.5f + 150);
                var borderWidth = 1;
                var rectTexture = _render.RenderRectangle(borderWidth, rectW, rectH, (rectR, rectG, rectB, 150), (borderR, borderG, borderB, 255));
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX, contentRegion.Y + rectY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text = $"Rect {idx} ({rectX}x{rectY}, {rectW}x{rectH})";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + rectW - textSize.X - borderWidth, contentRegion.Y + rectY + rectH - textSize.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text);
                ImGui.PopStyleColor();
                
                idx++;
            }
        }
        
        ImGui.End();
    }
}