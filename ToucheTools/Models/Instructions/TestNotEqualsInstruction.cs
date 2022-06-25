namespace ToucheTools.Models.Instructions;

public class TestNotEqualsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestNotEquals;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check not equals; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}