namespace ToucheTools.Models.Instructions;

public abstract class BaseInstruction
{
    public abstract ProgramDataModel.Opcode Opcode { get; }

    public virtual void Load(BinaryReader reader)
    {
        
    }

    public void Export(BinaryWriter writer)
    {
        writer.Write((byte)Opcode);
        ExportInternal(writer);
    }

    protected virtual void ExportInternal(BinaryWriter writer)
    {
        
    }

    public abstract override string ToString();
}