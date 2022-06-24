using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class RoomInfoDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(RoomInfoDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public RoomInfoDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(PaletteDataModel palette, ushort roomImageOffset)
    {
        _stream.Seek(2, SeekOrigin.Begin);
        _writer.Write(roomImageOffset);
        _stream.Seek(2, SeekOrigin.Current);

        for (var i = 0; i < 256; i++)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            if (palette.Colors.Count > i)
            {
                var color = palette.Colors[i];
                r = color.R;
                g = color.G;
                b = color.B;
            }

            _writer.Write(r);
            _writer.Write(g);
            _writer.Write(b);
        }
    }
}