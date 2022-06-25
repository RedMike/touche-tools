namespace ToucheTools.Models.Instructions;

public class AddRoomAreaInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.AddRoomArea;
    
    public ushort Num { get; set; }
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
        Flag = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} room {Num} position from flags {Flag}, {Flag+1}";
    }
}