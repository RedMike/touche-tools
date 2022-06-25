namespace ToucheTools.Models.Instructions;

public class SetFlagInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetFlag;
    
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Flag = reader.ReadUInt16(); //TODO: validate flag
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Flag);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Flag} to STK value";
    }
}