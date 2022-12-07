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
        var windowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();

        //sprite selection
        var sprites = _package.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedPackage.ImageType.Sprite)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var spriteList = sprites.Select(s => $"Sprite {s.Index} ({s.Key})").ToArray();
        var origSelectedSprite = sprites.FindIndex(p => p.Index == _animationManagementState.SelectedSpriteIndex);
        if (origSelectedSprite == -1)
        {
            origSelectedSprite = 0;
        }
        var selectedSprite = origSelectedSprite;
        ImGui.PushID("AnimationSprite");
        ImGui.SetNextItemWidth(windowSize.X/2.0f);
        ImGui.Combo("", ref selectedSprite, spriteList, spriteList.Length);
        if (selectedSprite != origSelectedSprite)
        {
            _animationManagementState.SelectedSpriteIndex = sprites[selectedSprite].Index;
        }
        ImGui.PopID();
        
        //palette selection
        ImGui.SameLine();
        var palettes = _package.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedPackage.ImageType.Room)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var paletteList = palettes.Select(s => $"Room {s.Index} ({s.Key})").ToArray();
        var origSelectedPalette = palettes.FindIndex(p => p.Index == _animationManagementState.SelectedPaletteIndex);
        if (origSelectedPalette == -1)
        {
            origSelectedPalette = 0;
        }
        var selectedPalette = origSelectedPalette;
        ImGui.PushID("AnimationPalette");
        ImGui.SetNextItemWidth(windowSize.X/2.0f);
        ImGui.Combo("", ref selectedPalette, paletteList, paletteList.Length);
        if (selectedPalette != origSelectedPalette)
        {
            _animationManagementState.SelectedPaletteIndex = palettes[selectedPalette].Index;
        }
        ImGui.PopID();
        
        ImGui.Separator();
        
        
        
        if (ImGui.Button("Close editor"))
        {
            _animationManagementState.EditorOpen = false;
        }
        
        ImGui.End();
    }
}