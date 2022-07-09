﻿using System.Text;
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
            var partStartOffsets = new List<int>();
            var frameStartOffsets = new Dictionary<int, SequenceDataModel.FrameInformation>();
            
            var memStream = new MemoryStream(sequence.Bytes);
            var reader = new BinaryReader(memStream);

            {
                var partStartOffs = new List<int>();
                var charToPartStartOff = new Dictionary<int, int>();
                var animOffs = new List<int>();
                var charToAnimOff = new Dictionary<int, int>(); 
                var dirOffs = new List<int>();
                var charAndAnimToDirOff = new Dictionary<(int, int), int>();
                var frameOffs = new List<int>();
                var charAndAnimAndDirToFrameOffs = new Dictionary<(int, int, int), int>();
                var partOffs = new List<int>();
                var charAndAnimAndDirAndFrameToPartOffs = new Dictionary<(int, int, int, int), int>();
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
                    charToAnimOff[characterId] = animOff;
                        

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

                    charToPartStartOff[characterId] = partStartOff;

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

                        charAndAnimToDirOff[(characterId, animationId)] = dirOff;

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
                            charAndAnimAndDirToFrameOffs[(characterId, animationId, dirId)] = frameOff;

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

                                charAndAnimAndDirAndFrameToPartOffs[(characterId, animationId, dirId, frameId)] = partOff;
                                
                                frameId++;
                            }
                        }

                        animationId++;
                    }

                    characterId++;
                }

                var frameOffToFrame = new Dictionary<int, List<SequenceDataModel.FrameInformation>>();
                foreach (var frameOff in frameOffs)
                {
                    frameOffToFrame[frameOff] = new List<SequenceDataModel.FrameInformation>();
                    
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

                        frameOffToFrame[frameOff].Add(frame);
                        frameId++;
                    }
                }

                var partOffsToPart = new Dictionary<int, List<SequenceDataModel.PartInformation>>();
                foreach (var partOff in partOffs)
                {
                    partOffsToPart[partOff] = new List<SequenceDataModel.PartInformation>();

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

                        var vFlipped = (frameIndex & 0x4000) != 0;
                        var hFlipped = (frameIndex & 0x8000) != 0;
                        // if (direction == 3)
                        // {
                        //     hFlipped = !hFlipped;
                        //     dstX = (short)-dstX;
                        // }

                        frameIndex &= 0xFFF;
                        part.DestX = dstX;
                        part.DestY = dstY;
                        part.FrameIndex = frameIndex;
                        part.VFlipped = vFlipped;
                        part.HFlipped = hFlipped;

                        if (frameIndex == 0x800)
                        {
                            continue;
                        }
                        
                        partOffsToPart[partOff].Add(part);
                        partId++;
                    }
                }

                sequence.Characters = new Dictionary<int, SequenceDataModel.CharacterInfo>();
                foreach (var charId in charAndAnimAndDirToFrameOffs.Keys.Select(i => i.Item1).Distinct())
                {
                    var ch = new SequenceDataModel.CharacterInfo();
                    //TODO:
                    sequence.Characters[charId] = ch;
                    foreach (var animId in charAndAnimAndDirToFrameOffs.Keys
                                 .Where(i => i.Item1 == charId)
                                 .Select(i => i.Item2)
                                 .Distinct())
                    {
                        var anim = new SequenceDataModel.AnimationInfo();
                        //TODO:
                        ch.Animations[animId] = anim;
                        foreach (var dirId in charAndAnimAndDirToFrameOffs.Keys
                                     .Where(i => i.Item1 == charId && i.Item2 == animId)
                                     .Select(i => i.Item3)
                                     .Distinct())
                        {
                            var dir = new SequenceDataModel.DirectedAnimationInfo();
                            //TODO:
                            anim.Directions[dirId] = dir;
                            var frameOff = charAndAnimAndDirToFrameOffs[(charId, animId, dirId)];

                            var frameId = 0;
                            foreach (var frame in frameOffToFrame[frameOff])
                            {
                                var partOff = charAndAnimAndDirAndFrameToPartOffs[(charId, animId, dirId, frameId)];

                                var parts = partOffsToPart[partOff];

                                //TODO: don't duplicate frames and parts
                                dir.Frames.Add(new SequenceDataModel.FrameInformation()
                                    {
                                        Parts = parts.Select(p => new SequenceDataModel.PartInformation()
                                        {
                                            FrameIndex = p.FrameIndex,
                                            DestX = (dirId == 3 ? ((short)-p.DestX) : p.DestX),
                                            DestY = p.DestY,
                                            HFlipped = (dirId == 3 ? !p.HFlipped : p.HFlipped),
                                            VFlipped = p.VFlipped
                                        }).ToList(),
                                        WalkDx = frame.WalkDx,
                                        WalkDy = frame.WalkDy,
                                        WalkDz = frame.WalkDz,
                                        Delay = frame.Delay,
                                    }
                                );
                                frameId++;
                            }
                        }
                    }
                }
                
                var q = 0;
            }
            
        //     {
        //     var characterId = 0; //sequence offset
        //     while (true)
        //     {
        //         var character = new SequenceDataModel.CharacterInfo();
        //
        //         memStream.Seek(characterId * 8 + 2, SeekOrigin.Begin);
        //         var characterInfoOff = reader.ReadUInt16();
        //         if (characterInfoOff == 0)
        //         {
        //             break;
        //         }
        //
        //         memStream.Seek(characterId * 8 + 4, SeekOrigin.Begin);
        //         character.FrameDirFlag = reader.ReadUInt16();
        //
        //         var animationId = 0;
        //         while (true)
        //         {
        //             var animation = new SequenceDataModel.AnimationInfo();
        //             memStream.Seek(characterInfoOff + animationId * 4, SeekOrigin.Begin);
        //             var animationInfoOff = reader.ReadUInt16();
        //             memStream.Seek(characterInfoOff + animationId * 4 + 4, SeekOrigin.Begin);
        //             var nextAnimationInfoOff = reader.ReadUInt16();
        //             if (animationInfoOff == 0)
        //             {
        //                 break;
        //             }
        //
        //             if (nextAnimationInfoOff == animationInfoOff)
        //             {
        //                 animationId++;
        //                 continue;
        //             }
        //
        //             var direction = 0; //facing
        //             while (true)
        //             {
        //                 if (direction > 3)
        //                 {
        //                     break;
        //                 }
        //
        //                 var directedAnimation = new SequenceDataModel.DirectedAnimationInfo();
        //                 memStream.Seek(animationInfoOff + direction * 2, SeekOrigin.Begin);
        //                 if (animationInfoOff + direction * 2 >= memStream.Length)
        //                 {
        //                     //TODO: should not be needed
        //                     break;
        //                 }
        //                 var directionAnimationInfoOff = reader.ReadUInt16();
        //                 if (!frameStartOffsets.ContainsKey(directionAnimationInfoOff))
        //                 {
        //                     frameStartOffsets[directionAnimationInfoOff] = new SequenceDataModel.FrameInformation();
        //                 }
        //                 else
        //                 {
        //                     directedAnimation.Frames.Add(frameStartOffsets[directionAnimationInfoOff]);
        //                     animation.Directions[direction] = directedAnimation;
        //                     
        //                     direction++;
        //                     continue;
        //                 }
        //
        //                 int frameId = 0;
        //                 var lastFrame = false;
        //                 while (true)
        //                 {
        //                     var frame = frameStartOffsets[directionAnimationInfoOff];
        //
        //                     memStream.Seek(directionAnimationInfoOff + frameId * 10, SeekOrigin.Begin);
        //                     if (directionAnimationInfoOff + frameId * 10 >= memStream.Length)
        //                     {
        //                         //TODO: should not be needed
        //                         break;
        //                     }
        //                     var rawFrameFlag = reader.ReadUInt16();
        //                     var walkDx = reader.ReadUInt16();
        //                     var walkDy = reader.ReadUInt16();
        //                     var walkDz = reader.ReadUInt16();
        //                     var delay = reader.ReadUInt16();
        //                     frame.WalkDx = walkDx;
        //                     frame.WalkDy = walkDy;
        //                     frame.WalkDz = walkDz;
        //                     frame.Delay = delay;
        //                     if ((rawFrameFlag & 0x8000) > 0)
        //                     {
        //                         lastFrame = true;
        //                     }
        //
        //                     var filteredFrameFlag = rawFrameFlag & 0x7FFF;
        //                     memStream.Seek(characterId * 8 + 0, SeekOrigin.Begin);
        //                     var partStartOffset = reader.ReadUInt16();
        //                     if (!partStartOffsets.Contains(partStartOffset))
        //                     {
        //                         partStartOffsets.Add(partStartOffset);
        //                     }
        //                     else
        //                     {
        //                         //break;
        //                     }
        //                     if (partStartOffset + filteredFrameFlag * 2 >= memStream.Length)
        //                     {
        //                         //TODO: should not be needed
        //                         break;
        //                     }
        //
        //                     memStream.Seek(partStartOffset + filteredFrameFlag * 2, SeekOrigin.Begin);
        //                     var partOffset = reader.ReadUInt16();
        //                     memStream.Seek(partOffset, SeekOrigin.Begin);
        //                     while (true)
        //                     {
        //                         if (memStream.Position >= memStream.Length - 2)
        //                         {
        //                             //TODO: should not be needed
        //                             break;
        //                         }
        //
        //                         var part = new SequenceDataModel.PartInformation();
        //                         var rawDstX = reader.ReadUInt16();
        //                         var dstX = BitConverter.ToInt16(BitConverter.GetBytes(rawDstX), 0);
        //                         if (dstX == 10000)
        //                         {
        //                             break;
        //                         }
        //
        //                         if (memStream.Position >= memStream.Length - 2)
        //                         {
        //                             //TODO: should not be needed
        //                             break;
        //                         }
        //
        //                         var rawDstY = reader.ReadUInt16();
        //                         var dstY = BitConverter.ToInt16(BitConverter.GetBytes(rawDstY), 0);
        //
        //                         if (memStream.Position >= memStream.Length - 2)
        //                         {
        //                             //TODO: should not be needed
        //                             break;
        //                         }
        //
        //                         var rawFrameIndex = reader.ReadUInt16();
        //                         var frameIndex = BitConverter.ToInt16(BitConverter.GetBytes(rawFrameIndex), 0);
        //
        //                         var vFlipped = (frameIndex & 0x4000) != 0;
        //                         var hFlipped = (frameIndex & 0x8000) != 0;
        //                         if (direction == 3)
        //                         {
        //                             hFlipped = !hFlipped;
        //                             dstX = (short)-dstX;
        //                         }
        //
        //                         frameIndex &= 0xFFF;
        //                         part.DestX = dstX;
        //                         part.DestY = dstY;
        //                         part.FrameIndex = frameIndex;
        //                         part.VFlipped = vFlipped;
        //                         part.HFlipped = hFlipped;
        //
        //                         if (frameIndex == 0x800)
        //                         {
        //                             continue;
        //                         }
        //
        //                         frame.Parts.Add(part);
        //                     }
        //
        //                     if (frame.Parts.Count > 0)
        //                     {
        //                         directedAnimation.Frames.Add(frame);
        //                     }
        //
        //                     frameId++;
        //                     if (lastFrame)
        //                     {
        //                         break;
        //                     }
        //                 }
        //
        //                 if (directedAnimation.Frames.Count > 0)
        //                 {
        //                     animation.Directions[direction] = directedAnimation;
        //                 }
        //
        //                 direction++;
        //             }
        //
        //             if (animation.Directions.Count > 0)
        //             {
        //                 character.Animations[animationId] = animation;
        //             }
        //
        //             animationId++;
        //         }
        //
        //         if (character.Animations.Count > 0)
        //         {
        //             sequence.Characters[characterId] = character;
        //         }
        //         characterId++;
        //     }
        //     }
        }
    }
}