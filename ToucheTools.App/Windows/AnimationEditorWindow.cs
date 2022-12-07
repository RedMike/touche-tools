using System.Numerics;
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
    private readonly PackageAnimations _animations;

    public AnimationEditorWindow(OpenedPackage package, MainWindowState state, AnimationManagementState animationManagementState, RenderWindow render, PackageImages images, PackageAnimations animations)
    {
        _package = package;
        _state = state;
        _animationManagementState = animationManagementState;
        _render = render;
        _images = images;
        _animations = animations;
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

        if (_animationManagementState.SelectedAnimationPath == null)
        {
            return;
        }
        
        if (!_package.Value.Animations.ContainsKey(_animationManagementState.SelectedAnimationPath))
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
        var imagePos = ImGui.GetCursorPos() + new Vector2(windowSize.X / 2.0f, 0.0f);
        var animation = _animations.GetAnimation(_animationManagementState.SelectedAnimationPath);
        
        //character field
        var characters = animation.CharToFrameFlag.Select(p => p.Key).OrderBy(p => p).ToList();
        if (characters.Count == 0)
        {
            characters = new List<int>() { 0 };
        }
        else
        {
            characters.Add(characters.Max());
        }
        var characterList = characters.Select((c, i) => $"{DisplayCharacter(c)}{((i == characters.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedCharacter = _animationManagementState.SelectedCharacter;
        var selectedCharacter = origSelectedCharacter;
        ImGui.PushID("AnimationCharacter");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedCharacter, characterList, characterList.Length);
        if (selectedCharacter != origSelectedCharacter)
        {
            _animationManagementState.SelectedCharacter = characters[selectedCharacter];
        }
        ImGui.PopID();
        
        //animation field
        var animations = animation.FrameMappings
            .Where(p => p.Key.Item1 == selectedCharacter)
            .Select(p => p.Key.Item2)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
        if (animations.Count == 0)
        {
            animations = new List<int>() { 0 };
        }
        else
        {
            animations.Add(animations.Max());
        }
        var animationList = animations.Select((a, i) => $"{DisplayAnimation(a)}{((i == animations.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedAnimation = _animationManagementState.SelectedCharacter;
        var selectedAnimation = origSelectedAnimation;
        ImGui.PushID("AnimationAnimation");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedAnimation, animationList, animationList.Length);
        if (selectedAnimation != origSelectedAnimation)
        {
            _animationManagementState.SelectedAnimation = animations[selectedAnimation];
        }
        ImGui.PopID();
        
        //image
        var w = (int)(windowSize.X / 3);
        var h = w;
        var blankBackground = _render.RenderCheckerboardRectangle(20, w, h,
            (40, 30, 40, 255), (50, 40, 50, 255)
        );
        ImGui.SetCursorPos(imagePos);
        ImGui.Image(blankBackground, new Vector2(w, h));
        
        ImGui.Separator();
        
        if (ImGui.Button("Close editor"))
        {
            _animationManagementState.EditorOpen = false;
        }
        
        ImGui.End();
    }

    private static string DisplayCharacter(int character)
    {
        return $"Character {character}";
    }

    private static string DisplayAnimation(int animation)
    {
        if (animation == 0)
        {
            return $"Idle animation";
        }

        if (animation == 1)
        {
            return $"Move animation";
        }
        return $"Animation {animation}";
    }
}