namespace ToucheTools.Models.Instructions;

public class TestLowerOrEqualsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestLowerOrEquals;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check prev value lower or equals; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}