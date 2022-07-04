namespace ToucheTools.Models.Instructions;

public class SleepInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Sleep;
    public override int Width => 2;
    
    public ushort RawCycles { get; set; }
    public int Cycles => RawCycles * 2;

    public override void Load(BinaryReader reader)
    {
        RawCycles = reader.ReadUInt16(); 
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)RawCycles);
    }

    public override string ToString()
    {
        return $"{Opcode:G} for {Cycles} cycles";
    }
}