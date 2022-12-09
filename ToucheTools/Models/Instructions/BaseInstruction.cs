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

    public string Serialise()
    {
        return $"{Opcode:G} {SerialiseInternal()}";
    }

    public static (ProgramDataModel.Opcode, string) DeserialiseOpcode(string s)
    {
        var index = s.IndexOf(' ');
        var opcode = s.Substring(0, index);
        var remainder = s.Substring(index);
        return (Enum.Parse<ProgramDataModel.Opcode>(opcode), remainder);
    }

    protected virtual string SerialiseInternal()
    {
        return "";
    }

    public virtual void DeserialiseRemainder(string remainder)
    {
        
    }

    public abstract override string ToString();
}