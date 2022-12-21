using Newtonsoft.Json;
using ToucheTools.App.Models;
using ToucheTools.App.ViewModels;
using ToucheTools.Exporters;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.Services;

public class PackagePublishService
{
    private readonly OpenedPackage _package;
    private readonly PackageImages _images;
    private readonly PackagePalettes _palettes;
    private readonly PackageAnimations _animations;
    private readonly PackageRooms _rooms;
    private readonly PackagePrograms _programs;

    public PackagePublishService(OpenedPackage package, PackageImages images, PackagePalettes palettes, PackageAnimations animations, PackageRooms rooms, PackagePrograms programs)
    {
        _package = package;
        _images = images;
        _palettes = palettes;
        _animations = animations;
        _rooms = rooms;
        _programs = programs;
    }

    public void Publish(string datPath)
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
        var icons = images.Where(p => p.Value.Type == OpenedPackage.ImageType.Icon);
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
        
        foreach (var (iconPath, iconImageData) in icons)
        {
            var iconId = iconImageData.Index;
            var palette = palettes.First().Value; //TODO: this may need a different selection via mapping

            var (iconWidth, iconHeight, iconBytes) = _images.GetImage(iconPath);
            var icon = new IconImageDataModel()
            {
                Width = (short)iconWidth,
                Height = (short)iconHeight,
                RawData = new byte[iconHeight, iconWidth],
                DecodedData = new byte[iconHeight, iconWidth],
            };
            
            for (var y = 0; y < iconHeight; y++)
            {
                for (var x = 0; x < iconWidth; x++)
                {
                    var r = iconBytes[(y * iconWidth + x) * 4 + 0];
                    var g = iconBytes[(y * iconWidth + x) * 4 + 1];
                    var b = iconBytes[(y * iconWidth + x) * 4 + 2];
                    var a = iconBytes[(y * iconWidth + x) * 4 + 3];

                    if (r == 255 && g == 0 && b == 255 && a == 255)
                    {
                        icon.RawData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                        icon.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                        continue;
                    }

                    if (a < 255)
                    {
                        icon.RawData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                        icon.DecodedData[y, x] = ToucheTools.Constants.Palettes.TransparencyColor;
                        continue;
                    }

                    var iconCol = palette.First(p => p.Key >= ToucheTools.Constants.Palettes.StartOfSpriteColors &&
                                                     p.Value.R == r && p.Value.G == g && p.Value.B == b).Key - ToucheTools.Constants.Palettes.StartOfSpriteColors + 1;
                    icon.RawData[y, x] = (byte)iconCol;
                    icon.DecodedData[y, x] = (byte)(iconCol + ToucheTools.Constants.Palettes.StartOfSpriteColors - 1);
                }
            }

            db.Icons[iconId] = new Lazy<IconImageDataModel>(icon);
        }

        foreach (var (animationPath, animation) in animations)
        {
            var sequence = _animations.GetAnimation(animationPath);
            db.Sequences[animation.Index] = sequence;
        }
        
        //room-independent strings and actions
        var game = _package.GetGame();
        var globalActionIdMapping = new Dictionary<int, int>();
        foreach (var (actionId, actionLabel) in game.ActionDefinitions)
        {
            //unlike room actions, global actions are handled per ID with gaps
            if (db.Text.Strings.ContainsKey(actionId))
            {
                throw new Exception("Duplicate global action");
            }

            db.Text.Strings[actionId] = actionLabel;
            globalActionIdMapping[actionId] = -actionId;
        }

        var programs = _package.GetIncludedPrograms();
        foreach (var (programId, programData) in _programs.GetIncludedPrograms())
        {
            var actionIdMapping = new Dictionary<int, int>(globalActionIdMapping); //new set of actions per program
            var hitboxIdMapping = new Dictionary<(int, int), int>();
            var program = new ProgramDataModel();
            db.Programs[programId] = program;

            var referencedRooms = programData.Values
                .Where(i => i is LoadRoomInstruction)
                .Select(i => ((LoadRoomInstruction)i).Num)
                .Distinct().ToList();
            
            var pointIds = new Dictionary<(int, int), int>();
            var pointIdCounter = 1; //skip room anchor
            var textIds = new Dictionary<(int, int), int>();
            var textCounter = 1;

            var addedAnchorRoomPoints = false;
            foreach (var roomId in referencedRooms)
            {
                var roomImageModel = db.RoomImages[roomId].Value;

                if (!addedAnchorRoomPoints)
                {
                    //TODO: do this for a specific room instead of the first one
                    addedAnchorRoomPoints = true;
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
                }

                var roomPath = rooms.First(r => r.Value.Index == roomId).Key;
                var room = _rooms.GetRoom(roomPath);
                
                foreach (var (id, (x, y, z)) in room.WalkablePoints.OrderBy(p => p.Key))
                {
                    pointIds[(roomId, id)] = pointIdCounter;
                    pointIdCounter++;
                    program.Points.Add(new ProgramDataModel.Point()
                    {
                        X = x,
                        Y = y,
                        Z = z
                    });
                }

                foreach (var ((p1, p2), (clipRect, area1, area2)) in room.WalkableLines)
                {
                    //get the indexes from the global mapping
                    var p1Id = pointIds[(roomId, p1)];
                    var p2Id = pointIds[(roomId, p2)];
                    program.Walks.Add(new ProgramDataModel.Walk()
                    {
                        Point1 = p1Id,
                        Point2 = p2Id,
                        ClipRect = clipRect,
                        Area1 = area1,
                        Area2 = area2
                    });
                }

                foreach (var (bgAreaId, bgArea) in room.BackgroundAreas)
                {
                    if (bgArea.Dynamic)
                    {
                        //it's an area
                        var destX = bgArea.DestX ?? 0x4000;
                        var destY = bgArea.DestY ?? 0x4000;
                        //offset/scaling is ignored
                        program.Areas.Add(new ProgramDataModel.Area()
                        {
                            Id = bgAreaId, //TODO: mapping?
                            SrcX = bgArea.SourceX,
                            SrcY = bgArea.SourceY,
                            Rect = new ProgramDataModel.Rect()
                            {
                                X = destX,
                                Y = destY,
                                W = bgArea.Width,
                                H = bgArea.Height
                            },
                            InitialState = ProgramDataModel.AreaState.Static,
                            AnimationCount = 0, //TODO: animations
                            AnimationNext = 0, //TODO: animations
                        });
                    }
                    else
                    {
                        //it's a background
                        var destX = bgArea.DestX ?? 0x4000;
                        var destY = bgArea.DestY ?? 0x4000;
                        var type = bgArea.ScaledOffset != null ? 4 : 1;
                        program.Backgrounds.Add(new ProgramDataModel.Background()
                        {
                            SrcX = bgArea.SourceX,
                            SrcY = bgArea.SourceY,
                            Type = type,
                            Rect = new ProgramDataModel.Rect()
                            {
                                X = destX,
                                Y = destY,
                                W = bgArea.Width,
                                H = bgArea.Height
                            },
                            Offset = bgArea.ScaledOffset ?? 0,
                            ScaleDiv = bgArea.ScaleDiv,
                            ScaleMul = bgArea.ScaleMul
                        });
                    }
                }

                foreach (var (hitboxId, hitbox) in room.Hitboxes)
                {
                    var item = hitbox.Item;
                    if (hitbox.Type == HitboxModel.HitboxType.Unknown)
                    {
                        //TODO: warning
                        continue;
                    }

                    if (hitbox.Type == HitboxModel.HitboxType.Inventory)
                    {
                        item = item | 0x1000;
                    }

                    if (hitbox.Type == HitboxModel.HitboxType.Disabled)
                    {
                        item = item | 0x2000;
                    }

                    if (hitbox.Type == HitboxModel.HitboxType.KeyChar)
                    {
                        item = item | 0x4000;
                    }

                    var stringId = textCounter;
                    if (program.Strings.Any(p => p.Value == hitbox.Label))
                    {
                        stringId = program.Strings.First(p => p.Value == hitbox.Label).Key;
                    }
                    else
                    {
                        program.Strings[stringId] = hitbox.Label;
                        textCounter++;
                    }

                    var secStringId = textCounter;
                    if (program.Strings.Any(p => p.Value == hitbox.SecondaryLabel))
                    {
                        secStringId = program.Strings.First(p => p.Value == hitbox.SecondaryLabel).Key;
                    }
                    else
                    {
                        program.Strings[secStringId] = hitbox.SecondaryLabel;
                        textCounter++;
                    }

                    var hitboxActions = hitbox.Actions
                        .Select(a => actionIdMapping.ContainsKey(a) ? actionIdMapping[a] : 0).ToArray();
                    var fallbackAction = hitbox.FallbackAction == 0 ? 0 : actionIdMapping[hitbox.FallbackAction];
                    hitboxIdMapping[(roomId, hitboxId)] = item;
                    program.Hitboxes.Add(new ProgramDataModel.Hitbox()
                    {
                        Item = item,
                        String = stringId,
                        DefaultString = secStringId,
                        Actions = hitboxActions,
                        Talk = fallbackAction,
                        Rect1 = new ProgramDataModel.Rect()
                        {
                            X = hitbox.X,
                            Y = hitbox.Y,
                            W = hitbox.W,
                            H = hitbox.H
                        },
                        Rect2 = new ProgramDataModel.Rect() //TODO: second rect
                    });
                }

                foreach (var (id, str) in room.Texts)
                {
                    var stringId = textCounter;
                    if (program.Strings.Any(p => p.Value == str))
                    {
                        stringId = program.Strings.First(p => p.Value == str).Key;
                    }
                    else
                    {
                        program.Strings[stringId] = str;
                        textCounter++;
                    }

                    textIds[(roomId, id)] = stringId;
                }
            }

            foreach (var (actionPath, actionOffset) in _programs.GetActionOffsetsForProgram(programId))
            {
                if (programs[actionPath].Type != OpenedPackage.ProgramType.Action)
                {
                    throw new Exception("Wrong program type for action");
                }

                var action = programs[actionPath];

                var object1 = 0;
                var object2 = 0; //TODO: is this used?
                if (action.Data.Length == 2)
                {
                    //(roomId, hitboxId)
                    var roomId = action.Data[0];
                    var hitboxId = action.Data[1];
                    if (!hitboxIdMapping.ContainsKey((roomId, hitboxId)))
                    {
                        throw new Exception("Missing hitbox");
                    }

                    object1 = hitboxIdMapping[(roomId, hitboxId)];
                }
                
                program.ActionScriptOffsets.Add(new ProgramDataModel.ActionScriptOffset()
                {
                    Action = actionIdMapping[action.Target],
                    Object1 = object1,
                    Object2 = object2, 
                    Offset = (ushort)(actionOffset)
                });
            }

            foreach (var (charPath, charOffset) in _programs.GetCharOffsetsForProgram(programId))
            {
                if (programs[charPath].Type != OpenedPackage.ProgramType.KeyChar)
                {
                    throw new Exception("Wrong program type for keychar");
                }

                var charScript = programs[charPath];
                program.CharScriptOffsets.Add(new ProgramDataModel.CharScriptOffset()
                {
                    Character = charScript.Target,
                    Offs = (ushort)(charOffset)
                });
            }

            foreach (var (convoPath, convoOffset) in _programs.GetConvoOffsetsForProgram(programId))
            {
                if (programs[convoPath].Type != OpenedPackage.ProgramType.Conversation)
                {
                    throw new Exception("Wrong program type for convo");
                }

                var convo = programs[convoPath];
                var num = convo.Target;
                if (convo.Data.Length != 1)
                {
                    throw new Exception("Missing convo text");
                }

                //TODO: room ID
                var msg = textIds[(1, convo.Data[0])];
                //TODO: ordering matters here
                program.Conversations.Add(new ProgramDataModel.Conversation()
                {
                    Offset = (ushort)(convoOffset),
                    Num = num,
                    Message = msg
                });
            }
            
            var indexCorrectedInstructions = new Dictionary<uint, BaseInstruction>(program.Instructions.Count);
            var currentRoom = ((LoadRoomInstruction)(programData.First(i => i.Value is LoadRoomInstruction).Value)).Num;
            foreach (var (offset, instruction) in programData)
            {
                var newInstruction = instruction;
                
                //TODO: this does not correctly handle char/action instructions but that might be ok
                if (instruction is LoadRoomInstruction loadRoom)
                {
                    currentRoom = loadRoom.Num;
                }
                
                if (instruction is MoveCharToPosInstruction { TargetingAnotherCharacter: false } moveChar)
                {
                    newInstruction = new MoveCharToPosInstruction()
                    {
                        Character = moveChar.Character,
                        Num = (short)pointIds[(currentRoom, moveChar.Num)]
                    };
                } else if (instruction is SetCharBoxInstruction setCharBox)
                {
                    newInstruction = new SetCharBoxInstruction()
                    {
                        Character = setCharBox.Character,
                        Num = (short)pointIds[(currentRoom, setCharBox.Num)]
                    };
                } else if (instruction is SetupWaitingCharInstruction { Val1: 1 } setupWaitingChar)
                {
                    newInstruction = new SetupWaitingCharInstruction()
                    {
                        Character = setupWaitingChar.Character,
                        Val1 = 1,
                        Val2 = (short)pointIds[(currentRoom, setupWaitingChar.Val2)]
                    };
                } else if (instruction is StartTalkInstruction startTalk)
                {
                    newInstruction = new StartTalkInstruction()
                    {
                        Character = startTalk.Character,
                        Num = (short)textIds[(currentRoom, startTalk.Num)]
                    };
                }
                //TODO: other places text entries are referenced (conversations?)

                indexCorrectedInstructions[offset] = newInstruction;
            }
            
            program.Instructions = indexCorrectedInstructions;
        }
        
        //save a debug JSON version too for inspecting
        var json = JsonConvert.SerializeObject(db, Formatting.Indented);
        File.WriteAllText(datPath + ".json", json);

        var memoryStream = new MemoryStream();
        var exporter = new MainExporter(memoryStream);
        exporter.Export(db);
        File.WriteAllBytes(datPath, memoryStream.ToArray());
    }
}