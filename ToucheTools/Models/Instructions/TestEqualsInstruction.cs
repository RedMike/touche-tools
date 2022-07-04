namespace ToucheTools.Models.Instructions;

public class TestEqualsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestEquals;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check equals; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}