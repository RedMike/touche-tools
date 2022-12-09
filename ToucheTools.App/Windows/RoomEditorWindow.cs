using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

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

            var blankTexture = _render.RenderCheckerboardRectangle(25, roomImageWidth, roomImageHeight,
                (40, 30, 40, 255), (50, 40, 50, 255));
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(blankTexture, new Vector2(roomImageWidth, roomImageHeight));

            var roomTexture = _render.RenderImage(RenderWindow.RenderType.Primitive, roomImagePath, roomImageWidth,
                roomImageHeight, roomImageBytes);
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(roomTexture, new Vector2(roomImageWidth, roomImageHeight));
        }
        ImGui.EndChild();
        
        ImGui.End();
    }
}