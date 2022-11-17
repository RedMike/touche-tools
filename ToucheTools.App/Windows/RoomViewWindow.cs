using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class RoomViewWindow : IWindow
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
    
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveRoom _room;
    private readonly RoomViewSettings _viewSettings;
    private readonly MultiActiveRects _rects;
    private readonly MultiActiveBackgrounds _backgrounds;
    private readonly MultiActiveAreas _areas;
    private readonly MultiActivePoints _points;

    public RoomViewWindow(RenderWindow render, WindowSettings windowSettings, ActiveRoom room, RoomViewSettings viewSettings, MultiActiveRects rects, MultiActiveBackgrounds backgrounds, MultiActiveAreas areas, MultiActivePoints points)
    {
        _render = render;
        _windowSettings = windowSettings;
        _room = room;
        _viewSettings = viewSettings;
        _rects = rects;
        _backgrounds = backgrounds;
        _areas = areas;
        _points = points;
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
        var areaOffsetX = _viewSettings.AreaOffsetX;
        var areaOffsetY = _viewSettings.AreaOffsetY;
    
        var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
        var contentRegion = ImGui.GetWindowContentRegionMin();
        ImGui.SetCursorPos(new Vector2(contentRegion.X, contentRegion.Y));
        ImGui.Image(roomTexture, new Vector2(roomWidth, roomHeight));

        if (_viewSettings.ShowRects)
        {
            if (_rects.RectsView.Count > _colours.Count)
            {
                throw new Exception($"Not enough colours defined in code for number of rectangles, need {_rects.RectsView.Count}");
            }

            var idx = 0;
            foreach (var (rectX, rectY, rectW, rectH) in _rects.RectsView)
            {
                var (rectR, rectG, rectB) = _colours[idx];
                var borderR = (byte)Math.Min(255, rectR * 2.5f + 150);
                var borderG = (byte)Math.Min(255, rectG * 2.5f + 100);
                var borderB = (byte)Math.Min(255, rectB * 2.5f + 150);
                var borderWidth = 1;
                var rectTexture = _render.RenderRectangle(borderWidth, rectW, rectH, (rectR, rectG, rectB, 150), (borderR, borderG, borderB, 255));
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX, contentRegion.Y + rectY + areaOffsetY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text = $"Rect {idx} ({rectX},{rectY} x {rectW},{rectH})";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX + rectW - textSize.X - borderWidth, contentRegion.Y + rectY + areaOffsetY + rectH - textSize.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text);
                ImGui.PopStyleColor();
                
                idx++;
            }
        }

        if (_viewSettings.ShowBackgrounds)
        {
            if (_backgrounds.BackgroundsView.Count > _colours.Count)
            {
                throw new Exception($"Not enough colours defined in code for number of backgrounds, need {_backgrounds.BackgroundsView.Count}");
            }

            var idx = 0;
            foreach (var ((rectX, rectY, rectW, rectH), (srcX, srcY), type, offset, scaleMul, scaleDiv) in _backgrounds.BackgroundsView)
            {
                var (rectR, rectG, rectB) = _colours[idx];
                var borderR = (byte)Math.Min(255, rectR * 2.5f + 150);
                var borderG = (byte)Math.Min(255, rectG * 2.5f + 100);
                var borderB = (byte)Math.Min(255, rectB * 2.5f + 150);
                var borderWidth = 1;
                
                var rectTexture = _render.RenderRectangle(borderWidth, rectW, rectH, (rectR, rectG, rectB, 150), (borderR, borderG, borderB, 255));
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX, contentRegion.Y + rectY + areaOffsetY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text = $"Background {idx} destination ({rectX},{rectY} x {rectW},{rectH})";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX + rectW - textSize.X - borderWidth, contentRegion.Y + rectY + areaOffsetY + rectH - textSize.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text);
                ImGui.PopStyleColor();
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + srcX + areaOffsetX, contentRegion.Y + srcY + areaOffsetY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text2 = $"Background {idx} source ({srcX},{srcY} x {rectW},{rectH})";
                var textSize2 = ImGui.CalcTextSize(text2);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + srcX + areaOffsetX + rectW - textSize2.X - borderWidth, contentRegion.Y + srcY + areaOffsetY + rectH - textSize2.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text2);
                ImGui.PopStyleColor();
                
                idx++;
            }
        }
        
        if (_viewSettings.ShowAreas)
        {
            if (_areas.AreaView.Count > _colours.Count)
            {
                throw new Exception($"Not enough colours defined in code for number of areas, need {_areas.AreaView.Count}");
            }

            var idx = 0;
            foreach (var ((rectX, rectY, rectW, rectH), (srcX, srcY), type, offset, scaleMul, scaleDiv) in _areas.AreaView)
            {
                var (rectR, rectG, rectB) = _colours[idx];
                var borderR = (byte)Math.Min(255, rectR * 2.5f + 150);
                var borderG = (byte)Math.Min(255, rectG * 2.5f + 100);
                var borderB = (byte)Math.Min(255, rectB * 2.5f + 150);
                var borderWidth = 1;
                
                var rectTexture = _render.RenderRectangle(borderWidth, rectW, rectH, (rectR, rectG, rectB, 150), (borderR, borderG, borderB, 255));
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX, contentRegion.Y + rectY + areaOffsetY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text = $"Area {idx} destination ({rectX},{rectY} x {rectW},{rectH})";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + rectX + areaOffsetX + rectW - textSize.X - borderWidth, contentRegion.Y + rectY + areaOffsetY + rectH - textSize.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text);
                ImGui.PopStyleColor();
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + srcX + areaOffsetX, contentRegion.Y + srcY + areaOffsetY));
                ImGui.Image(rectTexture, new Vector2(rectW, rectH));

                var text2 = $"Area {idx} source ({srcX},{srcY} x {rectW},{rectH})";
                var textSize2 = ImGui.CalcTextSize(text2);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + srcX + areaOffsetX + rectW - textSize2.X - borderWidth, contentRegion.Y + srcY + areaOffsetY + rectH - textSize2.Y - borderWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text2);
                ImGui.PopStyleColor();
                
                idx++;
            }
        }
        
        if (_viewSettings.ShowPoints)
        {
            if (_points.PointsView.Count > _colours.Count)
            {
                throw new Exception($"Not enough colours defined in code for number of points, need {_points.PointsView.Count}");
            }

            var idx = 0;
            foreach (var (pointX, pointY, pointZ, pointOrder) in _points.PointsView)
            {
                var (rectR, rectG, rectB) = _colours[idx];
                var borderR = (byte)Math.Min(255, rectR * 2.5f + 150);
                var borderG = (byte)Math.Min(255, rectG * 2.5f + 100);
                var borderB = (byte)Math.Min(255, rectB * 2.5f + 150);
                var borderWidth = 1;
                var pointW = 25;
                var pointH = 25;
                var ox = pointX;
                var oy = pointY;
                var oz = pointZ;
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
                
                var rectTexture = _render.RenderRectangle(borderWidth, (int)(Math.Max(1.0f, pointW * zFactor)), (int)(Math.Max(1.0f, pointH * zFactor)), (rectR, rectG, rectB, 150), (borderR, borderG, borderB, 255));
                
                ImGui.SetCursorPos(new Vector2(contentRegion.X + ox + areaOffsetX, contentRegion.Y + oy + areaOffsetY) * zFactor);
                ImGui.Image(rectTexture, new Vector2(pointW, pointH) * zFactor);

                var text = $"Point {idx}";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPos(new Vector2(contentRegion.X + ox + areaOffsetX + pointW - textSize.X - borderWidth, contentRegion.Y + oy + areaOffsetY + pointH - textSize.Y - borderWidth) * zFactor);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(borderR/255.0f, borderG/255.0f, borderB/255.0f, 1.0f));
                ImGui.Text(text);
                ImGui.PopStyleColor();

                idx++;
            }
        }

        
        ImGui.End();
    }
}