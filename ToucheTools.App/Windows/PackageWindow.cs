using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class PackageWindow : BaseWindow
{
    private readonly PackageViewModel _viewModel;
    private readonly RenderWindow _render;
    private readonly PackagePaletteViewModel _paletteViewModel;

    private readonly Dictionary<string, (IntPtr, int, int)> _textures = new Dictionary<string, (IntPtr, int, int)>();

    public PackageWindow(PackageViewModel viewModel, RenderWindow render, PackagePaletteViewModel paletteViewModel)
    {
        _viewModel = viewModel;
        _render = render;
        _paletteViewModel = paletteViewModel;
    }

    public override void Render()
    {
        {
            ImGui.Begin("Loaded Folder");
            var files = _viewModel.GetImages();
            for (var i = 0; i < files.Length; i++)
            {
                var origInclude = _viewModel.IsFileIncluded(i);
                var include = origInclude;

                var isSelected = _viewModel.GetSelectedIndex() == i; 
                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.4f, 0.3f, 0.8f, 1.0f));
                }
                if (ImGui.Button(files[i]))
                {
                    _viewModel.SetSelectedIndex(i);
                }
                if (isSelected)
                {
                    ImGui.PopStyleColor();
                }

                ImGui.SameLine();

                ImGui.PushID($"Include {i}");
                ImGui.Checkbox("", ref include);
                ImGui.PopID();
                if (include != origInclude)
                {
                    _viewModel.SetFileIncluded(i, include);
                }

                ImGui.SameLine();
                var types = Enum.GetValues<PackageViewModel.FileType>().ToList();
                var origFileTypeId = types.FindIndex(s => s == _viewModel.GetFileType(i));
                var fileTypeId = origFileTypeId;
                ImGui.PushID($"Type {i}");
                ImGui.SetNextItemWidth(100.0f);
                ImGui.Combo("", ref fileTypeId, types
                    .OrderBy(t => t)
                    .Select(t => t.ToString("G"))
                    .ToArray(), types.Count);
                ImGui.PopID();
                if (fileTypeId != origFileTypeId)
                {
                    _viewModel.SetFileType(i, types[fileTypeId]);
                }
                
                ImGui.SameLine();

                var indexes = Enumerable.Range(0, 100).ToArray();
                var origIndex = _viewModel.GetFileIndex(i);
                var index = origIndex;
                ImGui.PushID($"Index {i}");
                ImGui.SetNextItemWidth(50.0f);
                ImGui.Combo("", ref index, indexes.Select(q => $"{q}").ToArray(), indexes.Length);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _viewModel.SetFileIndex(i, index);
                }
            }

            ImGui.End();
        }

        {
            ImGui.Begin("Image Preview", ImGuiWindowFlags.HorizontalScrollbar);
            var (texture, width, height) = GetImage(_viewModel.GetSelectedIndex());

            var background = _render.RenderCheckerboardRectangle(10, width, height,
                (75, 75, 75, 255), (50, 50, 50, 255)
            );
            ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
            ImGui.Image(background, new Vector2(width, height));
            ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
            ImGui.Image(texture, new Vector2(width, height));
            ImGui.End();
        }

        {
            ImGui.Begin("Palette Preview");

            var images = _viewModel.GetImages();
            for (var i = 0; i < images.Length; i++)
            {
                if (!_viewModel.IsFileIncluded(i) || _viewModel.GetFileType(i) != PackageViewModel.FileType.Room)
                {
                    continue;
                }

                if (ImGui.TreeNodeEx($"Room {images[i]}"))
                {
                    var cursorPos = ImGui.GetCursorPos();
                    
                    var (texture, width, height) = GetPaletteImage(i);
                    var background = _render.RenderCheckerboardRectangle(10, width, height,
                        (75, 75, 75, 255), (50, 50, 50, 255)
                    );
                    //draw checkered background
                    ImGui.SetCursorPos(cursorPos);
                    ImGui.Image(background, new Vector2(width, height));
                    
                    //draw actual palette image
                    ImGui.SetCursorPos(cursorPos);
                    ImGui.Image(texture, new Vector2(width, height));
                    
                    //draw rectangle around room colours
                    var (roomRect, roomRectWidth, roomRectHeight) = GetPaletteRoomRect();
                    ImGui.SetCursorPos(cursorPos);
                    var roomRectSize = new Vector2(roomRectWidth, roomRectHeight);
                    ImGui.Image(roomRect, roomRectSize);
                    //draw text for room rectangle
                    var roomRectText = "Room colours (1-191)";
                    var roomRectTextSize = ImGui.CalcTextSize(roomRectText);
                    ImGui.SetCursorPos(cursorPos + roomRectSize - roomRectTextSize);
                    ImGui.Text(roomRectText);
                    
                    //draw rectangle around sprite colours
                    var (spriteRect, spriteRectWidth, spriteRectHeight) = GetPaletteSpriteRect();
                    var spriteRectOffset = new Vector2(PaletteRoomColourWidth, 0.0f);
                    ImGui.SetCursorPos(cursorPos + spriteRectOffset);
                    var spriteRectSize = new Vector2(spriteRectWidth, spriteRectHeight);
                    ImGui.Image(spriteRect, new Vector2(spriteRectWidth, spriteRectHeight));
                    //draw text for sprite rectangle
                    var spriteRectText = "Sprite colours (63-254)";
                    var spriteRectTextSize = ImGui.CalcTextSize(spriteRectText);
                    ImGui.SetCursorPos(cursorPos + spriteRectOffset + spriteRectSize - spriteRectTextSize);
                    ImGui.Text(spriteRectText);
                    
                    ImGui.TreePop();
                }
            }

            ImGui.End();
        }
    }

    private const int PaletteTileSize = 25;
    private const int PaletteTilesPerColumn = 8;
    private const int PaletteImageWidth = (255 / PaletteTilesPerColumn + 1) * PaletteTileSize;
    private const int PaletteImageHeight = PaletteTilesPerColumn * PaletteTileSize;

    private const int PaletteRoomColumnCount = (ToucheTools.Constants.Palettes.StartOfSpriteColors - 1) / PaletteTilesPerColumn;
    private const int PaletteRoomColourWidth = PaletteRoomColumnCount * PaletteTileSize;
    
    private const int PaletteSpriteColumnCount = (ToucheTools.Constants.Palettes.SpriteColorCount + 2) / PaletteTilesPerColumn;
    private const int PaletteSpriteColourWidth = PaletteSpriteColumnCount * PaletteTileSize;
    private (IntPtr, int, int) GetPaletteRoomRect()
    {
        var id = $"palette_room";
        if (_textures.ContainsKey(id))
        {
            return _textures[id];
        }

        var width = PaletteRoomColourWidth;
        var height = PaletteTilesPerColumn * PaletteTileSize + 50;
        var img = _render.RenderRectangle(1,
            width, height,
            (0, 0, 0, 0), (255, 0, 0, 255)
        );
        
        _textures[id] = (img, width, height);
        return (img, width, height);
    }
    
    private (IntPtr, int, int) GetPaletteSpriteRect()
    {
        var id = $"palette_sprite";
        if (_textures.ContainsKey(id))
        {
            return _textures[id];
        }

        var width = PaletteSpriteColourWidth;
        var height = PaletteTilesPerColumn * PaletteTileSize + 50;
        var img = _render.RenderRectangle(1,
            width, height,
            (0, 0, 0, 0), (0, 0, 255, 255)
        );
        
        _textures[id] = (img, width, height);
        return (img, width, height);
    }

    private (IntPtr, int, int) GetPaletteImage(int roomId)
    {
        var id = $"room_{roomId}";
        if (_textures.ContainsKey(id))
        {
            return _textures[id];
        }
        
        var palette = _paletteViewModel.GetPalette(roomId);
        var bytes = new byte[PaletteImageWidth * PaletteImageHeight * 4];
        for (var i = 0; i < 256; i++)
        {
            var x = i % PaletteTilesPerColumn;
            var y = i / PaletteTilesPerColumn;
            var col = new PaletteDataModel.Rgb()
            {
                R = 255,
                G = 0,
                B = 255
            };
            var missingCol = true;
            if (palette.ContainsKey(i))
            {
                col = palette[i];
                missingCol = false;
            }

            for (var q = 0; q < PaletteTileSize; q++)
            {
                for (var w = 0; w < PaletteTileSize; w++)
                {
                    bytes[((x * PaletteTileSize + q) * PaletteImageWidth + (y * PaletteTileSize + w)) * 4 + 0] =
                        col.R;
                    bytes[((x * PaletteTileSize + q) * PaletteImageWidth + (y * PaletteTileSize + w)) * 4 + 1] =
                        col.G;
                    bytes[((x * PaletteTileSize + q) * PaletteImageWidth + (y * PaletteTileSize + w)) * 4 + 2] =
                        col.B;
                    bytes[((x * PaletteTileSize + q) * PaletteImageWidth + (y * PaletteTileSize + w)) * 4 + 3] =
                        (byte)(missingCol ? 0 : 255);
                }
            }
        }
        
        var img = _render.RenderImage(RenderWindow.RenderType.Primitive, id, PaletteImageWidth, PaletteImageHeight, bytes);
        _textures[id] = (img, PaletteImageWidth, PaletteImageHeight);
        return (img, PaletteImageWidth, PaletteImageHeight);
    }

    private (IntPtr, int, int) GetImage(int index)
    {
        var id = $"img_{index}";
        if (_textures.ContainsKey(id))
        {
            return _textures[id];
        }

        var (width, height, bytes) = _viewModel.GetImage(index);
        var img = _render.RenderImage(RenderWindow.RenderType.Primitive, id, width, height, bytes);
        _textures[id] = (img, width, height);
        return (img, width, height);
    }
}