using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class MainExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(MainExporter));
    private const int GapBetweenOffsetBlockAndDataBlock = 100; //how many bytes to leave between offsets and data block
    private const int DataBlockEntityOffsetGap = 2; //how many bytes to leave between entities in data block
    private static readonly int StartOfDataBlockOffset; //where the data can live
    private int _currentNextEntityOffset = DataBlockEntityOffsetGap; //where the data will currently be inserted

    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    private readonly ResourceDataExporter _resourceExporter;

    static MainExporter()
    {
        var lastResource = Resources.DataInfo.Select(d => d.Value)
            .MaxBy(d => d.Offset);
        if (lastResource == null)
        {
            throw new Exception("No resource offsets found");
        }

        StartOfDataBlockOffset = GapBetweenOffsetBlockAndDataBlock + lastResource.Offset + lastResource.Count * 4;
    }
    
    public MainExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
        _resourceExporter = new ResourceDataExporter(_stream);

        _logger.Log(LogLevel.Information, "Start of data block offset: {}", StartOfDataBlockOffset);
    }

    public void Export(DatabaseModel db)
    {
        //TODO: export everything

        foreach (var pair in db.Palettes)
        {
            var id = pair.Key;
            var palette = pair.Value;
            var roomImageNum = 0;
            if (db.RoomImages.ContainsKey(id))
            {
                roomImageNum = id;
                //first save the room image as it's referenced by the room info
                using var roomImageStream = new MemoryStream();
                var roomImageExporter = new RoomImageDataExporter(roomImageStream);
                roomImageExporter.Export(db.RoomImages[id].Value);
                var roomImageBytes = roomImageStream.GetBuffer();
                var roomImageOffset = AllocateAndReturnOffset(roomImageBytes.Length);
                //save the actual data first
                _stream.Seek(roomImageOffset, SeekOrigin.Begin);
                _writer.Write(roomImageBytes);
                //now save the offset for it
                _resourceExporter.Export(Resource.RoomImage, roomImageNum, roomImageOffset);
            }

            using var memStream = new MemoryStream();
            var roomInfoExporter = new RoomInfoDataExporter(memStream);
            roomInfoExporter.Export(palette, (ushort)roomImageNum);
            var bytes = memStream.GetBuffer();

            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.RoomInfo, id, offset);
        }
        //also save resources we don't have, as null offsets
        for (var i = 0; i < Resources.DataInfo[Resource.RoomInfo].Count; i++)
        {
            if (!db.Palettes.ContainsKey(i + 1))
            {
                _resourceExporter.Export(Resource.RoomInfo, i + 1, 0);
            }
        }

        foreach (var pair in db.Sprites)
        {
            var id = pair.Key;
            var spriteImage = pair.Value.Value;
            using var memStream = new MemoryStream();
            var spriteExporter = new SpriteImageDataExporter(memStream);
            spriteExporter.Export(spriteImage);
            var bytes = memStream.GetBuffer();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.SpriteImage, id, offset);
        }
        //also save resources we don't have, as null offsets
        for (var i = 0; i < Resources.DataInfo[Resource.SpriteImage].Count; i++)
        {
            if (!db.Sprites.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.SpriteImage, i, 0);
            }
        }
        
        foreach (var pair in db.Icons)
        {
            var id = pair.Key;
            var iconImage = pair.Value.Value;
            using var memStream = new MemoryStream();
            var iconExporter = new IconImageDataExporter(memStream);
            iconExporter.Export(iconImage);
            var bytes = memStream.GetBuffer();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.IconImage, id, offset);
        }
        //also save resources we don't have, as null offsets
        for (var i = 0; i < Resources.DataInfo[Resource.IconImage].Count; i++)
        {
            if (!db.Icons.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.IconImage, i, 0);
            }
        }

        foreach (var pair in db.Programs)
        {
            var id = pair.Key;
            var program = pair.Value;
            using var memStream = new MemoryStream();
            var programExporter = new ProgramDataExporter(memStream);
            programExporter.Export(program);
            var bytes = memStream.GetBuffer();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.Program, id, offset);
        }
        //now also save programs that are empty so that the size calculations work, and an additional final program
        for (var i = 0; i < Resources.DataInfo[Resource.Program].Count + 1; i++)
        {
            if (!db.Programs.ContainsKey(i))
            {
                var offset = AllocateAndReturnOffset(0);
                _resourceExporter.Export(Resource.Program, i, offset);
            }
        }
    }

    private int AllocateAndReturnOffset(int size)
    {
        var nextEntityOffset = _currentNextEntityOffset;
        _currentNextEntityOffset += size + DataBlockEntityOffsetGap;
        return nextEntityOffset + StartOfDataBlockOffset;
    }
}