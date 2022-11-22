namespace ToucheTools.Models.Instructions;

public class SetCharFrameInstruction : BaseInstruction
{
    public enum Type
    {
        Loop = 0, //val2 => anim2start, anim3start; val3 => anim2count, anim3count
        RandomCountThenStop = 1, //??
        TalkFrames = 2, //val2 => anim1start; val3 => anim1count
        StartPaused = 3, //val2 => currentanim, set speed/count to 0
        Todo4 = 4 //val2 => anim3start; val3 => anim3count
    }
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharFrame;
    public override int Width => 8;
    
    public ushort Character { get; set; }
    public ushort Val1 { get; set; }
    public ushort Val2 { get; set; }
    public ushort Val3 { get; set; }

    public Type TransitionType => (Type)Val1;

    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Val1 = reader.ReadUInt16();
        Val2 = reader.ReadUInt16();
        Val3 = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Val1);
        writer.Write((ushort)Val2);
        writer.Write((ushort)Val3);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {TransitionType} ({Val1}) {Val2} {Val3}";
    }
}