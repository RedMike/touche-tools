namespace ToucheTools.Models.Instructions;

public class GetFlagInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetFlag;
    
    public ushort Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Flag = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} set STK value to flag {Flag}";
    }
}