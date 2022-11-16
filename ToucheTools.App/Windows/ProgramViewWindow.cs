using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ProgramViewWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ProgramViewSettings _viewSettings;
    private readonly ProgramViewState _viewState;
    private readonly ActiveProgramState _activeProgramState;

    public ProgramViewWindow(WindowSettings windowSettings, ProgramViewSettings viewSettings, ProgramViewState viewState, ActiveProgramState activeProgramState)
    {
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _viewState = viewState;
        _activeProgramState = activeProgramState;
    }

    public void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        var viewW = 400.0f;
        var viewH = 600.0f;
        ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
        ImGui.Begin("Program View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        _viewState.OffsetToIndex = new Dictionary<int, int>();
        _viewState.OffsetYPos = new Dictionary<int, float>();
        var idx = 0;
        foreach (var (offset, instruction) in _viewSettings.InstructionsView)
        {
            var currentInstruction = (_activeProgramState.CurrentState?.CurrentOffset ?? 0) == offset;
            if (currentInstruction)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.6f, 0.8f, 1.0f));
            }

            _viewState.OffsetYPos[offset] = ImGui.GetCursorPosY();
            _viewState.OffsetToIndex[offset] = idx;
            ImGui.Button($"{offset:D5}");
            ImGui.SameLine();
            ImGui.Text($" - {instruction}");
            if (currentInstruction)
            {
                ImGui.PopStyleColor();
                _viewState.QueueScrollToOffset(offset);
            }

            idx++;
        }

        var scrollTo = _viewState.GetQueuedScroll();
        if (scrollTo != null)
        {
            var scrollBack = ImGui.GetWindowHeight() / 2.0f;
            ImGui.SetScrollY(scrollTo.Value - scrollBack);
        }
    
        ImGui.End();
    }
}