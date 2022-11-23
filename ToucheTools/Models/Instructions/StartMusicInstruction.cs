namespace ToucheTools.Models.Instructions;

public class StartMusicInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartMusic;
    public override int Width => 2;
    
    public short Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num}";
    }
}