using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class GameViewWindow : BaseWindow
{
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;

    public GameViewWindow(RenderWindow render, WindowSettings windowSettings, ActiveProgramState activeProgramState)
    {
        _render = render;
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
    }

    public override void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        var rectTexture = _render.RenderRectangle(1, Constants.GameScreenWidth, Constants.GameScreenHeight, (255, 0, 255, 255), (255, 255, 255, 255));

        var frameHeight = ImGui.GetFrameHeight();
        var offset = new Vector2(1.0f, 1.0f + frameHeight);
        ImGui.SetNextWindowPos(new Vector2(750.0f, 200.0f), ImGuiCond.Once);
        ImGui.SetNextWindowSize(new Vector2(Constants.GameScreenWidth + 2, Constants.GameScreenHeight + 2 + frameHeight));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.Begin("Game View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        ImGui.SetCursorPos(offset);
        ImGui.Image(rectTexture, new Vector2(Constants.GameScreenWidth, Constants.GameScreenHeight));

        ImGui.End();
        ImGui.PopStyleVar();
    }
}