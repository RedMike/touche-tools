// See https://aka.ms/new-console-template for more information

using System.Linq;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Constants;
using ToucheTools.Exporters;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

var log = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(Program));

//load sequences
var sequenceBytes = File.ReadAllBytes("data/sequence_0.bytes");
//load palette
var palette = File.ReadAllLines("data/palette_1.csv").Select(l =>
{
    var c = l.Split(',').Select(byte.Parse).ToList();
    return new Rgb24(c[0], c[1], c[2]);
}).ToList();
//load images
var spriteBytes = File.ReadAllBytes("data/sprite.png");
var spriteImg = Image.Load<Rgb24>(spriteBytes, new PngDecoder());
if (spriteImg == null)
{
    throw new Exception("Can't decode sprite");
}
var roomBytes = File.ReadAllBytes("data/room.png");
var roomImage = Image.Load<Rgb24>(roomBytes, new PngDecoder());
if (roomImage == null)
{
    throw new Exception("Can't decode room");
}

var spriteIndexedBytes = new byte[spriteImg.Height, spriteImg.Width];
var spriteW = spriteImg.Width;
var foundW = false;
var spriteH = spriteImg.Height;
var foundH = false;
var spriteColours = new HashSet<byte>();
spriteImg.ProcessPixelRows(pixelAccessor => {
    for (var i = 0; i < pixelAccessor.Height; i++)
    {
        var row = pixelAccessor.GetRowSpan(i);
        for (var j = 0; j < pixelAccessor.Width; j++)
        {
            var pixel = row[j];
            if (pixel.R != pixel.G || pixel.G != pixel.B || pixel.R != pixel.B)
            {
                throw new Exception($"Invalid input image: {pixel.R} {pixel.G} {pixel.B}");
            }
            var col = palette[pixel.R];
            if (!foundH && (pixel.R == 64 || pixel.R == 255) && j == 0)
            {
                foundH = true;
                spriteH = i;
            }
            if (!foundW && (pixel.R == 64 || pixel.R == 255) && i == 0 && j != 0)
            {
                foundW = true;
                spriteW = j;
            }

            spriteColours.Add(pixel.R);
            spriteIndexedBytes[i, j] = pixel.R;
        }
    }
});
var roomIndexedBytes = new byte[roomImage.Height, roomImage.Width];
roomImage.ProcessPixelRows(pixelAccessor => {
    for (var i = 0; i < pixelAccessor.Height; i++)
    {
        var row = pixelAccessor.GetRowSpan(i);
        for (var j = 0; j < pixelAccessor.Width; j++)
        {
            var pixel = row[j];
            var col = palette.FindIndex(p => p.R == pixel.R && p.G == pixel.G && p.B == pixel.B);
            if (col == -1)
            {
                var found = false;
                //try to replace an unused colour in the palette
                for (var q = 0; q < 255; q++)
                {
                    if (!spriteColours.Contains((byte)q))
                    {
                        found = true;
                        spriteColours.Add((byte)q);
                        palette[q] = new Rgb24(pixel.R, pixel.G, pixel.B);
                        col = q;
                        break;
                    }
                }

                if (!found)
                {
                    //closest approximation
                    col = palette.Min(p =>
                        Math.Abs(p.R - pixel.R) * 3 + Math.Abs(p.G - pixel.G) + Math.Abs(p.B - pixel.B) * 2);
                }
            }

            roomIndexedBytes[i, j] = (byte)col;
        }
    }
});

//build image-related things
var sprite = new SpriteImageDataModel()
{
    Width = spriteImg.Width,
    Height = spriteImg.Height,
    SpriteWidth = spriteW,
    SpriteHeight = spriteH,
    RawData = spriteIndexedBytes
};
var inventory = new SpriteImageDataModel()
{
    Width = 640,
    Height = 48,
    SpriteWidth = 640,
    SpriteHeight = 48,
    RawData = new byte[48, 640]
};
var menu = new SpriteImageDataModel()
{
    Width = 42,
    Height = 120,
    SpriteWidth = 42,
    SpriteHeight = 120,
    RawData = new byte[120, 42]
};
var conv = new SpriteImageDataModel()
{
    Width = 152,
    Height = 80,
    SpriteWidth = 152,
    SpriteHeight = 80,
    RawData = new byte[80, 152]
};
var cursor = new IconImageDataModel()
{
    Width = 58,
    Height = 42,
    RawData = new byte[42, 58]
};
var roomImg = new RoomImageDataModel()
{
    Width = roomImage.Width,
    Height = roomImage.Height,
    RawData = roomIndexedBytes
};
var paletteModel = new PaletteDataModel()
{
    Colors = palette.Select(p => new PaletteDataModel.Rgb()
    {
        R = p.R,
        G = p.G,
        B = p.B
    }).ToList()
};


//build model
var db = new DatabaseModel();
db.Text = new TextDataModel()
{
    Strings = new Dictionary<int, string>()
};
db.Backdrop = new BackdropDataModel()
{
    Width = 1996,
    Height = 544
};
db.Icons = new Dictionary<int, Lazy<IconImageDataModel>>()
{
    { 0, new Lazy<IconImageDataModel>(cursor) } //cursor
};
db.Sprites = new Dictionary<int, Lazy<SpriteImageDataModel>>()
{
    { 0, new Lazy<SpriteImageDataModel>(sprite) },
    
    { 12, new Lazy<SpriteImageDataModel>(inventory)}, //inventory background
    { 18, new Lazy<SpriteImageDataModel>(menu) }, //menu
    { 19, new Lazy<SpriteImageDataModel>(conv) } //conversation
};
db.Sequences = new Dictionary<int, SequenceDataModel>()
{
    {
        0, new SequenceDataModel()
        {
            Bytes = sequenceBytes
        }
    }
};
db.RoomImages = new Dictionary<int, Lazy<RoomImageDataModel>>()
{
    { 1, new Lazy<RoomImageDataModel>(roomImg) },
    { 2, new Lazy<RoomImageDataModel>(roomImg) }
};
db.Rooms = new Dictionary<int, RoomInfoDataModel>()
{
    {
        1, new RoomInfoDataModel()
        {
            RoomImageNum = 1
        }
    },
    {
        2, new RoomInfoDataModel()
        {
            RoomImageNum = 2
        }
    }
};
db.Palettes = new Dictionary<int, PaletteDataModel>()
{
    { 1, paletteModel },
    { 2, paletteModel }
};
db.Programs = new Dictionary<int, ProgramDataModel>();
db.Programs[Game.StartupEpisode] = new ProgramDataModel()
{
      // Areas = new List<ProgramDataModel.Area>()
      // {
      //     new ProgramDataModel.Area()
      //     {
      //         Rect = new ProgramDataModel.Rect()
      //         {
      //             X = 0,
      //             Y = 0,
      //             W = 640,
      //             H = 400
      //         },
      //         Id = 0,
      //         AnimationCount = 0,
      //         AnimationNext = 0,
      //         State = 0,
      //         SrcX = 0,
      //         SrcY = 0
      //     }
      // },
      Rects = new List<ProgramDataModel.Rect>()
      {
          new ProgramDataModel.Rect()
          {
              X = 0,
              Y = 0,
              W = 640,
              H = 400
          }
      },
      Points = new List<ProgramDataModel.Point>()
      {
          new ProgramDataModel.Point()
          {
              X = 0,
              Y = 0,
              Z = 0,
              Order = 0
          },
          new ProgramDataModel.Point()
          {
              X = 200,
              Y = 200,
              Z = 160,
              Order = 0
          },
          new ProgramDataModel.Point()
          {
              X = 500,
              Y = 200,
              Z = 160,
              Order = 0
          },
      },
      Backgrounds = new List<ProgramDataModel.Background>()
      {
          new ProgramDataModel.Background()
          {
              Type = 0,
              Rect = new ProgramDataModel.Rect()
              {
                  X = 20000,
                  Y = 20000,
                  W = 528,
                  H = 50
              },
              SrcX = 660,
              SrcY = 24,
              ScaleDiv = 0,
              ScaleMul = 0
          }
      },
      Strings = new Dictionary<int, string>()
      {
          { 1, "Dummy" }
      },
      Instructions = new List<BaseInstruction>()
      {
          new NoopInstruction(),
          new InitCharInstruction()
          {
              Character = 0
          },
          new LoadSpriteInstruction()
          {
              Index = 0,
              Num = 0
          },
          new LoadSequenceInstruction()
          {
              Num = 0
          },
          new InitCharScriptInstruction()
          {
              Character = 0,
              Color = 255,
              F1 = 0,
              F2 = 0,
              F3 = 0
          },
          new SetCharFrameInstruction()
          {
              Character = 0,
              Val1 = 0,
              Val2 = 0,
              Val3 = 1
          },
          new SetCharBoxInstruction()
          {
              Character = 0,
              Num = 1
          },
          new EnableInputInstruction(),
          
          new FetchScriptWordInstruction()
          {
              Val = 1
          },
          new SetFlagInstruction()
          {
              Flag = 606 //disable inventory redraw
          },
          
          new FetchScriptWordInstruction()
          {
              Val = 0
          },
          new SetFlagInstruction()
          {
              Flag = 618 //hide mouse cursor 
          },
          
          // new FetchScriptWordInstruction()
          // {
          //     Val = 1
          // },
          // new SetFlagInstruction()
          // {
          //     Flag = 902 //debug walks 
          // },
          
          new FetchScriptWordInstruction()
          {
              Val = 20000
          },
          new SetFlagInstruction()
          {
              Flag = 252
          },
          new FetchScriptWordInstruction()
          {
              Val = 20000
          },
          new SetFlagInstruction()
          {
              Flag = 253
          },
          
          new SetCharFlagsInstruction()
          {
              Character = 0,
              Flags = 0
          },
          
          new LoadRoomInstruction()
          {
              Num = 1
          },
          new FetchScriptWordInstruction()
          {
              Val = 104
          },
          new SetFlagInstruction()
          {
              Flag = 0 //active char
          },
          
          new FetchScriptWordInstruction()
          {
              Val = 0
          },
          new SetFlagInstruction()
          {
              Flag = 614 //room offset x
          },
          
          new FetchScriptWordInstruction()
          {
              Val = 0
          },
          new SetFlagInstruction()
          {
              Flag = 615 //room offset h
          },
          new SetCharDirectionInstruction()
          {
              Character = 0,
              Direction = 3
          },
          new MoveCharToPosInstruction()
          {
              Character = 0,
              Num = 2
          },
          new StopScriptInstruction(),
          new StopScriptInstruction(),
          new StopScriptInstruction(),
          new StopScriptInstruction()
      }
};

var memStream = new MemoryStream();
var mainExporter = new MainExporter(memStream);
mainExporter.Export(db);
var bytes = memStream.ToArray();

var filename = $"TOUCHE_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.DAT";
File.WriteAllBytes(filename, bytes);