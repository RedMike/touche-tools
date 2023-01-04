using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ImageManagementWindow : BaseWindow
{
    private readonly OpenedManifest _manifest;
    private readonly MainWindowState _state;
    private readonly ImageManagementState _imageManagementState;
    private readonly PackagePalettes _palettes;

    public ImageManagementWindow(MainWindowState state, ImageManagementState imageManagementState, PackagePalettes palettes, OpenedManifest manifest)
    {
        _state = state;
        _imageManagementState = imageManagementState;
        _palettes = palettes;
        _manifest = manifest;
    }

    public override void Render()
    {
        if (!_manifest.IsLoaded())
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
        var allImages = _manifest.GetAllImages().ToList();
        var includedImages = _manifest.GetIncludedImages();
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
                    _manifest.IncludeFile(path);
                }
                else
                {
                    _manifest.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            //button to select for preview
            var isSelected = _imageManagementState.SelectedImage == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }
            if (ImGui.Button(path.ShortenPath()))
            {
                _imageManagementState.SelectedImage = path;
                _imageManagementState.PreviewOpen = true;
                _imageManagementState.EditorOpen = true;
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
                var types = OpenedManifest.ImageTypeAsList();
                var origSelectedType = types.FindIndex(i => i == image.Type.ToString("G"));
                var selectedType = origSelectedType;
                ImGui.PushID($"{path}_type");
                ImGui.SetNextItemWidth(100.0f);
                ImGui.Combo("", ref selectedType, types.ToArray(), types.Count);
                ImGui.PopID();
                if (selectedType != origSelectedType)
                {
                    _manifest.LoadedManifest.Images[path].Type = Enum.Parse<OpenedManifest.ImageType>(types[selectedType]);
                }
                ImGui.SameLine();
                
                //image index
                var indexes = Enumerable.Range(0, 99).ToList();
                var indexList = indexes.Select(i => ImageName(image.Type, i)).ToArray();
                var origIndex = image.Index;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(120.0f);
                ImGui.Combo("", ref index, indexList, indexList.Length);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _manifest.LoadedManifest.Images[path].Index = index;
                }
            }
        }

        ImGui.Separator();

        //TODO: turn this into an image/better representation
        foreach (var (roomIdx, palette) in _palettes.GetPalettes())
        {
            if (ImGui.TreeNodeEx($"Room {roomIdx} Palette"))
            {
                foreach (var (colId, col) in palette.OrderBy(p => p.Key))
                {
                    ImGui.Text($"{ColorName(colId)} - ({col.R}, {col.G}, {col.B})");
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button,
                        new Vector4(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f, 1.0f));
                    ImGui.Button($"Example");
                    ImGui.PopStyleColor();
                }
                ImGui.TreePop();
            }
        }
        
        ImGui.Separator();
        if (ImGui.Button("Refresh Images"))
        {
            //TODO: add any new images to list
        }
        ImGui.End();
    }

    private string ColorName(int i)
    {
        var id = ColourHelper.ColourName(i);
        
        var customColours = _manifest.GetGame().CustomColors;

        if (customColours.ContainsKey(i))
        {
            return $"Custom {id}";
        }

        return $"Generated {id}";
    }

    private static string ImageName(OpenedManifest.ImageType type, int i)
    {
        if (type == OpenedManifest.ImageType.Sprite)
        {
            if (i == ToucheTools.Constants.Sprites.InventoryBackground1)
            {
                return "Inventory 1";
            }
            if (i == ToucheTools.Constants.Sprites.InventoryBackground2)
            {
                return "Inventory 2 (not usually used)";
            }
            if (i == ToucheTools.Constants.Sprites.InventoryBackground3)
            {
                return "Inventory 3 (not used)";
            }
            
            if (i == ToucheTools.Constants.Sprites.ActionMenu)
            {
                return "Action Menu";
            }

            if (i == ToucheTools.Constants.Sprites.ConversationMenu)
            {
                return "Conversation Menu";
            }
        }

        if (type == OpenedManifest.ImageType.Icon)
        {
            if (i == ToucheTools.Constants.Icons.DefaultMouseCursor)
            {
                return "Mouse Cursor";
            }
            if (i == ToucheTools.Constants.Icons.MoneyIcon)
            {
                return "Money Icon";
            }
        }

        return $"{type:G} {i}";
    }
}