using ToucheTools.Constants;
using ToucheTools.Helpers;

namespace ToucheTools.Models.Instructions;

public class SetFlagInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetFlag;
    public override int Width => 2;
    
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Flag = reader.ReadInt16().AsUshort();//game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write(Flag.AsShort());
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Flag}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        Flag = ushort.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Flags.GetFlagText(Flag)} to STK val";
    }
}