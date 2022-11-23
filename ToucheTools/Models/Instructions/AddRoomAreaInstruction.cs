using ToucheTools.Constants;
using ToucheTools.Helpers;

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
        Flag = reader.ReadInt16().AsUshort(); //game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write(Flag.AsShort());
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} offset from flags {Flag}, {(Flag+1)}";
    }
}