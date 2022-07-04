namespace ToucheTools.Models.Instructions;

public class EndConversationInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.EndConversation;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}