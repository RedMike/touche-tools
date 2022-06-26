namespace ToucheTools.Models.Instructions;

public class ModInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Mod;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" take STK value, move STK position forwards 1, then if 0 set to 0 else modulus by STK value";
    }
}