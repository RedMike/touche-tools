﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class AnimationManagementWindow : BaseWindow
{
    private readonly MainWindowState _state;
    private readonly AnimationManagementState _animationManagementState;
    private readonly OpenedManifest _manifest;

    public AnimationManagementWindow(MainWindowState state, AnimationManagementState animationManagementState, OpenedManifest manifest)
    {
        _state = state;
        _animationManagementState = animationManagementState;
        _manifest = manifest;
    }

    public override void Render()
    {
        if (!_manifest.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.AnimationManagement)
        {
            return;
        }
        
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Animations", ImGuiWindowFlags.NoCollapse);

        var allAnimations = _manifest.GetAllAnimations();
        var includedAnimations = _manifest.GetIncludedAnimations();
        foreach (var path in allAnimations)
        {
            //included checkbox
            var origIsIncluded = includedAnimations.ContainsKey(path);
            var isIncluded = origIsIncluded;
            ImGui.PushID($"{path}_include");
            ImGui.Checkbox("", ref isIncluded);
            ImGui.PopID();
            if (isIncluded != origIsIncluded)
            {
                if (isIncluded)
                {
                    _manifest.IncludeFile(path);
                }
                else
                {
                    _manifest.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            //button to select for preview
            var isSelected = _animationManagementState.SelectedAnimationPath == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }
            if (ImGui.Button(path.ShortenPath()))
            {
                _animationManagementState.SelectedAnimationPath = path;
                _animationManagementState.PreviewOpen = true;
                _animationManagementState.EditorOpen = true;
            }
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
            
            if (isIncluded)
            {
                ImGui.SameLine();
                //image index
                var indexes = Enumerable.Range(1, 99).ToList();
                var origIndex = includedAnimations[path].Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(60.0f);
                ImGui.Combo("", ref index, indexes.Select(i => i.ToString()).ToArray(), indexes.Count);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _manifest.LoadedManifest.Animations[path].Index = index + 1;
                }
            }
        }

        if (ImGui.Button("Refresh Animations"))
        {
            //TODO: load from files
        }
        ImGui.End();
    }
}