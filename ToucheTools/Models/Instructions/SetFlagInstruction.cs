using ToucheTools.Constants;

namespace ToucheTools.Models.Instructions;

public class SetFlagInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetFlag;
    public override int Width => 2;
    
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Flag = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadInt16()), 0);//game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Flag);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Flags.GetFlagText(Flag)} to STK value";
    }
}