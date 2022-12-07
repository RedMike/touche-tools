using ToucheTools.App.ViewModels;
using ToucheTools.Exporters;
using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class PackagePublishService
{
    private readonly OpenedPackage _package;
    private readonly PackageImages _images;
    private readonly PackagePalettes _palettes;

    public PackagePublishService(OpenedPackage package, PackageImages images, PackagePalettes palettes)
    {
        _package = package;
        _images = images;
        _palettes = palettes;
    }

    public void Publish()
    {
        if (!_package.IsLoaded())
        {
            throw new Exception("Package not loaded");
        }
        
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
        var images = _package.GetIncludedImages();
        var rooms = images.Where(p => p.Value.Type == OpenedPackage.ImageType.Room);
        var sprites = images.Where(p => p.Value.Type == OpenedPackage.ImageType.Sprite);
        var palettes = _palettes.GetPalettes();
        
        
        foreach (var (roomPath, roomImageData) in rooms)
        {
            var roomId = roomImageData.Index;
            var palette = palettes[roomId];
            db.Palettes[roomId] = new PaletteDataModel()
            {
                Colors = palette.OrderBy(p => p.Key).Select(p => p.Value).ToList()
            };

            db.Rooms[roomId] = new RoomInfoDataModel()
            {
                RoomImageNum = roomId
            };
            var (roomWidth, roomHeight, roomBytes) = _images.GetImage(roomPath);
            var roomImage = new RoomImageDataModel()
            {
                Width = roomWidth,
                Height = roomHeight,
                RoomWidth = -1,
                RawData = new byte[roomHeight, roomWidth]
            };
            for (var x = 0; x < roomWidth; x++)
            {
                for (var y = 0; y < roomHeight; y++)
                {
                    var r = roomBytes[(y * roomWidth + x) * 4 + 0];
                    var g = roomBytes[(y * roomWidth + x) * 4 + 1];
                    var b = roomBytes[(y * roomWidth + x) * 4 + 2];
                    var a = roomBytes[(y * roomWidth + x) * 4 + 3];

                    if (r == 255 && g == 0 && b == 255 && a == 255 && y == 0 && roomImage.RoomWidth < 0)
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

                    var col = palette.First(p => p.Key < ToucheTools.Constants.Palettes.StartOfSpriteColors &&
                                                 p.Value.R == r && p.Value.G == g && p.Value.B == b)
                        .Key;
                    roomImage.RawData[y, x] = (byte)col;
                }
            }
            db.RoomImages[roomId] = new Lazy<RoomImageDataModel>(roomImage);
        }

        foreach (var (spritePath, spriteImageData) in sprites)
        {
            var spriteId = spriteImageData.Index;
            var palette = palettes.First().Value; //TODO: this may need a different selection via mapping

            var (spriteWidth, spriteHeight, spriteBytes) = _images.GetImage(spritePath);
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

                    var spriteCol = palette.First(p => p.Key >= ToucheTools.Constants.Palettes.StartOfSpriteColors &&
                                                     p.Value.R == r && p.Value.G == g && p.Value.B == b).Key - ToucheTools.Constants.Palettes.StartOfSpriteColors + 1;
                    sprite.RawData[y, x] = (byte)spriteCol;
                    sprite.DecodedData[y, x] = (byte)(spriteCol + ToucheTools.Constants.Palettes.StartOfSpriteColors - 1);
                }
            }

            db.Sprites[spriteId] = new Lazy<SpriteImageDataModel>(sprite);
        }
        
        var memoryStream = new MemoryStream();
        var exporter = new MainExporter(memoryStream);
        exporter.Export(db);
        var path = "../../../../sample/TOUCHE_PACKAGE.DAT"; //TODO: different path
        File.WriteAllBytes(path, memoryStream.ToArray());
    }
}