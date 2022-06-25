namespace ToucheTools.Models.Instructions;

public class MoveCharToPosInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.MoveCharToPos;
    
    public ushort Character { get; set; }
    public ushort Num { get; set; }
    public ushort TargetCharacter { get; set; }
    
    public bool CurrentCharacter => Character == 256;
    public bool TargetingAnotherCharacter => Num == ushort.MaxValue;
    
    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Num = reader.ReadUInt16();
        if (TargetingAnotherCharacter)
        {
            TargetCharacter = reader.ReadUInt16();
        }
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Num);
        if (TargetingAnotherCharacter)
        {
            writer.Write((ushort)TargetCharacter);
        }
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {(TargetingAnotherCharacter ? "to char" : "to pos")} {Num}";
    }
}