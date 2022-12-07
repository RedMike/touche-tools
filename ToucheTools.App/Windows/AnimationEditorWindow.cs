using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class AnimationEditorWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly AnimationManagementState _animationManagementState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;

    public AnimationEditorWindow(OpenedPackage package, MainWindowState state, AnimationManagementState animationManagementState, RenderWindow render, PackageImages images)
    {
        _package = package;
        _state = state;
        _animationManagementState = animationManagementState;
        _render = render;
        _images = images;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.AnimationManagement)
        {
            return;
        }

        if (!_animationManagementState.EditorOpen)
        {
            return;
        }

        if (_animationManagementState.SelectedAnimation == null)
        {
            return;
        }
        
        if (!_package.Value.Animations.ContainsKey(_animationManagementState.SelectedAnimation))
        {
            //error?
            return;
        }
        
        ImGui.Begin("Animation Editor", ImGuiWindowFlags.NoCollapse);
        
        
        
        if (ImGui.Button("Close editor"))
        {
            _animationManagementState.EditorOpen = false;
        }
        
        ImGui.End();
    }
}