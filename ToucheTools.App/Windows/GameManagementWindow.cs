using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class GameManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;

    public GameManagementWindow(OpenedPackage package, MainWindowState state)
    {
        _package = package;
        _state = state;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.GameManagement)
        {
            return;
        }

        var game = _package.GetGame();
        
        ImGui.Begin("Game Editor", ImGuiWindowFlags.NoCollapse);
        
        if (ImGui.TreeNodeEx("Actions"))
        {
            foreach (var (actionId, origLabel) in game.ActionDefinitions)
            {
                var id = $"{actionId}";
                if (actionId == Actions.DoNothing)
                {
                    id = "DoNothing";
                }
                if (actionId == Actions.LeftClick)
                {
                    id = "LeftClick";
                }
                if (actionId == Actions.LeftClickWithItem)
                {
                    id = "LeftClickWithItem";
                }
                ImGui.Text($"{id} -");
                ImGui.SameLine();
                var actionLabel = origLabel;
                ImGui.PushID($"Action{actionId}");
                ImGui.InputText("", ref actionLabel, 32);
                if (actionLabel != origLabel)
                {
                    game.ActionDefinitions[actionId] = actionLabel;
                }
                ImGui.PopID();
            }

            if (ImGui.Button("Add Action"))
            {
                var newId = 1;
                if (game.ActionDefinitions.Any(a => a.Key >= 0))
                {
                    newId = game.ActionDefinitions.Where(a => a.Key >= 0).Select(a => a.Key).Max() + 1;
                }

                game.ActionDefinitions[newId] = "~";
            }
            
            ImGui.TreePop();
        }
        
        ImGui.End();
    }
}