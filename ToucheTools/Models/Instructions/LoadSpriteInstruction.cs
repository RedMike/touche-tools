namespace ToucheTools.Models.Instructions;

public class LoadSpriteInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.LoadSprite;
    
    public ushort Index { get; set; }
    public ushort Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Index = reader.ReadUInt16();
        Num = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Index);
        writer.Write((ushort)Num);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Index} {Num}";
    }
}