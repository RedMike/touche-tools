using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class MainDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(MainDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;

    public MainDataLoader(Stream stream)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
    }

    public void Read(out TextDataModel? textData, out BackdropDataModel? backdrop)
    {
        _stream.Seek(64, SeekOrigin.Begin);
        
        uint textDataOffs = _reader.ReadUInt32();
        uint textDataSize = _reader.ReadUInt32();
        textData = new TextDataModel();
        
        _stream.Seek((int)textDataOffs, SeekOrigin.Begin);
        byte[] rawTextData = _reader.ReadBytes((int)textDataSize);
        using var memStream = new MemoryStream(rawTextData);
        var binaryReader = new BinaryReader(memStream);
        for (var i = 1; i < rawTextData.Length / 4; i++)
        {
            var s = "";
            memStream.Seek(i * 4, SeekOrigin.Begin);
            var offset = binaryReader.ReadUInt32();
            _stream.Seek(textDataOffs + offset, SeekOrigin.Begin);
            while (_stream.Position-textDataOffs < textDataSize)
            {
                var chr = _reader.ReadChar();
                if (chr == 0)
                {
                    break;
                }
                if (chr < 32 || chr >= 32 + Fonts.FontOffsets.Length)
                {
                    throw new Exception("Invalid character: " + chr);
                }

                s += chr;
            }

            if (!string.IsNullOrWhiteSpace(s))
            {
                textData.Strings[i] = s;
            }
        }

        _stream.Seek(2, SeekOrigin.Begin);

        ushort bw = _reader.ReadUInt16();
        ushort bh = _reader.ReadUInt16();
        backdrop = new BackdropDataModel();
        backdrop.Width = bw;
        backdrop.Height = bh;
    }
}