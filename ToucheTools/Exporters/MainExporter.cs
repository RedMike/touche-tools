﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class MainExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(MainExporter));
    private const int GapBetweenOffsetBlockAndDataBlock = 128; //how many bytes to leave between offsets and data block
    private static readonly int StartOfDataBlockOffset; //where the data can live
    private int _currentNextEntityOffset = 0; //where the data will currently be inserted

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
        if (db.Text == null)
        {
            throw new Exception("Missing text data");
        }
       
        //text data
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            var maxString = 0;
            if (db.Text.Strings.Count > 0)
            {
                maxString = db.Text.Strings.Keys.Max();
            }
            var nextOffset = maxString * 4 + 4;
            var allocate = (int size) =>
            {
                var retOffset = nextOffset;
                nextOffset += size;
                return retOffset;
            };

            for(var i = 1; i <= maxString; i++) //1-indexed
            {
                var s = "~";
                if (db.Text.Strings.ContainsKey(i))
                {
                    s = db.Text.Strings[i];
                }
                
                var offset = allocate(s.Length + 1);
                memStream.Seek(offset, SeekOrigin.Begin);
                for (var j = 0; j < s.Length; j++)
                {
                    writer.Write((byte)s[j]);
                }
                writer.Write((byte)0);

                memStream.Seek(i * 4, SeekOrigin.Begin);
                writer.Write((uint)offset);
            }
            
            var bytes = memStream.ToArray();
            
            //first save the data
            var textOffset = AllocateAndReturnOffset(bytes.Length);
            var textSize = bytes.Length;
            _stream.Seek(textOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //then save the offsets
            _stream.Seek(64, SeekOrigin.Begin);
            _writer.Write((uint)textOffset);
            _writer.Write((uint)textSize);
        }
        
        if (db.Backdrop == null)
        {
            throw new Exception("Missing backdrop data");
        }
        _stream.Seek(2, SeekOrigin.Begin);
        _writer.Write((ushort)db.Backdrop.Width);
        _writer.Write((ushort)db.Backdrop.Height);
        
        for (var i = 0; i < Resources.DataInfo[Resource.Sequence].Count; i++)
        {
            //also save resources we don't have, as null offsets
            if (!db.Sequences.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.Sequence, i, 0);
            }
        }
        foreach (var pair in db.Sequences)
        {
            var id = pair.Key;
            var sequence = pair.Value;
            var sequenceBytes = new byte[16000];
            using var memStream = new MemoryStream(sequenceBytes);
            var sequenceExporter = new SequenceDataExporter(memStream);
            sequenceExporter.Export(sequence);
            var bytes = memStream.ToArray();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.Sequence, id, offset);
        }
        _resourceExporter.Export(Resource.Sequence, db.Sequences.Keys.Max() + 1, AllocateAndReturnOffset(0));

        //also save resources we don't have, as null offsets
        for (var i = 1; i <= Resources.DataInfo[Resource.RoomInfo].Count; i++)
        {
            if (!db.Palettes.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.RoomInfo, i, 0);
            }
        }
        foreach (var pair in db.Rooms)
        {
            var id = pair.Key;
            var roomInfo = pair.Value;
            var palette = db.Palettes[id];
            
            if (db.RoomImages.ContainsKey(roomInfo.RoomImageNum))
            {
                //first save the room image as it's referenced by the room info
                using var roomImageStream = new MemoryStream();
                var roomImageExporter = new RoomImageDataExporter(roomImageStream);
                roomImageExporter.Export(db.RoomImages[roomInfo.RoomImageNum].Value);
                var roomImageBytes = roomImageStream.ToArray();
                var roomImageOffset = AllocateAndReturnOffset(roomImageBytes.Length);
                //save the actual data first
                _stream.Seek(roomImageOffset, SeekOrigin.Begin);
                _writer.Write(roomImageBytes);
                //now save the offset for it
                _resourceExporter.Export(Resource.RoomImage, roomInfo.RoomImageNum, roomImageOffset);
            }
            
            using var memStream = new MemoryStream();
            var roomInfoExporter = new RoomInfoDataExporter(memStream);
            roomInfoExporter.Export(palette, (ushort)roomInfo.RoomImageNum);
            var bytes = memStream.ToArray();

            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.RoomInfo, id, offset);
        }
        _resourceExporter.Export(Resource.RoomInfo, db.Rooms.Keys.Max() + 1, AllocateAndReturnOffset(0));

        //also save resources we don't have, as null offsets
        for (var i = 0; i <= Resources.DataInfo[Resource.SpriteImage].Count; i++)
        {
            if (!db.Sprites.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.SpriteImage, i, 0);
            }
        }
        foreach (var pair in db.Sprites)
        {
            var id = pair.Key;
            var spriteImage = pair.Value.Value;
            using var memStream = new MemoryStream();
            var spriteExporter = new SpriteImageDataExporter(memStream);
            spriteExporter.Export(spriteImage);
            var bytes = memStream.ToArray();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _logger.LogError("sprite: {} to {}", id, offset);
            _resourceExporter.Export(Resource.SpriteImage, id, offset);
        }
        _resourceExporter.Export(Resource.SpriteImage, db.Sprites.Keys.Max() + 1, AllocateAndReturnOffset(0));
        
        //also save resources we don't have, as null offsets
        for (var i = 0; i <= Resources.DataInfo[Resource.IconImage].Count-1; i++)
        {
            if (!db.Icons.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.IconImage, i, 0);
            }
        }
        foreach (var pair in db.Icons)
        {
            var id = pair.Key;
            var iconImage = pair.Value.Value;
            using var memStream = new MemoryStream();
            var iconExporter = new IconImageDataExporter(memStream);
            iconExporter.Export(iconImage);
            var bytes = memStream.ToArray();
            
            var offset = AllocateAndReturnOffset(bytes.Length);
            //save the actual data first
            _stream.Seek(offset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //now save the offset for it
            _resourceExporter.Export(Resource.IconImage, id, offset);
        }

        var iconMax = 0;
        if (db.Icons.Count > 0)
        {
            iconMax = db.Icons.Keys.Max();
        }
        _resourceExporter.Export(Resource.IconImage, iconMax + 1, AllocateAndReturnOffset(0));

        //also save programs that are empty so that the size calculations work, and an additional final program
        for (var i = 0; i <= Resources.DataInfo[Resource.Program].Count; i++)
        {
            if (!db.Programs.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.Program, i, _currentNextEntityOffset + StartOfDataBlockOffset);
            }
            else
            {
                var id = i;
                var program = db.Programs[i];
                using var memStream = new MemoryStream();
                var programExporter = new ProgramDataExporter(memStream);
                programExporter.Export(program);
                var bytes = memStream.ToArray();
            
                var offset = AllocateAndReturnOffset(bytes.Length);
                //save the actual data first
                _stream.Seek(offset, SeekOrigin.Begin);
                _writer.Write(bytes);
                //now save the offset for it
                _resourceExporter.Export(Resource.Program, id, offset);
            }
        }
        _resourceExporter.Export(Resource.Program, db.Programs.Keys.Max() + 1, AllocateAndReturnOffset(0));
        
        //also save resources we don't have, as null offsets
        for (var i = 0; i <= Resources.DataInfo[Resource.Music].Count; i++)
        {
            if (!db.MusicTracks.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.Music, i, _currentNextEntityOffset + StartOfDataBlockOffset);
            }
            else
            {
                var id = i;
                var music = db.MusicTracks[i];
                using var memStream = new MemoryStream();
                var musicExporter = new MusicDataExporter(memStream);
                musicExporter.Export(music);
                var bytes = memStream.ToArray();
            
                var offset = AllocateAndReturnOffset(bytes.Length);
                //save the actual data first
                _stream.Seek(offset, SeekOrigin.Begin);
                _writer.Write(bytes);
                //now save the offset for it
                _resourceExporter.Export(Resource.Music, id, offset);
            }
        }
        
        //also save resources we don't have, as null offsets
        for (var i = 0; i <= Resources.DataInfo[Resource.Sound].Count; i++)
        {
            if (!db.Sounds.ContainsKey(i))
            {
                _resourceExporter.Export(Resource.Sound, i, _currentNextEntityOffset + StartOfDataBlockOffset);
            }
            else
            {
                var id = i;
                var sound = db.Sounds[i];
                using var memStream = new MemoryStream();
                var soundExporter = new SoundDataExporter(memStream);
                soundExporter.Export(sound);
                var bytes = memStream.ToArray();
            
                var offset = AllocateAndReturnOffset(bytes.Length);
                //save the actual data first
                _stream.Seek(offset, SeekOrigin.Begin);
                _writer.Write(bytes);
                //now save the offset for it
                _resourceExporter.Export(Resource.Sound, id, offset);
            }
        }
    }

    private int AllocateAndReturnOffset(int size)
    {
        var nextEntityOffset = _currentNextEntityOffset;
        _currentNextEntityOffset += size;
        return nextEntityOffset + StartOfDataBlockOffset;
    }
}