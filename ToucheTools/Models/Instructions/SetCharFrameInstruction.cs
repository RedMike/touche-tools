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
    
    public short Character { get; set; }
    public short Val1 { get; set; }
    public short Val2 { get; set; }
    public short Val3 { get; set; }

    public Type TransitionType => (Type)Val1;

    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Val1 = reader.ReadInt16();
        Val2 = reader.ReadInt16();
        Val3 = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Val1);
        writer.Write((short)Val2);
        writer.Write((short)Val3);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {TransitionType} ({Val1}) {Val2} {Val3}";
    }
}