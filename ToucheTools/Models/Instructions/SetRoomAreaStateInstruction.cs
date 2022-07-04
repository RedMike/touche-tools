namespace ToucheTools.Models.Instructions;

public class SetRoomAreaStateInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetRoomAreaState;
    public override int Width => 4;
    
    public ushort Num { get; set; }
    public ushort Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
        Val = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
        writer.Write((ushort)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} {Val}";
    }
}