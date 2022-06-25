namespace ToucheTools.Models.Instructions;

public class TestGreaterOrEqualsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestGreaterOrEquals;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check prev value greater or equals; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}