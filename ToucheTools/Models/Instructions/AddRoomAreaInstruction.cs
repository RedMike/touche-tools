using ToucheTools.Constants;

namespace ToucheTools.Models.Instructions;

public class AddRoomAreaInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.AddRoomArea;
    public override int Width => 4;
    
    public short Num { get; set; }
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        Flag = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((ushort)Flag);
    }

    public override string ToString()
    {
        return $"{Opcode:G} background {Num} offset flags {Flags.GetFlagText(Flag)}, {Flags.GetFlagText((ushort)(Flag+1))}";
    }
}