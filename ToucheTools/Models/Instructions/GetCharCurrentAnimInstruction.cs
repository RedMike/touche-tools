namespace ToucheTools.Models.Instructions;

public class GetCharCurrentAnimInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetCharCurrentAnim;
    public override int Width => 2;
    
    public short Character { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} animation to STK val";
    }
}