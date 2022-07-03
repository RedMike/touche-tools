using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class SequenceDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(SequenceDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public SequenceDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out SequenceDataModel sequence)
    {
        _resourceDataLoader.Read(Resource.Sequence, number, false, out var offset, out _);
        _stream.Seek(offset, SeekOrigin.Begin);

        sequence = new SequenceDataModel();
        sequence.Bytes = new byte[16000];
        _stream.Read(sequence.Bytes, 0, 16000);

        {
            var memStream = new MemoryStream(sequence.Bytes);
            var reader = new BinaryReader(memStream);

            var characterId = 0; //sequence offset //TODO:
            var character = new SequenceDataModel.CharacterInfo();
            sequence.Characters[characterId] = character;

            memStream.Seek(characterId * 8 + 2, SeekOrigin.Begin);
            var characterInfoOff = reader.ReadUInt16();
            
            memStream.Seek(characterId * 8 + 4, SeekOrigin.Begin);
            character.FrameDirFlag = reader.ReadUInt16();

            var animationId = 0; //TODO: 
            var animation = new SequenceDataModel.AnimationInfo();
            character.Animations[animationId] = animation;
            memStream.Seek(characterInfoOff + animationId * 4, SeekOrigin.Begin);
            var animationInfoOff = reader.ReadUInt16();
            
            var direction = 3; //facing //TODO: 
            var directedAnimation = new SequenceDataModel.DirectedAnimationInfo();
            character.Animations[animationId].Directions[direction] = directedAnimation;
            memStream.Seek(animationInfoOff + direction * 2, SeekOrigin.Begin);
            var directionAnimationInfoOff = reader.ReadUInt16();

            int frameId = 0;
            var lastFrame = false;
            while(true)
            {
                var frame = new SequenceDataModel.FrameInformation();
                
                memStream.Seek(directionAnimationInfoOff + frameId * 10, SeekOrigin.Begin);
                var rawFrameFlag = reader.ReadUInt16();
                var walkDx = reader.ReadUInt16();
                var walkDy = reader.ReadUInt16();
                var walkDz = reader.ReadUInt16();
                var delay = reader.ReadUInt16();
                frame.WalkDx = walkDx;
                frame.WalkDy = walkDy;
                frame.WalkDz = walkDz;
                frame.Delay = delay;
                if ((rawFrameFlag & 0x8000) > 0)
                {
                    lastFrame = true;
                }
                directedAnimation.Frames.Add(frame);

                var filteredFrameFlag = rawFrameFlag & 0x7FFF;
                memStream.Seek(characterId * 8 + 0, SeekOrigin.Begin);
                var partStartOffset = reader.ReadUInt16();
                memStream.Seek(partStartOffset + filteredFrameFlag * 2, SeekOrigin.Begin);
                var partOffset = reader.ReadUInt16();
                memStream.Seek(partOffset, SeekOrigin.Begin);
                while (true)
                {
                    var part = new SequenceDataModel.PartInformation();
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
                    
                    var vFlipped = (frameIndex & 0x8000) != 0;
                    if (direction == 3)
                    {
                        vFlipped = !vFlipped;
                    }
                    frameIndex &= 0xFFF;
                    part.DestX = dstX;
                    part.DestY = dstY;
                    part.FrameIndex = frameIndex;
                    part.VFlipped = vFlipped;

                    if (frameIndex == 0x800)
                    {
                        continue;
                    }
                    frame.Parts.Add(part);
                }

                frameId++;
                if (lastFrame)
                {
                    break;
                }
            }
        }
    }
}