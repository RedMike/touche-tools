namespace ToucheTools.Models.Instructions;

public class JzInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Jz;
    public override int Width => 2;
    
    public ushort NewOffset { get; set; }

    public override void Load(BinaryReader reader)
    {
        NewOffset = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadInt16()), 0);//game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)NewOffset);
    }

    public override string ToString()
    {
        return $"{Opcode:G} if STK value is 0, jump to {NewOffset}";
    }
}