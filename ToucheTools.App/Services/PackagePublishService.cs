using ToucheTools.App.ViewModels;
using ToucheTools.Exporters;
using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class PackagePublishService
{
    private readonly OpenedPackage _package;
    private readonly PackageImages _images;
    private readonly PackagePalettes _palettes;
    private readonly PackageAnimations _animations;
    private readonly PackageRooms _rooms;

    public PackagePublishService(OpenedPackage package, PackageImages images, PackagePalettes palettes, PackageAnimations animations, PackageRooms rooms)
    {
        _package = package;
        _images = images;
        _palettes = palettes;
        _animations = animations;
        _rooms = rooms;
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
            Sequences = new Dictionary<int, SequenceDataModel>()
            {
            },
            Programs = Sample.Programs(),
        };
        var images = _package.GetIncludedImages();
        var roomImages = images.Where(p => p.Value.Type == OpenedPackage.ImageType.Room);
        var sprites = images.Where(p => p.Value.Type == OpenedPackage.ImageType.Sprite);
        var palettes = _palettes.GetPalettes();
        var animations = _package.GetIncludedAnimations();
        var rooms = _package.GetIncludedRooms();

        foreach (var (roomImagePath, roomImageData) in roomImages)
        {
            var roomImageId = roomImageData.Index;
            var palette = palettes[roomImageId];
            db.Palettes[roomImageId] = new PaletteDataModel()
            {
                Colors = palette.OrderBy(p => p.Key).Select(p => p.Value).ToList()
            };

            db.Rooms[roomImageId] = new RoomInfoDataModel()
            {
                RoomImageNum = roomImageId
            };
            var (roomWidth, roomHeight, roomBytes) = _images.GetImage(roomImagePath);
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
            db.RoomImages[roomImageId] = new Lazy<RoomImageDataModel>(roomImage);
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

        foreach (var (animationPath, animation) in animations)
        {
            var sequence = _animations.GetAnimation(animationPath);
            db.Sequences[animation.Index] = sequence;
        }
        
        //TODO: correctly build programs instead of hard-coding just one program
        var program = db.Programs[ToucheTools.Constants.Game.StartupEpisode];
        var roomInfo = rooms.First();
        var roomImageIndex = roomInfo.Value.Index;
        var roomImageModel = db.RoomImages[roomImageIndex].Value;
        
        program.Points = new List<ProgramDataModel.Point>();
        program.Walks = new List<ProgramDataModel.Walk>();
        program.Rects = new List<ProgramDataModel.Rect>();
        program.Hitboxes = new List<ProgramDataModel.Hitbox>();
        program.Strings = new Dictionary<int, string>();
        var stringCounter = 1;
        //rect showing room size
        program.Rects.Add(new ProgramDataModel.Rect()
        {
            X = 0,
            Y = 0,
            W = roomImageModel.RoomWidth,
            H = roomImageModel.Height
        });
        //point showing room anchor?
        program.Points.Add(new ProgramDataModel.Point()
        {
            X = 0,
            Y = 0,
            Z = 0
        });
        var roomPath = roomInfo.Key;
        var room = _rooms.GetRoom(roomPath);
        //TODO: correctly ensure indexes match
        foreach (var (_, (x, y, z)) in room.WalkablePoints.OrderBy(p => p.Key))
        {
            program.Points.Add(new ProgramDataModel.Point()
            {
                X = x,
                Y = y,
                Z = z
            });
        }
        //TODO: correctly ensure indexes match
        foreach (var ((p1, p2), (clipRect, area1, area2)) in room.WalkableLines)
        {
            program.Walks.Add(new ProgramDataModel.Walk()
            {
                Point1 = p1,
                Point2 = p2,
                ClipRect = clipRect,
                Area1 = area1,
                Area2 = area2
            });
        }

        foreach (var hitbox in room.Hitboxes)
        {
            var item = hitbox.Item;
            if (hitbox.Displayed)
            {
                item = item & ~0x1000;
            }

            var stringId = stringCounter;
            if (program.Strings.Any(p => p.Value == hitbox.Label))
            {
                stringId = program.Strings.First(p => p.Value == hitbox.Label).Key;
            }
            else
            {
                program.Strings[stringId] = hitbox.Label;
                stringCounter++;
            }
            
            var secStringId = stringCounter;
            if (program.Strings.Any(p => p.Value == hitbox.SecondaryLabel))
            {
                secStringId = program.Strings.First(p => p.Value == hitbox.SecondaryLabel).Key;
            }
            else
            {
                program.Strings[secStringId] = hitbox.SecondaryLabel;
                stringCounter++;
            }
            program.Hitboxes.Add(new ProgramDataModel.Hitbox()
            {
                Item = item,
                String = stringId,
                DefaultString = secStringId,
                Actions = new int[8],
                Rect1 = new ProgramDataModel.Rect()
                {
                    X = hitbox.X,
                    Y = hitbox.Y,
                    W = hitbox.W,
                    H = hitbox.H
                },
                Rect2 = new ProgramDataModel.Rect()
            });
        }

        var memoryStream = new MemoryStream();
        var exporter = new MainExporter(memoryStream);
        exporter.Export(db);
        var path = "../../../../sample/TOUCHE_PACKAGE.DAT"; //TODO: different path
        File.WriteAllBytes(path, memoryStream.ToArray());
    }
}