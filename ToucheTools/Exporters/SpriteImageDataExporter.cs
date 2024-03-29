﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Helpers;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class SpriteImageDataExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(SpriteImageDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    public SpriteImageDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(SpriteImageDataModel sprite)
    {
        _logger.LogInformation("Saving sprite of size {}x{} (individual {}x{})", sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight);
        
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write(sprite.Width.AsUshort());
        _writer.Write(sprite.Height.AsUshort());

        var addedHeightMeasure = false;
        var addedWidthMeasure = false;
        //RLE compression
        for (var i = 0; i < sprite.Height; i++)
        {
            var compressing = false;
            byte compressingColor = 0x00;
            byte compressingLength = 0;
            for (var j = 0; j < sprite.Width; j++)
            {
                byte color = sprite.RawData[i, j];
                if (!addedHeightMeasure && j == 0)
                {
                    if (i < sprite.SpriteHeight && color == 64)
                    {
                        color = 0;
                    }
                }
                if (!addedHeightMeasure && i == sprite.SpriteHeight && j == 0)
                {
                    addedHeightMeasure = true;
                    color = 64;
                }

                if (!addedWidthMeasure && i == 0)
                {
                    if (j < sprite.SpriteWidth && color == 64)
                    {
                        color = 0;
                    }
                }
                if (!addedWidthMeasure && i == 0 && j == sprite.SpriteWidth)
                {
                    addedWidthMeasure = true;
                    color = 64;
                }
                
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
                        if (color >= 0xC0)
                        {
                            //force compression at least of length 1
                            _writer.Write((byte)0xC1);
                        }
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