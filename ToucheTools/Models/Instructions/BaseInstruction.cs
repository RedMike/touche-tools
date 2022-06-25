namespace ToucheTools.Models.Instructions;

public abstract class BaseInstruction
{
    public abstract ProgramDataModel.Opcode Opcode { get; }

    public virtual void Load(BinaryReader reader)
    {
        
    }

    public virtual void Export(BinaryWriter writer)
    {
        
    }

    public abstract override string ToString();
}