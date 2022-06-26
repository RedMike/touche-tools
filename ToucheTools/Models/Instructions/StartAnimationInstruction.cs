namespace ToucheTools.Models.Instructions;

public class StartAnimationInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartAnimation;
    
    public ushort Character { get; set; }
    public ushort Position { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Position = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Position);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character}'s animation of ID that matches STK value in position {Position}";
    }
}