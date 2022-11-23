namespace ToucheTools.Models.Instructions;

public class DrawSpriteOnBackdropInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.DrawSpriteOnBackdrop;
    public override int Width => 6;
    
    public short Num { get; set; }
    public short X { get; set; }
    public short Y { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        X = reader.ReadInt16();
        Y = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((short)X);
        writer.Write((short)Y);
    }

    public override string ToString()
    {
        return $"{Opcode:G} index {Num} at {X}, {Y}";
    }
}