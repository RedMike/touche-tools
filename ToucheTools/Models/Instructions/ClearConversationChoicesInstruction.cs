namespace ToucheTools.Models.Instructions;

public class ClearConversationChoicesInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.ClearConversationChoices;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}