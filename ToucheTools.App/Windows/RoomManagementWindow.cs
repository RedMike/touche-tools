using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class RoomManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly RoomManagementState _roomManagementState;

    public RoomManagementWindow(OpenedPackage package, MainWindowState state, RoomManagementState roomManagementState)
    {
        _package = package;
        _state = state;
        _roomManagementState = roomManagementState;
    }

    public override void Render()
    {
        if (!_package.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.RoomManagement)
        {
            return;
        }
        
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Rooms", ImGuiWindowFlags.NoCollapse);
        var allRooms = _package.GetAllRooms().ToList();
        var includedRooms = _package.GetIncludedRooms();
        foreach (var path in allRooms)
        {
            //included checkbox
            var origIsIncluded = includedRooms.ContainsKey(path);
            var isIncluded = origIsIncluded;
            ImGui.PushID($"{path}_include");
            ImGui.Checkbox("", ref isIncluded);
            ImGui.PopID();
            if (isIncluded != origIsIncluded)
            {
                if (isIncluded)
                {
                    _package.IncludeFile(path);
                }
                else
                {
                    _package.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            //button to select for preview
            var isSelected = _roomManagementState.SelectedRoom == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }
            if (ImGui.Button(path))
            {
                _roomManagementState.SelectedRoom = path;
                _roomManagementState.PreviewOpen = true;
                _roomManagementState.EditorOpen = true;
            }
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
        }
        
        ImGui.End();
    }
}