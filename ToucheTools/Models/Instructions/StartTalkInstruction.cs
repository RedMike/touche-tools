namespace ToucheTools.Models.Instructions;

public class StartTalkInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartTalk;
    public override int Width => 4;
    
    public ushort Character { get; set; }
    public ushort Num { get; set; }

    public bool DoNothing => Num == 750;

    public bool CurrentCharacter => !DoNothing && Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Num = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Num);
    }

    public override string ToString()
    {
        if (DoNothing)
        {
            return $"{Opcode:G} do nothing";
        }
        return $"{Opcode:G} {(CurrentCharacter ? "current to next" : (Character + " to " + Num))}";
    }
}