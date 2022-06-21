using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class MainDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(MainDataLoader));
    private readonly FileStream _stream;
    private readonly BinaryReader _reader;

    public MainDataLoader(string path)
    {
        _stream = File.OpenRead(path);
        _reader = new BinaryReader(_stream);
    }

    public void Read(out TextDataModel textData)
    {
        _stream.Seek(64, SeekOrigin.Begin);
        uint textDataOffs = _reader.ReadUInt32();
        uint textDataSize = _reader.ReadUInt32();
        _logger.Log(LogLevel.Information, "Text Data Offset(s?): {}", textDataOffs);
        _logger.Log(LogLevel.Information, "Text Data Size: {}", textDataSize);

        _stream.Seek((int)textDataOffs, SeekOrigin.Current);
        byte[] rawTextData = _reader.ReadBytes((int)textDataSize);
        textData = new TextDataModel((int)textDataSize, rawTextData);

        _stream.Seek(2, SeekOrigin.Current);

        ushort bw = _reader.ReadUInt16();
        ushort bh = _reader.ReadUInt16();
        int backdropSize = bw * bh;
        _logger.Log(LogLevel.Information, "Backdrop: {w}x{h}", bw, bh);
    }
}