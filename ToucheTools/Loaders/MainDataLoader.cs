using System.Text;
using Microsoft.Extensions.Logging;
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
        textData.Offsets = textDataOffs;
        textData.Size = textDataSize;
        
        _stream.Seek((int)textDataOffs, SeekOrigin.Begin);
        byte[] rawTextData = _reader.ReadBytes((int)textDataSize);
        textData.Data = rawTextData;

        _stream.Seek(2, SeekOrigin.Begin);

        ushort bw = _reader.ReadUInt16();
        ushort bh = _reader.ReadUInt16();
        backdrop = new BackdropDataModel();
        backdrop.Width = bw;
        backdrop.Height = bh;
    }
}