namespace ToucheTools.Models;

public class SequenceDataModel
{
    public class PartInformation
    {
        public short RawFrameIndex { get; set; }
        public short FrameIndex => (short)(RawFrameIndex & 0xFFF);
        public int DestX { get; set; }
        public int DestY { get; set; }
        public bool VFlipped => (RawFrameIndex & 0x4000) != 0;
        public bool HFlipped => (RawFrameIndex & 0x8000) != 0;
    }

    public class FrameInformation
    {
        public List<PartInformation> Parts { get; set; } = new List<PartInformation>();
        public int WalkDx { get; set; }
        public int WalkDy { get; set; }
        public int WalkDz { get; set; }
        public int Delay { get; set; }
    }

    public class DirectedAnimationInfo
    {
        public List<FrameInformation> Frames { get; set; } = new List<FrameInformation>();
    }

    public class AnimationInfo
    {
        public Dictionary<int, DirectedAnimationInfo> Directions { get; set; } = new Dictionary<int, DirectedAnimationInfo>();
    }

    public class CharacterInfo
    {
        public Dictionary<int, AnimationInfo> Animations { get; set; } = new Dictionary<int, AnimationInfo>();
        public ushort FrameDirFlag { get; set; } //TODO
    }

    public Dictionary<int, CharacterInfo> Characters
    {
        get
        {
            if (!_updatedCharacters)
            {
                UpdateCharacters();
                _updatedCharacters = true;
            }

            return _characters;
        }
        private set => _characters = value;
    }

    private Dictionary<int, CharacterInfo> _characters = null!;
    private bool _updatedCharacters = false; //TODO: dirty on other set

    private void UpdateCharacters()
    {
        var chars = new Dictionary<int, SequenceDataModel.CharacterInfo>();
        foreach (var charId in FrameMappings.Keys.Select(i => i.Item1).Distinct())
        {
            var ch = new SequenceDataModel.CharacterInfo();
            ch.FrameDirFlag = CharToFrameFlag[charId];
            chars[charId] = ch;
            foreach (var animId in FrameMappings.Keys
                         .Where(i => i.Item1 == charId)
                         .Select(i => i.Item2)
                         .Distinct())
            {
                var anim = new SequenceDataModel.AnimationInfo();
                ch.Animations[animId] = anim;
                foreach (var dirId in FrameMappings.Keys
                             .Where(i => i.Item1 == charId && i.Item2 == animId)
                             .Select(i => i.Item3)
                             .Distinct())
                {
                    var dir = new SequenceDataModel.DirectedAnimationInfo();
                    anim.Directions[dirId] = dir;
                    var frameOff = FrameMappings[(charId, animId, dirId)];

                    var frameId = 0;
                    foreach (var frame in Frames[frameOff])
                    {
                        var partOff = PartMappings[(charId, animId, dirId, frameId)];

                        var parts = Parts[partOff];

                        dir.Frames.Add(new SequenceDataModel.FrameInformation()
                            {
                                Parts = parts.Select(p =>
                                {
                                    var frameIndex = p.RawFrameIndex;
                                    var destX = p.DestX;
                                    if (dirId == 3)
                                    {
                                        if (p.HFlipped)
                                        {
                                            frameIndex = (short)(frameIndex & 0x7FFF);
                                        }
                                        else
                                        {
                                            frameIndex = (short)(frameIndex | 0x8000);
                                        }

                                        destX = (short)-destX;
                                    }
                                    return new SequenceDataModel.PartInformation()
                                    {
                                        RawFrameIndex = frameIndex,
                                        DestX = destX,
                                        DestY = p.DestY,
                                    };
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

        Characters = chars;
    }
    
    public Dictionary<(int, int, int), int> FrameMappings { get; set; } = new Dictionary<(int, int, int), int>();
    public Dictionary<(int, int, int, int), int> PartMappings { get; set; } = new Dictionary<(int, int, int, int), int>();
    public Dictionary<int, ushort> CharToFrameFlag { get; set; } = new Dictionary<int, ushort>();
    public Dictionary<int, List<SequenceDataModel.FrameInformation>> Frames { get; set; } = new Dictionary<int, List<FrameInformation>>();
    public Dictionary<int, List<SequenceDataModel.PartInformation>> Parts { get; set; } = new Dictionary<int, List<PartInformation>>();

    public int GetExpectedSize()
    {
        var charCount = FrameMappings.Select(i => (i.Key.Item1, i.Value)).Distinct().Count();
        var animCount = FrameMappings.Select(i => (i.Key.Item1, i.Key.Item2, i.Value)).Distinct().Count();
        var dirCount = FrameMappings.Select(i => (i.Key.Item1, i.Key.Item2, i.Key.Item3, i.Value)).Distinct().Count();
        var frameCount = PartMappings.Select(i => (i.Key.Item1, i.Key.Item2, i.Key.Item3, i.Key.Item4, i.Value))
            .Distinct().Count();

        var pointerSegmentSize = charCount * 8 + 2 + animCount * 4 + 2 + dirCount * 2 + frameCount * 2;
        var dataSegmentSize = Frames.Select(i => i.Value.Count).Sum() * 10 + 2 +
                              Parts.Select(i => i.Value.Count).Sum() * 6;

        return pointerSegmentSize + dataSegmentSize;
    }
    
    public byte[] Bytes { get; set; }
}
