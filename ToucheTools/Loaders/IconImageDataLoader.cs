﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class IconImageDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(IconImageDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public IconImageDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out IconImageDataModel sprite)
    {
        _resourceDataLoader.Read(Resource.IconImage, number, false, out var offset, out _);
        _stream.Seek(offset, SeekOrigin.Begin);
        ushort rawWidth = _reader.ReadUInt16();
        if (rawWidth == 0)
        {
            throw new UnknownResourceException();
        }
        ushort rawHeight = _reader.ReadUInt16();
        var initialWidth = (int)rawWidth;
        var initialHeight = (int)rawHeight;
        _logger.Log(LogLevel.Debug, "Icon image {}: initially {}x{}", number, initialWidth, initialHeight);

        var imageData = new byte[initialHeight, initialWidth];
        //RLE compression
        for (var i = 0; i < initialHeight; i++)
        {
            for (var j = 0; j < initialWidth; j++)
            {
                byte code = _reader.ReadByte();
                if ((code & 0xC0) == 0xC0)
                {
                    var len = code & 0x3F;
                    byte color = _reader.ReadByte();
                    
                    //it's RLE, the next {len} pixels are the same colour
                    if ((j + len) > initialWidth)
                    {
                        _logger.LogError("RLE encoding overflowing to next line: {}x{} RLE length {} width {} by {}", j, i, len, initialWidth, j + len - initialWidth);
                        throw new Exception("RLE encoding overflowed to next line");
                    }
                    
                    for (var q = 0; q < len; q++)
                    {
                        imageData[i, j+q] = color;
                    }
                    j = j + len - 1;
                }
                else
                {
                    imageData[i, j] = code;
                }
            }
        }
        
        //find true width and height
        var height = initialHeight;
        //TODO: this wasn't correcting the byte array size
        // for (var i = 0; i < initialHeight; i++)
        // {
        //     if (imageData[i, 0] == 64 || imageData[i, 0] == 255)
        //     {
        //         height = i;
        //         break;
        //     }
        // }

        var width = initialWidth;
        //TODO: this wasn't correcting the byte array size
        // for (var i = 0; i < initialWidth; i++)
        // {
        //     if (imageData[0, i] == 64 || imageData[0, i] == 255)
        //     {
        //         width = i;
        //         break;
        //     }
        // }

        if (width != initialWidth || height != initialHeight)
        {
            _logger.Log(LogLevel.Information, "Sprite image {}: true {}x{}", number, width, height);
        }

        var decodedImageData = new byte[height, width];
        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                if (imageData[i, j] != 0)
                {
                    if (imageData[i, j] < 64)
                    {
                        decodedImageData[i, j] = (byte)(imageData[i, j] + 192);
                    }
                    else
                    {
                        decodedImageData[i, j] = 0;
                    }
                }
                else
                {
                    decodedImageData[i, j] = 0;
                }
            }
        }

        sprite = new IconImageDataModel()
        {
            Width = width,
            Height = height,
            RawData = imageData,
            DecodedData = decodedImageData
        };
    }
}