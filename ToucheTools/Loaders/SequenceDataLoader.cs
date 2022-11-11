using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class SequenceDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(SequenceDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public SequenceDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(SequenceDataModel sequence)
    {
        var memStream = new MemoryStream(sequence.Bytes);
        var reader = new BinaryReader(memStream);

        {
            var partStartOffs = new List<int>();
            var animOffs = new List<int>();
            var dirOffs = new List<int>();
            var frameOffs = new List<int>();
            var partOffs = new List<int>();
            var characterId = 0;
            while (true)
            {
                memStream.Seek(characterId * 8 + 2, SeekOrigin.Begin);
                var animOff = reader.ReadUInt16();
                if (animOff == 0)
                {
                    break;
                }

                if (animOff >= 16000)
                {
                    throw new Exception("Invalid offset: " + animOff);
                }

                if (!animOffs.Contains(animOff))
                {
                    animOffs.Add(animOff);
                }


                memStream.Seek(characterId * 8 + 0, SeekOrigin.Begin);
                var partStartOff = reader.ReadUInt16();
                if (partStartOff >= 16000)
                {
                    throw new Exception("Invalid offset: " + partStartOff);
                }

                if (!partStartOffs.Contains(partStartOff))
                {
                    partStartOffs.Add(partStartOff);
                }

                memStream.Seek(characterId * 8 + 4, SeekOrigin.Begin);
                var charFrameFlag = reader.ReadUInt16();
                sequence.CharToFrameFlag[characterId] = charFrameFlag;

                var animationId = 0;
                while (true)
                {
                    memStream.Seek(animOff + animationId * 4, SeekOrigin.Begin);
                    if (animOff + animationId * 4 >= memStream.Length)
                    {
                        break;
                    }

                    var dirOff = reader.ReadUInt16();
                    if (dirOff == 0)
                    {
                        break;
                    }

                    if (reader.ReadUInt16() != 0)
                    {
                        break;
                    }

                    if (dirOff >= 16000)
                    {
                        throw new Exception("Invalid offset: " + dirOff);
                    }

                    if (frameOffs.Contains(dirOff))
                    {
                        //we've overshot out of animations into the next section
                        break;
                    }

                    if (!dirOffs.Contains(dirOff))
                    {
                        dirOffs.Add(dirOff);
                    }

                    for (var dirId = 0; dirId < 4; dirId++)
                    {
                        memStream.Seek(dirOff + dirId * 2, SeekOrigin.Begin);
                        if (dirOff + dirId * 2 >= memStream.Length)
                        {
                            break;
                        }

                        var frameOff = reader.ReadUInt16();
                        if (frameOff == 0)
                        {
                            break;
                        }

                        if (frameOff >= 16000)
                        {
                            throw new Exception("Invalid offset: " + frameOff);
                        }

                        if (!frameOffs.Contains(frameOff))
                        {
                            frameOffs.Add(frameOff);
                        }

                        sequence.FrameMappings[(characterId, animationId, dirId)] = frameOff;

                        var frameId = 0;
                        var lastFrame = false;
                        while (!lastFrame)
                        {
                            memStream.Seek(frameOff + frameId * 10, SeekOrigin.Begin);
                            var rawFrameFlag = reader.ReadUInt16();
                            if ((rawFrameFlag & 0x8000) > 0)
                            {
                                lastFrame = true;
                            }

                            var partOffOff = rawFrameFlag & 0x7FFF;
                            memStream.Seek(partStartOff + partOffOff * 2, SeekOrigin.Begin);
                            var partOff = reader.ReadUInt16();
                            if (partOff == 0)
                            {
                                break;
                            }

                            if (!partOffs.Contains(partOff))
                            {
                                partOffs.Add(partOff);
                            }

                            sequence.PartMappings[(characterId, animationId, dirId, frameId)] = partOff;

                            frameId++;
                        }
                    }

                    animationId++;
                }

                characterId++;
            }

            foreach (var frameOff in frameOffs)
            {
                sequence.Frames[frameOff] = new List<SequenceDataModel.FrameInformation>();

                var frameId = 0;
                var lastFrame = false;
                while (!lastFrame)
                {
                    var frame = new SequenceDataModel.FrameInformation();

                    memStream.Seek(frameOff + frameId * 10, SeekOrigin.Begin);
                    var rawFrameFlag = reader.ReadUInt16();
                    if ((rawFrameFlag & 0x8000) > 0)
                    {
                        lastFrame = true;
                    }

                    var walkDx = reader.ReadUInt16();
                    var walkDy = reader.ReadUInt16();
                    var walkDz = reader.ReadUInt16();
                    var delay = reader.ReadUInt16();
                    frame.WalkDx = walkDx;
                    frame.WalkDy = walkDy;
                    frame.WalkDz = walkDz;
                    frame.Delay = delay;

                    sequence.Frames[frameOff].Add(frame);
                    frameId++;
                }
            }

            foreach (var partOff in partOffs)
            {
                sequence.Parts[partOff] = new List<SequenceDataModel.PartInformation>();

                var partId = 0;
                while (true)
                {
                    var part = new SequenceDataModel.PartInformation();
                    memStream.Seek(partOff + partId * 6, SeekOrigin.Begin);

                    var rawDstX = reader.ReadUInt16();
                    var dstX = BitConverter.ToInt16(BitConverter.GetBytes(rawDstX), 0);
                    if (dstX == 10000)
                    {
                        break;
                    }

                    var rawDstY = reader.ReadUInt16();
                    var dstY = BitConverter.ToInt16(BitConverter.GetBytes(rawDstY), 0);

                    var rawFrameIndex = reader.ReadUInt16();
                    var frameIndex = BitConverter.ToInt16(BitConverter.GetBytes(rawFrameIndex), 0);

                    part.DestX = dstX;
                    part.DestY = dstY;
                    part.RawFrameIndex = frameIndex;

                    if (part.FrameIndex == 0x800)
                    {
                        continue;
                    }

                    sequence.Parts[partOff].Add(part);
                    partId++;
                }
            }
        }
    }

    public void Read(int number, out SequenceDataModel sequence)
    {
        _resourceDataLoader.Read(Resource.Sequence, number, false, out var offset, out _);
        _stream.Seek(offset, SeekOrigin.Begin);

        sequence = new SequenceDataModel();
        sequence.Bytes = new byte[16000];
        _stream.Read(sequence.Bytes, 0, 16000);
        Read(sequence);
    }
}