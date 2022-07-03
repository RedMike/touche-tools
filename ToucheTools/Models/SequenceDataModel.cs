namespace ToucheTools.Models;

public class SequenceDataModel
{
    public class PartInformation
    {
        public short FrameIndex { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
        public bool VFlipped { get; set; }
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

    public Dictionary<int, CharacterInfo> Characters { get; set; } = new Dictionary<int, CharacterInfo>();
    
    public byte[] Bytes { get; set; }
}
