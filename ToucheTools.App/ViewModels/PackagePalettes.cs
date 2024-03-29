﻿using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class PackagePalettes
{
    private readonly OpenedManifest _manifest;
    private readonly PackageImages _images;

    private Dictionary<int, Dictionary<int, PaletteDataModel.Rgb>> _palettesByRoom = null!;

    public PackagePalettes(OpenedManifest manifest, PackageImages images)
    {
        _manifest = manifest;
        _images = images;

        _manifest.Observe(Update);
        Update();
    }

    public Dictionary<int, Dictionary<int, PaletteDataModel.Rgb>> GetPalettes()
    {
        return _palettesByRoom;
    }

    private void Update()
    {
        _palettesByRoom = new Dictionary<int, Dictionary<int, PaletteDataModel.Rgb>>();
        if (!_manifest.IsLoaded())
        {
            return;
        }
        
        var images = _manifest.GetIncludedImages();
        var rooms = images.Where(i => i.Value.Type == OpenedManifest.ImageType.Room).ToList();
        var sprites = images.Where(i => 
            i.Value.Type == OpenedManifest.ImageType.Sprite ||
            i.Value.Type == OpenedManifest.ImageType.Icon //icons also use the same colour range as sprites/UI
        ).ToList();
        foreach (var (path, imageData) in rooms)
        {
            var roomId = imageData.Index;
            var (width, height, bytes) = _images.GetImage(path);
            var palette = NewPalette();

            var roomUniqueColours = GetUniqueColoursFromImage(width, height, bytes);
            if (roomUniqueColours.Count > ToucheTools.Constants.Palettes.RoomColorCount)
            {
                throw new Exception($"Too many colours in room: {roomId} {path}");
            }

            var colIdx = 0;
            for (var i = 0; i < ToucheTools.Constants.Palettes.StartOfSpriteColors; i++)
            {
                if (palette.ContainsKey(i))
                {
                    continue;
                }

                if (colIdx >= roomUniqueColours.Count)
                {
                    //we've exhausted the room colours, so fill out with nothing
                    palette[i] = new PaletteDataModel.Rgb()
                    {
                        R = 100,
                        G = 0,
                        B = 200,
                    };
                }
                else
                {
                    palette[i] = roomUniqueColours[colIdx];
                    colIdx++;
                }
            }

            var customSpriteColours = _manifest.GetGame().CustomColors
                .Where(pair => pair.Key >= ToucheTools.Constants.Palettes.StartOfSpriteColors)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            
            //TODO: build mapping of which sprites are expected to be in which rooms to filter down
            var spriteCols = new List<PaletteDataModel.Rgb>();
            foreach (var (spritePath, _) in sprites)
            {
                var (spriteWidth, spriteHeight, spriteBytes) = _images.GetImage(spritePath);
                var spriteUniqueColours = GetUniqueColoursFromImage(spriteWidth, spriteHeight, spriteBytes);
                foreach (var spriteCol in spriteUniqueColours)
                {
                    if (spriteCols.Any(c => c.R == spriteCol.R && c.G == spriteCol.G && c.B == spriteCol.B))
                    {
                        continue;
                    }

                    if (customSpriteColours.ContainsValue((spriteCol.R, spriteCol.G, spriteCol.B)))
                    {
                        //custom colours were added separately
                        continue;
                    }

                    spriteCols.Add(spriteCol);
                }
                if (spriteCols.Count > ToucheTools.Constants.Palettes.SpriteColorCount - customSpriteColours.Count)
                {
                    throw new Exception($"Too many colours in room/sprite: {roomId} {path} {spritePath}");
                }
            }

            colIdx = 0;
            for (var i = ToucheTools.Constants.Palettes.StartOfSpriteColors; i <= 255; i++)
            {
                if (palette.ContainsKey(i))
                {
                    continue;
                }

                if (colIdx >= spriteCols.Count)
                {
                    //we've exhausted the sprite colours, so fill out with nothing
                    
                    palette[i] = new PaletteDataModel.Rgb()
                    {
                        R = (byte)(i-100),
                        G = 0,
                        B = (byte)(i-100),
                    };
                }
                else
                {
                    palette[i] = spriteCols[colIdx];
                    colIdx++;
                }
            }

            _palettesByRoom[roomId] = palette;
        }
    }

    private Dictionary<int, PaletteDataModel.Rgb> NewPalette()
    {
        var game = _manifest.GetGame();
        var palette = new Dictionary<int, PaletteDataModel.Rgb>();
        //add custom colours from the start
        foreach (var (colId, (r, g, b)) in game.CustomColors)
        {
            palette[colId] = new PaletteDataModel.Rgb()
            {
                R = r,
                G = g,
                B = b
            };
        }
        
        //TODO: bg color should come from image
        palette[ToucheTools.Constants.Palettes.ActionMenuBackgroundColor] = new PaletteDataModel.Rgb()
        {
            R = 92,
            G = 100,
            B = 188
        };
        //TODO: bg color should come from image
        palette[ToucheTools.Constants.Palettes.InventoryBackgroundColor] = new PaletteDataModel.Rgb()
        {
            R = 15,
            G = 15,
            B = 15
        };
        return palette;
    }
    
    private static List<PaletteDataModel.Rgb> GetUniqueColoursFromImage(int width, int height, byte[] bytes)
    {
        var uniqueColours = new List<PaletteDataModel.Rgb>();
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var r = bytes[(i * height + j) * 4 + 0];
                var g = bytes[(i * height + j) * 4 + 1];
                var b = bytes[(i * height + j) * 4 + 2];
                var a = bytes[(i * height + j) * 4 + 3];
                if (a == 0)
                {
                    continue;
                }

                if (r == 255 && g == 0 && b == 255 && a == 255)
                {
                    //it's the sprite/room transparency colour
                    continue;
                }

                if (uniqueColours.Any(col => col.R == r && col.G == g && col.B == b))
                {
                    continue;
                }
                var col = new PaletteDataModel.Rgb()
                {
                    R = r,
                    G = g,
                    B = b
                };
                uniqueColours.Add(col);
            }
        }

        return uniqueColours;
    }
}