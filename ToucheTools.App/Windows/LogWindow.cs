using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class LogWindow : IWindow
{
    private static readonly Vector4 ErrorColour = new Vector4(0.9f, 0.1f, 0.2f, 1.0f);
    
    private readonly LogData _logData;
    
    public LogWindow(LogData logData)
    {
        _logData = logData;
    }

    public void Render()
    {
        ImGui.SetNextWindowPos(new Vector2(0.0f, Constants.MainWindowHeight-100.0f));
        ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth, 100.0f));
        ImGui.Begin("Log", ImGuiWindowFlags.NoDocking);
        foreach (var (isError, msg) in _logData.List())
        {
            if (isError)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ErrorColour);
            }

            ImGui.TextWrapped(msg);
            
            if (isError)
            {
                ImGui.PopStyleColor();
            }
        }
        ImGui.End();
    }
}