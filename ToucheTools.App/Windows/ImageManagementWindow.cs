using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ImageManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly PreviewInfoState _previewState;

    public ImageManagementWindow(OpenedPackage package, MainWindowState state, PreviewInfoState previewState)
    {
        _package = package;
        _state = state;
        _previewState = previewState;
    }

    public override void Render()
    {
        if (!_package.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.ImageManagement)
        {
            return;
        }
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Images", ImGuiWindowFlags.NoCollapse);
        var allImages = _package.GetAllImages().ToList();
        var includedImages = _package.GetIncludedImages();
        foreach (var path in allImages)
        {
            //included checkbox
            var origIsIncluded = includedImages.ContainsKey(path);
            var isIncluded = origIsIncluded;
            ImGui.PushID($"{path}_include");
            ImGui.Checkbox("", ref isIncluded);
            ImGui.PopID();
            if (isIncluded != origIsIncluded)
            {
                if (isIncluded)
                {
                    _package.IncludeFile(path);
                }
                else
                {
                    _package.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            //button to select for preview
            var isSelected = _previewState.ImagePreviewOpen && _previewState.SelectedImage == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }
            if (ImGui.Button(path))
            {
                _previewState.SelectedImage = path;
                _previewState.ImagePreviewOpen = true;
            }
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
            
            if (isIncluded)
            {
                ImGui.SameLine();
                //image type
                var image = includedImages[path];
                var types = OpenedPackage.ImageTypeAsList();
                var origSelectedType = types.FindIndex(i => i == image.Type.ToString("G"));
                var selectedType = origSelectedType;
                ImGui.PushID($"{path}_type");
                ImGui.SetNextItemWidth(100.0f);
                ImGui.Combo("", ref selectedType, types.ToArray(), types.Count);
                ImGui.PopID();
                if (selectedType != origSelectedType)
                {
                    //TODO: change image type
                }
                ImGui.SameLine();
                
                //image index
                var indexes = Enumerable.Range(1, 99).ToList();
                var origIndex = image.Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(60.0f);
                ImGui.Combo("", ref index, indexes.Select(i => i.ToString()).ToArray(), indexes.Count);
                ImGui.PopID();
                if (index != origIndex)
                {
                    //TODO: change image index
                }
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Refresh Images"))
        {
            //TODO: add any new images to list
        }
        ImGui.End();
    }
}