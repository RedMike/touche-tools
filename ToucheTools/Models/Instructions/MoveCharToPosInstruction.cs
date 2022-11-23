namespace ToucheTools.Models.Instructions;

public class MoveCharToPosInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.MoveCharToPos;
    public override int Width => TargetingAnotherCharacter ? 6 : 4;
    
    public short Character { get; set; }
    public short Num { get; set; }
    public short TargetCharacter { get; set; }
    
    public bool CurrentCharacter => Character == 256;
    public bool TargetingAnotherCharacter => Num == -1;
    
    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Num = reader.ReadInt16();
        if (TargetingAnotherCharacter)
        {
            TargetCharacter = reader.ReadInt16();
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