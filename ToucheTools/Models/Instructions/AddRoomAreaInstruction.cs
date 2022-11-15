namespace ToucheTools.Models.Instructions;

public class AddRoomAreaInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.AddRoomArea;
    public override int Width => 4;
    
    public ushort Num { get; set; }
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
        Flag = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
        writer.Write((ushort)Flag);
    }

    public override string ToString()
    {
        return $"{Opcode:G} background {Num} offset flags {Flag}, {Flag+1}";
    }
}