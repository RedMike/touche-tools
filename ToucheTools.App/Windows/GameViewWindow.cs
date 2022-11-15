using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class GameViewWindow : BaseWindow
{
    private readonly DatabaseModel _model;
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;
    private readonly RoomImageRenderer _roomImageRenderer;

    public GameViewWindow(DatabaseModel model, RenderWindow render, WindowSettings windowSettings, ActiveProgramState activeProgramState, RoomImageRenderer roomImageRenderer)
    {
        _model = model;
        _render = render;
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
        _roomImageRenderer = roomImageRenderer;
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
        RenderSprites(offset);

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private void RenderRoom(Vector2 offset)
    {
        if (_activeProgramState.CurrentState == null || _activeProgramState.CurrentState.LoadedRoom == null)
        {
            return;
        }

        var activeRoom = _activeProgramState.CurrentState.LoadedRoom.Value;
        var offsetX = 0;
        var offsetY = 0;
        var w = Constants.GameScreenWidth;
        var h = Constants.RoomHeight;

        var roomImageId = _model.Rooms[activeRoom].RoomImageNum;
        var roomImage = _model.RoomImages[roomImageId].Value;
        var palette = _model.Palettes[activeRoom]; //TODO: palette shifting

        var (viewId, bytes) = _roomImageRenderer.RenderRoomImage(roomImageId, roomImage, activeRoom, palette, offsetX, offsetY, w, h);

        var roomFullTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, w, h, bytes);

        ImGui.SetCursorPos(offset);
        ImGui.Image(roomFullTexture, new Vector2(w, h));
    }

    private void RenderSprites(Vector2 offset)
    {
        //TODO:
    }
}