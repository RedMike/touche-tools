using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ImageManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly ImageManagementState _imageManagementState;
    private readonly PackagePalettes _palettes;

    public ImageManagementWindow(OpenedPackage package, MainWindowState state, ImageManagementState imageManagementState, PackagePalettes palettes)
    {
        _package = package;
        _state = state;
        _imageManagementState = imageManagementState;
        _palettes = palettes;
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
                var types = OpenedPackage.ImageTypeAsList();
                var origSelectedType = types.FindIndex(i => i == image.Type.ToString("G"));
                var selectedType = origSelectedType;
                ImGui.PushID($"{path}_type");
                ImGui.SetNextItemWidth(100.0f);
                ImGui.Combo("", ref selectedType, types.ToArray(), types.Count);
                ImGui.PopID();
                if (selectedType != origSelectedType)
                {
                    _package.Value.Images[path].Type = Enum.Parse<OpenedPackage.ImageType>(types[selectedType]);
                }
                ImGui.SameLine();
                
                //image index
                var indexes = Enumerable.Range(1, 99).ToList();
                var indexList = indexes.Select(i => ImageName(image.Type, i)).ToArray();
                var origIndex = image.Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(120.0f);
                ImGui.Combo("", ref index, indexList, indexList.Length);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _package.Value.Images[path].Index = index + 1;
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

    private static string ColorName(int i)
    {
        if (!ToucheTools.Constants.Palettes.SpecialColors.Contains(i))
        {
            return $"{i}";
        }

        if (ToucheTools.Constants.Palettes.TransparencyColor == i)
        {
            return "Transparent";
        }
        if (ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor == i)
        {
            return "Sprite Marker (Transparent)";
        }
        if (ToucheTools.Constants.Palettes.TransparentRoomMarkerColor == i)
        {
            return "Room Marker/UI text";
        }
        if (ToucheTools.Constants.Palettes.InventoryBackgroundColor == i)
        {
            return "Inventory/Conversation Background";
        }
        if (ToucheTools.Constants.Palettes.ConversationTextColor == i)
        {
            return "Conversation Text";
        }
        if (ToucheTools.Constants.Palettes.InventoryMoneyTextColor == i)
        {
            return "Inventory Money";
        }
        if (ToucheTools.Constants.Palettes.ActionMenuBackgroundColor == i)
        {
            return "Action Menu Background";
        }
        if (ToucheTools.Constants.Palettes.ActionMenuTextColor == i)
        {
            return "Action Menu Text";
        }

        throw new Exception($"Unknown color: {i}");
    }

    private static string ImageName(OpenedPackage.ImageType type, int i)
    {
        if (type == OpenedPackage.ImageType.Sprite)
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

        if (type == OpenedPackage.ImageType.Icon)
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