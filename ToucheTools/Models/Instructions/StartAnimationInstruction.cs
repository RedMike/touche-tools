namespace ToucheTools.Models.Instructions;

public class StartAnimationInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartAnimation;
    public override int Width => 4;
    
    public short Character { get; set; }
    public short Position { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Position = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Position);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character}'s animation to STK val in position {Position}";
    }
}