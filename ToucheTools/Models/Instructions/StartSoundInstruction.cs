namespace ToucheTools.Models.Instructions;

public class StartSoundInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartSound;
    public override int Width => 4;
    
    public ushort Num { get; set; }
    public ushort Delay { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
        Delay = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
        writer.Write((ushort)Delay);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} after {Delay}";
    }
}