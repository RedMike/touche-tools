using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class SpriteImageDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(SpriteImageDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    public SpriteImageDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(SpriteImageDataModel sprite)
    {
        _logger.LogInformation("Saving sprite of size {}x{}", sprite.Width, sprite.Height);
        
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write((ushort)sprite.Width);
        _writer.Write((ushort)sprite.Height);
        //RLE compression
        for (var i = 0; i < sprite.Height; i++)
        {
            var compressing = false;
            byte compressingColor = 0x00;
            byte compressingLength = 0;
            for (var j = 0; j < sprite.Width; j++)
            {
                byte color = sprite.RawData[i, j];
                if (compressing && 
                    (
                        color != compressingColor || //we're done compressing
                        compressingLength > 62 || //engine can only do RLE up to 63 length
                        j > (sprite.Width - 2) //we're near the edge
                    )
                ) 
                {
                    //RLE run over, save compressed run
                    _writer.Write((byte)(compressingLength | 0xC0));
                    _writer.Write(compressingColor);
                    compressing = false;
                }
                if (!compressing)
                {
                    if (
                        j >= (sprite.Width - 2) || //we're near the edge
                        sprite.RawData[i, j + 1] != color) //or there's a single colour so no point
                    {
                        //can't RLE compress
                        //color = (byte)(color / 4 * 3);
                        _writer.Write(color);
                    }
                    else
                    {
                        compressing = true;
                        compressingColor = color;
                        compressingLength = 1;
                    }
                }
                else
                {
                    if (color == compressingColor)
                    {
                        compressingLength++;
                    }
                }
            }
        }
    }
}