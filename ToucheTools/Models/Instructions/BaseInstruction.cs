namespace ToucheTools.Models.Instructions;

public abstract class BaseInstruction
{
    public abstract ProgramDataModel.Opcode Opcode { get; }
    public abstract int Width { get; } //in bytes

    public virtual void Load(BinaryReader reader)
    {
        
    }

    public int Export(BinaryWriter writer)
    {
        writer.Write((byte)Opcode);
        ExportInternal(writer);
        return 1 + Width;
    }

    protected virtual void ExportInternal(BinaryWriter writer)
    {
        
    }

    public abstract override string ToString();
}