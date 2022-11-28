using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class PackagePaletteViewModel
{
    private readonly PackageViewModel _package;
    private List<PaletteDataModel.Rgb> _spritePalette = new List<PaletteDataModel.Rgb>();
    private Dictionary<int, List<PaletteDataModel.Rgb>> _roomPalettes = new Dictionary<int, List<PaletteDataModel.Rgb>>();
    
    public PackagePaletteViewModel(PackageViewModel package)
    {
        _package = package;
        _package.RegisterForUpdate(Update);

        Update();
    }

    public Dictionary<int, PaletteDataModel.Rgb> GetPalette(int roomId)
    {
        var palette = new Dictionary<int, PaletteDataModel.Rgb>();
        palette[ToucheTools.Constants.Palettes.TransparencyColor] = new PaletteDataModel.Rgb()
        {
            R = 0,
            G = 0,
            B = 0
        };
        palette[ToucheTools.Constants.Palettes.TransparentRoomMarkerColor] = new PaletteDataModel.Rgb()
        {
            R = 100,
            G = 0,
            B = 100
        };
        palette[ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor] = new PaletteDataModel.Rgb()
        {
            R = 250,
            G = 0,
            B = 250
        };
        for (var i = 0; i < _spritePalette.Count; i++)
        {
            palette[ToucheTools.Constants.Palettes.StartOfSpriteColors + i] = _spritePalette[i];
        }

        if (!_roomPalettes.ContainsKey(roomId))
        {
            throw new Exception($"No palette for room: {roomId}");
        }

        var roomPalette = _roomPalettes[roomId];
        var colIdx = 0;
        for (var i = 0; i < roomPalette.Count; i++)
        {
            if (colIdx == ToucheTools.Constants.Palettes.TransparencyColor)
            {
                colIdx++;
            }
            if (colIdx == ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor)
            {
                colIdx++;
            }

            palette[colIdx] = roomPalette[i];
            colIdx++;
        }

        return palette;
    }

    private void Update()
    {
        _spritePalette = new List<PaletteDataModel.Rgb>();
        _roomPalettes = new Dictionary<int, List<PaletteDataModel.Rgb>>();
        
        var images = _package.GetImages();
        for (var i = 0; i < images.Length; i++)
        {
            var (width, height, bytes) = _package.GetImage(i);
            var type = _package.GetFileType(i);
            if (type == PackageViewModel.FileType.Sprite)
            {
                var uniqueColours = GetUniqueColoursFromImage(width, height, bytes);
                _spritePalette = _spritePalette
                    .Concat(uniqueColours)
                    .DistinctBy(c => $"{c.R}_{c.G}_{c.B}")
                    .OrderBy(c => c.R)
                    .ThenBy(c => c.G)
                    .ThenBy(c => c.B)
                    .ToList();
            } else if (type == PackageViewModel.FileType.Room)
            {
                var uniqueColours = GetUniqueColoursFromImage(width, height, bytes);
                _roomPalettes[i] = uniqueColours
                    .DistinctBy(c => $"{c.R}_{c.G}_{c.B}")
                    .OrderBy(c => c.R)
                    .ThenBy(c => c.G)
                    .ThenBy(c => c.B)
                    .ToList();
            }
        }

        if (_spritePalette.Count > ToucheTools.Constants.Palettes.SpriteColorCount)
        {
            throw new Exception("Too many unique sprite colours: " + _spritePalette.Count);
        }

        foreach (var (roomId, roomPalette) in _roomPalettes)
        {
            if (roomPalette.Count > ToucheTools.Constants.Palettes.RoomColorCount)
            {
                throw new Exception($"Too many unique room colours in room {roomId}: {roomPalette.Count}");
            }
        }
    }

    private List<PaletteDataModel.Rgb> GetUniqueColoursFromImage(int width, int height, byte[] bytes)
    {
        var uniqueColours = new List<PaletteDataModel.Rgb>();
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var r = bytes[(j * height + i) * 4 + 0];
                var g = bytes[(j * height + i) * 4 + 1];
                var b = bytes[(j * height + i) * 4 + 2];
                var a = bytes[(j * height + i) * 4 + 3];
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