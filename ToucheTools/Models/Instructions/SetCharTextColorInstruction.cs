using ToucheTools.Helpers;

namespace ToucheTools.Models.Instructions;

public class SetCharTextColorInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharTextColor;
    public override int Width => 4;
    
    public short Character { get; set; }
    public ushort Color { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Color = reader.ReadInt16().AsUshort(); //game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(Color.AsShort());
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Color}";
    }
}