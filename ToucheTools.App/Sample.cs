using ToucheTools.Constants;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App;

public static class Sample
{
    public static DatabaseModel Model()
    {
        return new DatabaseModel()
        {
            Text = Text(),
            Backdrop = new BackdropDataModel()
            {
                Width = ToucheTools.Constants.Resources.BackdropWidth,
                Height = ToucheTools.Constants.Resources.BackdropHeight
            },
            Icons = Icons(),
            Sprites = Sprites(),
            Palettes = Palettes(),
            Rooms = Rooms(),
            RoomImages = RoomImages(),
            Sequences = Sequences(),
            Programs = Programs(),
        };
    }
    
    #region Icons

    private static IconImageDataModel Cursor()
    {
        var w = ToucheTools.Constants.Icons.Width;
        var h = ToucheTools.Constants.Icons.Height;
        var b = GetImage(w, h, 0);
        b[h / 2, w / 2] = 255;

        return new IconImageDataModel()
        {
            Width = w,
            Height = h,
            RawData = b,
            DecodedData = b
        };
    }
    #endregion
    
    #region Sprites
    private const int Sprite1FramesPerLine = 2;
    private const int Sprite1Frames = 5;
    private const int Sprite1TileWidth = 100;
    private const int Sprite1TileHeight = 60;
    
    private static SpriteImageDataModel Sprite1()
    {
        var lines = (int)Math.Ceiling((float)Sprite1Frames / Sprite1FramesPerLine);

        short w = (short)(Sprite1TileWidth * Sprite1FramesPerLine); 
        short h = (short)(Sprite1TileHeight * lines); 
        var b = GetImage(w, h, 0);
        //part 1 (rest)
        b = AddRectangle(b, w, h, 
            Sprite1TileWidth - 10, Sprite1TileHeight - 4,
            2, 2, 50); 
        //part 2 (walk 1)
        b = AddRectangle(b, w, h, 
            Sprite1TileWidth - 10, Sprite1TileHeight - 4, 
            Sprite1TileWidth + 5, 2, 5); 
        //part 3 (walk 2)
        b = AddRectangle(b, w, h, 
            Sprite1TileWidth - 10, Sprite1TileHeight - 4, 
            2, Sprite1TileHeight + 5, 30); 
        

        b[0, Sprite1TileWidth-1] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor;
        b[Sprite1TileHeight-1, 0] = ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor;
        
        var bd = GetDecodedImage(b, w, h);
        
        return new SpriteImageDataModel()
        {
            Width = w,
            Height = h,
            SpriteWidth = Sprite1TileWidth,
            SpriteHeight = Sprite1TileHeight,
            RawData = b,
            DecodedData = bd,
        };
    }
    
    private static SpriteImageDataModel ActionMenu()
    {
        var w = ToucheTools.Constants.Sprites.ActionMenuWidth;
        var h = ToucheTools.Constants.Sprites.ActionMenuHeight;
        var b = GetImage(w, h, 0);
        b = AddRectangle(b, w, h, ToucheTools.Constants.Sprites.ActionMenuWidth-4, ToucheTools.Constants.Sprites.ActionMenuHeight-4, 2, 2, 50);

        var bd = GetDecodedImage(b, w, h);
        
        return new SpriteImageDataModel()
        {
            Width = ToucheTools.Constants.Sprites.ActionMenuWidth,
            Height = ToucheTools.Constants.Sprites.ActionMenuHeight,
            SpriteWidth = ToucheTools.Constants.Sprites.ActionMenuWidth,
            SpriteHeight = ToucheTools.Constants.Sprites.ActionMenuHeight,
            RawData = b,
            DecodedData = bd,
        };
    }

    private static SpriteImageDataModel ConversationMenu()
    {
        //TODO: load image from file
        return new SpriteImageDataModel()
        {
            Width = ToucheTools.Constants.Sprites.ConversationMenuWidth,
            Height = ToucheTools.Constants.Sprites.ConversationMenuHeight,
            SpriteWidth = ToucheTools.Constants.Sprites.ConversationMenuWidth,
            SpriteHeight = ToucheTools.Constants.Sprites.ConversationMenuHeight,
            RawData = new byte[ToucheTools.Constants.Sprites.ConversationMenuHeight, ToucheTools.Constants.Sprites.ConversationMenuWidth],
            DecodedData = new byte[ToucheTools.Constants.Sprites.ConversationMenuHeight, ToucheTools.Constants.Sprites.ConversationMenuWidth],
        };
    }
    
    private static SpriteImageDataModel InventoryBackground1()
    {
        //TODO: load image from file
        return new SpriteImageDataModel()
        {
            Width = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            Height = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            SpriteWidth = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            SpriteHeight = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            RawData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
            DecodedData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
        };
    }
    
    private static SpriteImageDataModel InventoryBackground2()
    {
        //TODO: load image from file
        return new SpriteImageDataModel()
        {
            Width = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            Height = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            SpriteWidth = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            SpriteHeight = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            RawData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
            DecodedData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
        };
    }
    
    private static SpriteImageDataModel InventoryBackground3()
    {
        //TODO: load image from file
        return new SpriteImageDataModel()
        {
            Width = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            Height = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            SpriteWidth = ToucheTools.Constants.Sprites.InventoryBackgroundWidth,
            SpriteHeight = ToucheTools.Constants.Sprites.InventoryBackgroundHeight,
            RawData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
            DecodedData = new byte[ToucheTools.Constants.Sprites.InventoryBackgroundHeight, ToucheTools.Constants.Sprites.InventoryBackgroundWidth],
        };
    }
    #endregion

    #region Rooms
    private static List<PaletteDataModel.Rgb> Colours()
    {
        var list = new List<PaletteDataModel.Rgb>()
        {
            
        };
        for (var i = 0; i < 256; i++)
        {
            if (i == ToucheTools.Constants.Palettes.TransparencyColor)
            {
                list.Add(new PaletteDataModel.Rgb());
            } else if (i == ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor)
            {
                list.Add(new PaletteDataModel.Rgb()
                {
                    R = 40,
                    G = 40,
                    B = 40
                });
            } else if (i == ToucheTools.Constants.Palettes.TransparentRoomMarkerColor)
            {
                list.Add(new PaletteDataModel.Rgb()
                {
                    R = 200,
                    G = 200,
                    B = 200
                });
            } else if (i < ToucheTools.Constants.Palettes.StartOfSpriteColors)
            {
                list.Add(new PaletteDataModel.Rgb()
                {
                    R = (byte)(i+50),
                    G = 1,
                    B = 1,
                });
            }
            else
            {
                list.Add(new PaletteDataModel.Rgb()
                {
                    R = 1,
                    G = 1,
                    B = (byte)(i/2),
                });
            }
        }

        return list;
    }

    private static RoomImageDataModel Room1()
    {
        var b = GetImage(1400, 600, 0);
        b = AddRectangle(b, 1400, 600, 200, 200, 450, 220, 150);
        b = AddRectangle(b, 1400, 600, 200, 100, 150, 200, 70);
        b[0, 1000] = ToucheTools.Constants.Palettes.TransparentRoomMarkerColor;
        return new RoomImageDataModel()
        {
            Width = 1400,
            Height = 600,
            RoomWidth = 1000,
            RawData = b
        };
    }
    #endregion
    
    #region Sequences
    private static SequenceDataModel Sequence1()
    {
        return new SequenceDataModel()
        {
            CharToFrameFlag = new Dictionary<int, ushort>()
            {
                {0, 0}
            },
            //frame 1 is idle
            //frames 20 is walk right
            //frames 21 is walk left
            //frames 30 is walk up/down
            Frames = new Dictionary<int, List<SequenceDataModel.FrameInformation>>()
            {
                {
                    1, new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                        {
                            
                            WalkDx = 0,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 0
                        }
                    }
                },
                {
                    20, new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 2
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 0
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 0
                        }
                    }
                },
                
                {
                    21, new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = -8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 2
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = -8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 0
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = -8,
                            WalkDy = 0,
                            WalkDz = 0,
                            Delay = 0
                        }
                    }
                },
                {
                    30, new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 0,
                            WalkDy = 0,
                            WalkDz = 8,
                            Delay = 2
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 0,
                            WalkDy = 0,
                            WalkDz = 8,
                            Delay = 0
                        },
                        new SequenceDataModel.FrameInformation()
                        {
                            WalkDx = 0,
                            WalkDy = 0,
                            WalkDz = 8,
                            Delay = 0
                        }
                    }
                },
            },
            //part 1 is stand frame
            //parts 2, 3, 4 are walk frames
            Parts = new Dictionary<int, List<SequenceDataModel.PartInformation>>()
            {
                {
                    1, new List<SequenceDataModel.PartInformation>()
                    {
                        new SequenceDataModel.PartInformation()
                        {
                            RawFrameIndex = 0,
                            DestX = -Sprite1TileWidth/2,
                            DestY = -Sprite1TileHeight
                        }
                    }
                },
                {
                    2, new List<SequenceDataModel.PartInformation>()
                    {
                        new SequenceDataModel.PartInformation()
                        {
                            RawFrameIndex = 1,
                            DestX = -Sprite1TileWidth/2,
                            DestY = -Sprite1TileHeight - 3
                        }
                    }
                },
                {
                    3, new List<SequenceDataModel.PartInformation>()
                    {
                        new SequenceDataModel.PartInformation()
                        {
                            RawFrameIndex = 2,
                            DestX = -Sprite1TileWidth/2,
                            DestY = -Sprite1TileHeight
                        }
                    }
                },
                {
                    4, new List<SequenceDataModel.PartInformation>()
                    {
                        new SequenceDataModel.PartInformation()
                        {
                            RawFrameIndex = 1,
                            DestX = -Sprite1TileWidth/2,
                            DestY = -Sprite1TileHeight
                        }
                    }
                },
            },
            FrameMappings = new Dictionary<(int, int, int), int>()
            {
                { (0, 0, 0), 1},
                { (0, 0, 1), 1},
                { (0, 0, 2), 1},
                { (0, 0, 3), 1},
                
                { (0, 1, 0), 30},
                { (0, 1, 1), 20},
                { (0, 1, 2), 30},
                { (0, 1, 3), 21},
            },
            PartMappings = new Dictionary<(int, int, int, int), int>()
            {
                { (0, 0, 0, 0), 1 },
                { (0, 0, 1, 0), 1 },
                { (0, 0, 2, 0), 1 },
                { (0, 0, 3, 0), 1 },
                
                { (0, 1, 0, 0), 2 },
                { (0, 1, 0, 1), 3 },
                { (0, 1, 0, 2), 4 },
                { (0, 1, 1, 0), 2 },
                { (0, 1, 1, 1), 3 },
                { (0, 1, 1, 2), 4 },
                { (0, 1, 2, 0), 2 },
                { (0, 1, 2, 1), 3 },
                { (0, 1, 2, 2), 4 },
                { (0, 1, 3, 0), 2 },
                { (0, 1, 3, 1), 3 },
                { (0, 1, 3, 2), 4 },
            }
        };
    }
    #endregion
    
    #region Programs
    #region Startup
    private static List<ProgramDataModel.Rect> StartupRects()
    {
        return new List<ProgramDataModel.Rect>()
        {
            {
                new ProgramDataModel.Rect()
                {
                    X = 0,
                    Y = 0,
                    W = 640,
                    H = 400
                }
            }
        };
    } 
    private static List<ProgramDataModel.Point> StartupPoints()
    {
        //0 1 2   4
        //      3
        return new List<ProgramDataModel.Point>()
        {
            {
                new ProgramDataModel.Point()
                {
                    X = 300,
                    Y = 250,
                    Z = 160
                }
            },
            {
                new ProgramDataModel.Point()
                {
                    X = 340,
                    Y = 250,
                    Z = 160
                }
            },
            {
                new ProgramDataModel.Point()
                {
                    X = 380,
                    Y = 250,
                    Z = 160
                }
            },
            {
                new ProgramDataModel.Point()
                {
                    X = 440,
                    Y = 350,
                    Z = 110
                }
            },
            {
                new ProgramDataModel.Point()
                {
                    X = 480,
                    Y = 250,
                    Z = 160
                }
            }
        };
    }

    private static List<ProgramDataModel.Walk> StartupWalks()
    {
        return new List<ProgramDataModel.Walk>()
        {
            {
                new ProgramDataModel.Walk()
                {
                    Point1 = 0,
                    Point2 = 1,
                    ClipRect = 0,
                    Area1 = -1,
                    Area2 = -1
                }
            },
            {
                new ProgramDataModel.Walk()
                {
                    Point1 = 1,
                    Point2 = 2,
                    ClipRect = 0,
                    Area1 = -1,
                    Area2 = -1
                }
            },
            {
                new ProgramDataModel.Walk()
                {
                    Point1 = 2,
                    Point2 = 3,
                    ClipRect = 0,
                    Area1 = -1,
                    Area2 = -1
                }
            },
            {
                new ProgramDataModel.Walk()
                {
                    Point1 = 3,
                    Point2 = 4,
                    ClipRect = 0,
                    Area1 = -1,
                    Area2 = -1
                }
            },
            {
                new ProgramDataModel.Walk()
                {
                    Point1 = 2,
                    Point2 = 4,
                    ClipRect = 0,
                    Area1 = -1,
                    Area2 = -1
                }
            },
        };
    }

    private static Dictionary<uint, BaseInstruction> StartupInstructions()
    {
        return new Dictionary<uint, BaseInstruction>()
        {
            {
                0,
                new NoopInstruction()
            },
            {
                2,
                new LoadRoomInstruction()
                {
                    Num = 1
                }
            },
            {
                6,
                new LoadSpriteInstruction()
                {
                    Index = 0,
                    Num = 1
                }
            },
            {
                12,
                new LoadSequenceInstruction()
                {
                    Index = 0,
                    Num = 0
                }
            },
            {
                18,
                new InitCharScriptInstruction()
                {
                    Character = 0,
                    Color = 255,
                    SpriteIndex = 0,
                    SequenceIndex = 0,
                    SequenceCharacterId = 0
                }
            },
            {
                30,
                new SetCharFrameInstruction()
                {
                    Character = 0,
                    Val1 = 2,
                    Val2 = 0,
                    Val3 = 0
                }
            },
            {
                40,
                new SetCharFrameInstruction()
                {
                    Character = 0,
                    Val1 = 4,
                    Val2 = 1,
                    Val3 = 0
                }
            },
            {
                50,
                new SetCharFrameInstruction()
                {
                    Character = 0,
                    Val1 = 0,
                    Val2 = 0,
                    Val3 = 1
                }
            },
            {
                60,
                new EnableInputInstruction()
            },
            {
                62,
                new SetCharFlagsInstruction()
                {
                    Character = 0,
                    Flags = 0
                }
            },
            {
                68,
                new SetCharBoxInstruction()
                {
                    Character = 0,
                    Num = 1
                }
            },
            {
                74,
                new SetCharDelayInstruction()
                {
                    Delay = 80
                }
            },
            {
                78,
                new MoveCharToPosInstruction()
                {
                    Character = 0,
                    Num = 4
                }
            },
            {
                84,
                new SetupWaitingCharInstruction()
                {
                    Character = 0, //actually ignored
                    Val1 = 1,
                    Val2 = 4
                }
            },
            {
                92,
                new StopScriptInstruction()
            },
            {
                94,
                new StopScriptInstruction()
            }
        };
    }
    #endregion

    private static ProgramDataModel StartupEpisode()
    {
        return new ProgramDataModel()
        {
            Rects = StartupRects(),
            Points = StartupPoints(),
            Walks = StartupWalks(),
            Instructions = StartupInstructions()
        };
    }
    #endregion

    private static Dictionary<int, Lazy<IconImageDataModel>> Icons()
    {
        return new Dictionary<int, Lazy<IconImageDataModel>>()
        {
            {ToucheTools.Constants.Icons.DefaultMouseCursor, new Lazy<IconImageDataModel>(Cursor)}
        };
    }
    
    private static Dictionary<int, Lazy<SpriteImageDataModel>> Sprites()
    {
        return new Dictionary<int, Lazy<SpriteImageDataModel>>()
        {
            { 1, new Lazy<SpriteImageDataModel>(Sprite1) },
            { ToucheTools.Constants.Sprites.ActionMenu, new Lazy<SpriteImageDataModel>(ActionMenu) },
            { ToucheTools.Constants.Sprites.ConversationMenu, new Lazy<SpriteImageDataModel>(ConversationMenu) },
            { ToucheTools.Constants.Sprites.InventoryBackground1, new Lazy<SpriteImageDataModel>(InventoryBackground1) },
            { ToucheTools.Constants.Sprites.InventoryBackground2, new Lazy<SpriteImageDataModel>(InventoryBackground2) },
            { ToucheTools.Constants.Sprites.InventoryBackground3, new Lazy<SpriteImageDataModel>(InventoryBackground3) },
        };
    }
    
    private static Dictionary<int, PaletteDataModel> Palettes()
    {
        return new Dictionary<int, PaletteDataModel>()
        {
            {1, new PaletteDataModel()
                {
                    Colors = Colours()
                }
            }
        };
    }

    private static Dictionary<int, RoomInfoDataModel> Rooms()
    {
        return new Dictionary<int, RoomInfoDataModel>()
        {
            {
                1, new RoomInfoDataModel()
                {
                    RoomImageNum = 1
                }
            }
        };
    }

    private static Dictionary<int, Lazy<RoomImageDataModel>> RoomImages()
    {
        return new Dictionary<int, Lazy<RoomImageDataModel>>()
        {
            {1, new Lazy<RoomImageDataModel>(Room1)}
        };
    }

    private static Dictionary<int, SequenceDataModel> Sequences()
    {
        return new Dictionary<int, SequenceDataModel>()
        {
            {0, Sequence1()}
        };
    }

    private static Dictionary<int, ProgramDataModel> Programs()
    {
        return new Dictionary<int, ProgramDataModel>()
        {
            { Game.StartupEpisode, StartupEpisode() }
        };
    }

    private static TextDataModel Text()
    {
        return new TextDataModel()
        {
            Strings = new Dictionary<int, string>()
            {
                {-Actions.DoNothing, ""},
                {-Actions.LeftClick, "Look at"},
                {-Actions.LeftClickWithItem, "Use"},
            }
        };
    }

    private static byte[,] GetImage(int w, int h, byte col)
    {
        var b = new byte[h, w];
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                b[i, j] = col;
            }
        }
        
        return b;
    }

    private static byte[,] AddRectangle(byte[,] b, int w, int h, int cw, int ch, int cx, int cy, byte col)
    {
        var b2 = new byte[h, w];

        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                b2[i, j] = b[i, j];
            }
        }
        
        for (var i = 0; i < ch; i++)
        {
            for (var j = 0; j < cw; j++)
            {
                var x = cx + j;
                var y = cy + i;
                if (x < 0 || x >= w || y < 0 || y >= h)
                {
                    continue;
                }

                b2[y, x] = col;
            }
        }

        return b2;
    }
    
    private static byte[,] GetDecodedImage(byte[,] b, int w, int h)
    {
        var bd = new byte[h, w];
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                bd[i, j] = b[i, j];
                if (bd[i, j] > 0 && 
                    bd[i, j] != ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor &&
                    bd[i, j] != ToucheTools.Constants.Palettes.TransparentRoomMarkerColor)
                {
                    if (bd[i, j] >= 2 + ToucheTools.Constants.Palettes.SpriteColorCount)
                    {
                        throw new Exception("Decoding image with values that are too high");
                    }

                    bd[i, j] += ToucheTools.Constants.Palettes.StartOfSpriteColors;
                }
            }
        }

        return bd;
    }
}