using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.Exporters;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class PackageWindow
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

    #region Publishing
    private void Publish()
    {
        var db = new DatabaseModel()
        {
            Text = Sample.Text(),
            Backdrop = new BackdropDataModel()
            {
                Width = ToucheTools.Constants.Resources.BackdropWidth,
                Height = ToucheTools.Constants.Resources.BackdropHeight
            },
            Icons = Sample.Icons(),
            Sprites = Sample.Sprites(),
            Palettes = new Dictionary<int, PaletteDataModel>()
            {
            },
            Rooms = new Dictionary<int, RoomInfoDataModel>()
            {
            },
            RoomImages = new Dictionary<int, Lazy<RoomImageDataModel>>()
            {
            },
            Sequences = Sample.Sequences(),
            Programs = Sample.Programs(),
        };

        var files = _viewModel.GetImages();
        Dictionary<int, PaletteDataModel.Rgb>? anyPalette = null;
        for (var i = 0; i < files.Length; i++)
        {
            var type = _viewModel.GetFileType(i);
            var index = _viewModel.GetFileIndex(i) + 1;
            if (type == PackageViewModel.FileType.Room)
            {
                var palette = _paletteViewModel.GetPalette(i);
                anyPalette = palette;
                var colors = new List<PaletteDataModel.Rgb>(256);
                for (var c = 0; c < 256; c++)
                {
                    if (palette.ContainsKey(c))
                    {
                        colors.Add(palette[c]);
                    }
                    else
                    {
                        colors.Add(new PaletteDataModel.Rgb()
                        {
                            R = 255,
                            G = 0,
                            B = 255
                        });
                    }
                }

                db.Palettes[index] = new PaletteDataModel()
                {
                    Colors = colors
                };

                db.Rooms[index] = new RoomInfoDataModel()
                {
                    RoomImageNum = index
                };

                var (roomWidth, roomHeight, roomBytes) = _viewModel.GetImage(i);
                var roomImage = new RoomImageDataModel()
                {
                    Width = roomWidth,
                    Height = roomHeight,
                    RoomWidth = roomWidth,
                    RawData = new byte[roomHeight, roomWidth]
                };
                for (var y = 0; y < roomHeight; y++)
                {
                    for (var x = 0; x < roomWidth; x++)
                    {
                        var r = roomBytes[(y * roomWidth + x) * 4 + 0];
                        var g = roomBytes[(y * roomWidth + x) * 4 + 1];
                        var b = roomBytes[(y * roomWidth + x) * 4 + 2];
                        var a = roomBytes[(y * roomWidth + x) * 4 + 3];
                        if (r == 255 && g == 0 && b == 255 && a == 255 && y == 0)
                        {
                            //it's the room width marker
                            roomImage.RoomWidth = x;
                            roomImage.RawData[y, x] = ToucheTools.Constants.Palettes.TransparentRoomMarkerColor;
                            continue;
                        }

                        if (a < 255)
                        {
                            roomImage.RawData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                            continue;
                        }

                        var roomCol = palette.First(p => p.Key < ToucheTools.Constants.Palettes.StartOfSpriteColors &&
                                                         p.Value.R == r && p.Value.G == g && p.Value.B == b).Key;
                        roomImage.RawData[y, x] = (byte)roomCol;
                    }
                }

                db.RoomImages[index] = new Lazy<RoomImageDataModel>(roomImage);
            }
        }

        if (anyPalette == null)
        {
            throw new Exception("No palette found");
        }
        
        for (var i = 0; i < files.Length; i++)
        {
            var type = _viewModel.GetFileType(i);
            var index = _viewModel.GetFileIndex(i) + 1;
            if (type == PackageViewModel.FileType.Sprite)
            {
                var (spriteWidth, spriteHeight, spriteBytes) = _viewModel.GetImage(i);
                var sprite = new SpriteImageDataModel()
                {
                    Width = (short)spriteWidth,
                    Height = (short)spriteHeight,
                    SpriteWidth = (short)spriteWidth,
                    SpriteHeight = (short)spriteHeight,
                    RawData = new byte[spriteHeight, spriteWidth],
                    DecodedData = new byte[spriteHeight, spriteWidth],
                };
                var foundWidth = false;
                var foundHeight = false;
                for (var y = 0; y < spriteHeight; y++)
                {
                    for (var x = 0; x < spriteWidth; x++)
                    {
                        var r = spriteBytes[(y * spriteWidth + x) * 4 + 0];
                        var g = spriteBytes[(y * spriteWidth + x) * 4 + 1];
                        var b = spriteBytes[(y * spriteWidth + x) * 4 + 2];
                        var a = spriteBytes[(y * spriteWidth + x) * 4 + 3];
                        if (r == 255 && g == 0 && b == 255 && a == 255 && y == 0 && !foundWidth)
                        {
                            //it's the sprite width marker
                            foundWidth = true;
                            sprite.SpriteWidth = (short)x;
                            sprite.RawData[y, x] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor;
                            sprite.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor; 
                            continue;
                        }
                        if (r == 255 && g == 0 && b == 255 && a == 255 && x == 0 && !foundHeight)
                        {
                            //it's the sprite height marker
                            foundHeight = true;
                            sprite.SpriteHeight = (short)y;
                            sprite.RawData[y, x] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor;
                            sprite.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor;
                            continue;
                        }

                        if (r == 255 && g == 0 && b == 255 && a == 255)
                        {
                            sprite.RawData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                            sprite.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                            continue;
                        }

                        if (a < 255)
                        {
                            sprite.RawData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                            sprite.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                            continue;
                        }

                        var spriteCol = anyPalette.First(p => p.Key >= ToucheTools.Constants.Palettes.StartOfSpriteColors &&
                                                         p.Value.R == r && p.Value.G == g && p.Value.B == b).Key - ToucheTools.Constants.Palettes.StartOfSpriteColors + 1;
                        sprite.RawData[y, x] = (byte)spriteCol;
                        sprite.DecodedData[y, x] = (byte)(spriteCol + ToucheTools.Constants.Palettes.StartOfSpriteColors - 1);
                    }
                }

                db.Sprites[index] = new Lazy<SpriteImageDataModel>(sprite);
            }
        }

        var memoryStream = new MemoryStream();
        var exporter = new MainExporter(memoryStream);
        exporter.Export(db);
        File.WriteAllBytes("../../../../sample/TOUCHE_PACKAGE.DAT", memoryStream.ToArray());
    }
    #endregion

    public void Render()
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

            if (ImGui.Button("Publish"))
            {
                Publish();
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

                    // foreach (var (colId, col) in _paletteViewModel.GetPalette(i))
                    // {
                    //     ImGui.Text($"{colId} - {col.R}, {col.G}, {col.B}");
                    // }
                    
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