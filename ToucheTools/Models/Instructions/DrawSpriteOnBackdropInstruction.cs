namespace ToucheTools.Models.Instructions;

public class DrawSpriteOnBackdropInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.DrawSpriteOnBackdrop;
    
    public ushort Num { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
        writer.Write((ushort)X);
        writer.Write((ushort)Y);
    }

    public override string ToString()
    {
        return $"{Opcode:G} sprite {Num} at {X}, {Y}";
    }
}