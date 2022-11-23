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
        Color = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadInt16()), 0);//game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(BitConverter.ToInt16(BitConverter.GetBytes(Color), 0));//game does it this way
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Color}";
    }
}