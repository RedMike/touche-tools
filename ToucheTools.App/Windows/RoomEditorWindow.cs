using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class RoomEditorWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly RoomManagementState _roomManagementState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;
    private readonly PackageRooms _rooms;

    public RoomEditorWindow(OpenedPackage package, MainWindowState state, RoomManagementState roomManagementState, RenderWindow render, PackageImages images, PackageRooms rooms)
    {
        _package = package;
        _state = state;
        _roomManagementState = roomManagementState;
        _render = render;
        _images = images;
        _rooms = rooms;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.RoomManagement)
        {
            return;
        }

        if (!_roomManagementState.EditorOpen)
        {
            return;
        }

        if (_roomManagementState.SelectedRoom == null)
        {
            return;
        }

        var room = _rooms.GetRoom(_roomManagementState.SelectedRoom);
        
        ImGui.Begin("Room Editor", ImGuiWindowFlags.NoCollapse);
        var windowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
        var childWindowSize = new Vector2(windowSize.X / 2.0f, windowSize.Y);
        var childWidowWidth = windowSize.X / 2.0f - 5.0f;

        var startPos = ImGui.GetCursorPos();
        ImGui.BeginChild("Room Controls", childWindowSize, false, ImGuiWindowFlags.ChildWindow);

        //room image
        var roomImages = _package.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedPackage.ImageType.Room)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var roomImageList = roomImages.Select(r => $"Room {r.Index} ({r.Key})").ToArray();
        var origSelectedRoomImage = roomImages.FindIndex(p => p.Index == room.RoomImageIndex);
        var selectedRoomImage = origSelectedRoomImage;
        ImGui.PushID("RoomImage");
        ImGui.SetNextItemWidth(childWidowWidth);
        ImGui.Combo("", ref selectedRoomImage, roomImageList, roomImageList.Length);
        if (selectedRoomImage != origSelectedRoomImage)
        {
            room.RoomImageIndex = roomImages[selectedRoomImage].Index;
        }
        ImGui.PopID();

        var roomWidth = 0;
        var roomHeight = 0;
        if (room.RoomImageIndex != null)
        {
            var roomImagePath = _package.GetIncludedImages().First(p =>
                p.Value.Type == OpenedPackage.ImageType.Room && p.Value.Index == room.RoomImageIndex).Key;
            (roomWidth, roomHeight, _) = _images.GetImage(roomImagePath);
        }
        
        ImGui.Separator();

        if (ImGui.TreeNodeEx("Walkable Points"))
        {
            foreach (var (walkablePointId, (origX, origY, origZ)) in room.WalkablePoints)
            {
                ImGui.Text($"{walkablePointId} -");
                ImGui.SameLine();
                var x = origX;
                var y = origY;
                var z = origZ;

                ImGui.PushID($"RoomWalkPoints{walkablePointId}X");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref x, 0, roomWidth, $"X {x}");
                if (x != origX)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
                ImGui.SameLine();

                ImGui.PushID($"RoomWalkPoints{walkablePointId}Y");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref y, 0, roomHeight, $"Y {y}");
                if (y != origY)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
                ImGui.SameLine();

                ImGui.PushID($"RoomWalkPoints{walkablePointId}Z");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref z, ToucheTools.Constants.Game.ZDepthMin, ToucheTools.Constants.Game.ZDepthMax,
                    $"Z {z}");
                if (z != origZ)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
            }

            ImGui.SetNextItemWidth(childWidowWidth);
            if (ImGui.Button("Add Walkable Point"))
            {
                var newId = 1;
                if (room.WalkablePoints.Count > 0)
                {
                    newId = room.WalkablePoints.Select(p => p.Key).Max() + 1;
                }

                room.WalkablePoints.Add(newId, (0, 0, ToucheTools.Constants.Game.ZDepthEven));
            }
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Walkable Lines"))
        {
            foreach (var ((p1, p2), (clip, area1, area2)) in room.WalkableLines.ToList())
            {
                ImGui.Text($"Point {p1} to point {p2}");
                ImGui.SameLine();
                ImGui.PushID($"RoomWalkLine{p1}-{p2}Delete");
                //TODO: rest
                if (ImGui.Button("Delete"))
                {
                    room.WalkableLines.Remove((p1, p2));
                }
                ImGui.PopID();
            }
            
            var points = room.WalkablePoints.Keys.ToList();
            var pointList = points.Select(p => $"Point {p}").ToArray();
            
            var origPoint1 = _roomManagementState.WalkableLinePoint1;
            var point1 = origPoint1;
            ImGui.PushID($"RoomWalkLineP1");
            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            ImGui.Combo("", ref point1, pointList, pointList.Length);
            if (point1 != origPoint1)
            {
                _roomManagementState.WalkableLinePoint1 = point1;
            }
            ImGui.PopID();
            ImGui.SameLine();
            
            var origPoint2 = _roomManagementState.WalkableLinePoint2;
            var point2 = origPoint2;
            ImGui.PushID($"RoomWalkLineP2");
            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            ImGui.Combo("", ref point2, pointList, pointList.Length);
            if (point2 != origPoint2)
            {
                _roomManagementState.WalkableLinePoint2 = point2;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            if (ImGui.Button("Add"))
            {
                var p = (points[point1], points[point2]);
                if (room.WalkableLines.ContainsKey(p))
                {
                    throw new Exception("Line already exists");
                }

                room.WalkableLines[p] = (0, -1, -1);
            }

            ImGui.TreePop();
        }

        ImGui.Separator();
        if (ImGui.Button("Save"))
        {
            _rooms.SaveRoom(_roomManagementState.SelectedRoom);
        }
        if (ImGui.Button("Close Editor"))
        {
            _roomManagementState.EditorOpen = false;
        }
        
        ImGui.EndChild();

        ImGui.SetCursorPos(startPos + new Vector2(windowSize.X/2.0f, 0.0f));
        ImGui.BeginChild("Room View", childWindowSize, true, ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        var imagePos = ImGui.GetCursorPos();
        if (room.RoomImageIndex != null)
        {
            var roomImagePath = _package.GetIncludedImages().First(p =>
                    p.Value.Type == OpenedPackage.ImageType.Room && p.Value.Index == room.RoomImageIndex).Key;
            var (roomImageWidth, roomImageHeight, roomImageBytes) = _images.GetImage(roomImagePath);

            //blank texture
            var blankTexture = _render.RenderCheckerboardRectangle(25, roomImageWidth, roomImageHeight,
                (40, 30, 40, 255), (50, 40, 50, 255));
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(blankTexture, new Vector2(roomImageWidth, roomImageHeight));

            //room texture
            var roomTexture = _render.RenderImage(RenderWindow.RenderType.Primitive, roomImagePath, roomImageWidth,
                roomImageHeight, roomImageBytes);
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(roomTexture, new Vector2(roomImageWidth, roomImageHeight));
            
            //points
            var pw = 10;
            var pointTexture = _render.RenderRectangle(1, pw, pw,
                (255, 255, 255, 50), (255, 255, 255, 255));
            foreach (var (pointId, (x, y, z)) in room.WalkablePoints)
            {
                var zFactor = Game.GetZFactor(z);
                var ow = pw * zFactor;
                var ox = x;
                var oy = y;
                ImGui.SetCursorPos(imagePos + new Vector2(ox - ow/2, oy - ow/2));
                ImGui.Image(pointTexture, new Vector2(ow, ow));
                
                ImGui.SetCursorPos(imagePos + new Vector2(ox, oy));
                ImGui.Text($"{pointId}");
            }
            
            //walk lines
            var drawList = ImGui.GetWindowDrawList();
            foreach (var ((point1, point2), (_, _, _)) in room.WalkableLines)
            {
                var (p1X, p1Y, _) = room.WalkablePoints[point1];
                var (p2X, p2Y, _) = room.WalkablePoints[point2];
                var curPos = ImGui.GetWindowPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + imagePos;
                drawList.AddLine(curPos + new Vector2(p1X, p1Y), curPos + new Vector2(p2X, p2Y), ImGui.GetColorU32(new Vector4(0.7f, 0.9f, 0.6f, 1.0f)), 1.0f);
            }
        }
        ImGui.EndChild();
        
        ImGui.End();
    }
}