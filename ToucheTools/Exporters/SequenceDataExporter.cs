using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class SequenceDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(SequenceDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public SequenceDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(SequenceDataModel sequence)
    {
        var partSegments = new Dictionary<int, int>(); //within partsStream
        var partOffsets = new Dictionary<int, int>(); //global
        var partsOffOff = new Dictionary<int, int>(); //within partsStream
        var frameOffsets = new Dictionary<int, int>(); //within framesStream

        var partsStream = new MemoryStream();
        var framesStream = new MemoryStream();
        
        var globalOffset = 15998;
        var globalAllocate = (int size) =>
        {
            globalOffset -= size;
            return globalOffset;
        };
        {
            var partsWriter = new BinaryWriter(partsStream);
            var partOffOffset = 0;
            var partsOffAllocate = (int size) =>
            {
                var offset = partOffOffset;
                partOffOffset += size;
                return offset;
            };

            var charStreams = new Dictionary<int, (MemoryStream, BinaryWriter)>();
            var charsForPart = new Dictionary<int, List<int>>();
            foreach (var pair in sequence.PartMappings
                         .Select(i => (i.Key.Item1, i.Value))
                         .GroupBy(i => i.Item1))
            {
                var charId = pair.Key;
                var stream = new MemoryStream();
                charStreams[charId] = (stream, new BinaryWriter(stream));
                foreach (var partId in pair.Select(p => p.Value))
                {
                    if (!charsForPart.ContainsKey(partId))
                    {
                        charsForPart[partId] = new List<int>();
                    }

                    if (!charsForPart[partId].Contains(charId))
                    {
                        charsForPart[partId].Add(charId);
                    }
                }
            }

            //parts shared by more characters show up earlier to minimise dead space
            foreach (var pair in charsForPart.OrderByDescending(p => p.Value.Count))
            {
                var partId = pair.Key;

                var partStream = new MemoryStream();
                var partWriter = new BinaryWriter(partStream);
                foreach (var part in sequence.Parts[partId])
                {
                    partWriter.Write((ushort)BitConverter.ToUInt16(BitConverter.GetBytes((short)part.DestX), 0));
                    partWriter.Write((ushort)BitConverter.ToUInt16(BitConverter.GetBytes((short)part.DestY), 0));
                    partWriter.Write((ushort)BitConverter.ToUInt16(BitConverter.GetBytes((short)part.RawFrameIndex),
                        0));
                }

                partWriter.Write((ushort)BitConverter.ToUInt16(BitConverter.GetBytes((short)10000), 0));
                var bytes = partStream.ToArray();

                partOffsets[partId] = globalAllocate(bytes.Length);
                _stream.Seek(partOffsets[partId], SeekOrigin.Begin);
                _writer.Write(bytes);

                partsOffOff[partId] = partsOffAllocate(2);
                
                foreach (var charId in sequence.PartMappings
                             .Select(i => (i.Key.Item1)).Distinct())
                {
                    var (charStream, charWriter) = charStreams[charId];
                    charStream.Seek(partsOffOff[partId], SeekOrigin.Begin);
                    charWriter.Write(partOffsets[partId]);
                }
            }

            var partSegmentOffset = 0;
            foreach (var pair in charStreams)
            {
                partSegments[pair.Key] = partSegmentOffset;
                var bytes = pair.Value.Item1.ToArray();
                partsWriter.Write(bytes);
                partSegmentOffset += bytes.Length;
            }

            var partBytes = partsStream.ToArray();
            var partSegmentsOffset = globalAllocate(partBytes.Length);
            _stream.Seek(partSegmentsOffset, SeekOrigin.Begin);
            _writer.Write(partBytes);
            foreach (var pair in partSegments)
            {
                partSegments[pair.Key] += partSegmentsOffset;
            }
        }

        {
            var framesWriter = new BinaryWriter(framesStream);
            var framesOffset = 0;
            var framesAllocate = (int size) =>
            {
                var offset = framesOffset;
                framesOffset += size;
                return offset;
            };
            var seenFrames = new List<int>();
            foreach (var pair in sequence.FrameMappings.GroupBy(i => i.Key.Item1))
            {
                var charId = pair.Key;
                foreach (var frameData in pair)
                {
                    if (seenFrames.Contains(frameData.Value))
                    {
                        continue;
                    }

                    seenFrames.Add(frameData.Value);
                    var animId = frameData.Key.Item2;
                    var dirId = frameData.Key.Item3;

                    var frameStream = new MemoryStream();
                    var frameWriter = new BinaryWriter(frameStream);
                    var frameIdx = 0;
                    foreach (var frame in sequence.Frames[frameData.Value])
                    {
                        var partId = sequence.PartMappings.First(i => i.Key.Item1 == charId &&
                                                                      i.Key.Item2 == animId &&
                                                                      i.Key.Item3 == dirId &&
                                                                      i.Key.Item4 == frameIdx)
                            .Value;
                        var frameFlag = (ushort)(partsOffOff[partId]/2);
                        if (frame == sequence.Frames[frameData.Value].Last())
                        {
                            frameFlag = (ushort)(frameFlag | 0x8000);
                        }

                        frameWriter.Write((ushort)frameFlag);
                        frameWriter.Write((ushort)frame.WalkDx);
                        frameWriter.Write((ushort)frame.WalkDy);
                        frameWriter.Write((ushort)frame.WalkDz);
                        frameWriter.Write((ushort)frame.Delay);

                        frameIdx++;
                    }

                    var bytes = frameStream.ToArray();
                    var frameOff = framesAllocate(bytes.Length);
                    frameOffsets[frameData.Value] = frameOff;
                    framesStream.Seek(frameOff, SeekOrigin.Begin);
                    framesWriter.Write(bytes);
                }
            }

            var frameBytes = framesStream.ToArray();
            var frameOffset = globalAllocate(frameBytes.Length);
            _stream.Seek(frameOffset, SeekOrigin.Begin);
            _writer.Write(frameBytes);
            foreach (var pair in frameOffsets)
            {
                frameOffsets[pair.Key] += frameOffset;
            }
        }

        {
            var chars = sequence.FrameMappings.Select(i => i.Key.Item1).Distinct().ToList();
            var charSegmentEnd = chars.Count * 8 + 8;
            var animOffset = charSegmentEnd;
            foreach (var charId in chars)
            {
                _stream.Seek(charId * 8, SeekOrigin.Begin);
                _writer.Write((ushort)(partSegments[charId]));
                _writer.Write((ushort)(animOffset));
                _writer.Write((ushort)(sequence.CharToFrameFlag[charId]));
                    
                var anims = sequence.FrameMappings.Where(i => i.Key.Item1 == charId)
                    .Select(i => i.Key.Item2).Distinct().ToList();
                var dirOffset = animOffset + anims.Count * 4 + 4;
                if (dirOffset >= globalOffset)
                {
                    throw new Exception("Ran out of space in 16kb");
                }
                foreach (var animId in anims)
                {
                    _stream.Seek(animOffset, SeekOrigin.Begin);
                    _writer.Write((ushort)dirOffset);
                    animOffset += 4;
                    
                    var dirs = sequence.FrameMappings.Where(i => i.Key.Item1 == charId && i.Key.Item2 == animId)
                        .Select(i => i.Key.Item3).Distinct().ToList();
                    foreach (var dirId in dirs)
                    {
                        _stream.Seek(dirOffset, SeekOrigin.Begin);
                        dirOffset += 2;
                        var firstFrameId = sequence.FrameMappings[(charId, animId, dirId)];
                        var firstFrameOffset = frameOffsets[firstFrameId];
                        _writer.Write((ushort)firstFrameOffset);
                    }
                }

                _stream.Seek(animOffset, SeekOrigin.Begin);
                _writer.Write((ushort)0);
                _writer.Write((ushort)9999);
                animOffset = dirOffset;
                if (dirOffset >= globalOffset)
                {
                    throw new Exception("Ran out of space in 16kb");
                }
            }
        }
    }
}