namespace ToucheTools.Models.Instructions;

public class TestLowerInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestLower;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check prev value lower; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}