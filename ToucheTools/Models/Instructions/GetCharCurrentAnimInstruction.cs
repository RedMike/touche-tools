namespace ToucheTools.Models.Instructions;

public class GetCharCurrentAnimInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetCharCurrentAnim;
    public override int Width => 2;
    
    public ushort Character { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
    }

    public override string ToString()
    {
        return $"{Opcode:G} load {Character} current animation into STK value";
    }
}