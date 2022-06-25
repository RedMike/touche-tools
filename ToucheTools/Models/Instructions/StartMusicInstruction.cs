namespace ToucheTools.Models.Instructions;

public class StartMusicInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartMusic;
    
    public ushort Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num}";
    }
}