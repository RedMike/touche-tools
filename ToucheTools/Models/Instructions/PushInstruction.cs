namespace ToucheTools.Models.Instructions;

public class PushInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Push;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" move STK back one and set value to 0";
    }
}