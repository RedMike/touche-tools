namespace ToucheTools.Models.Instructions;

public class SetRoomAreaStateInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetRoomAreaState;
    public override int Width => 4;
    
    public short Num { get; set; }
    public short Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        Val = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((short)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} {Val}";
    }
}