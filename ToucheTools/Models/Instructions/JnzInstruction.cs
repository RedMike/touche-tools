namespace ToucheTools.Models.Instructions;

public class JnzInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Jnz;
    
    public ushort NewOffset { get; set; }

    public override void Load(BinaryReader reader)
    {
        NewOffset = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)NewOffset);
    }

    public override string ToString()
    {
        return $"{Opcode:G} if STK value is not 0, jump to {NewOffset}";
    }
}