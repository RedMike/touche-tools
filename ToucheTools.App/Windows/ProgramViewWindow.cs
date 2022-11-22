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
            var currentInstruction = (_activeProgramState.LastKnownOffset) == offset;
            var isBreakpoint = _activeProgramState.Breakpoints.Contains(offset);
            if (currentInstruction)
            {
                if (_activeProgramState.BreakpointHit)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.9f, 0.6f, 0.5f, 1.0f));
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.6f, 0.8f, 1.0f));
                }
            }
            else
            {
                if (isBreakpoint)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.9f, 0.2f, 0.2f, 1.0f));
                }
            }

            _viewState.OffsetYPos[offset] = ImGui.GetCursorPosY();
            _viewState.OffsetToIndex[offset] = idx;
            if (ImGui.Button($"{offset:D5}"))
            {
                if (isBreakpoint)
                {
                    _activeProgramState.Breakpoints.RemoveAll(x => x == offset);
                }
                else
                {
                    _activeProgramState.Breakpoints.Add(offset);
                }
            }
            ImGui.SameLine();
            ImGui.Text($" - {instruction}");
            if (currentInstruction)
            {
                ImGui.PopStyleColor();
                if (_activeProgramState.BreakpointHit || _activeProgramState.CurrentState.GetRunningScript()?.Status ==
                    ActiveProgramState.ProgramState.ScriptStatus.Running)
                {
                    _viewState.QueueScrollToOffset(offset);
                }
            }
            else
            {
                if (isBreakpoint)
                {
                    ImGui.PopStyleColor();
                }
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