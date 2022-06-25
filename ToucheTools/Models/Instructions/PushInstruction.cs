namespace ToucheTools.Models.Instructions;

public class PushInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Push;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" move STK back one and set value to 0";
    }
}