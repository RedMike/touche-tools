namespace ToucheTools.Models.Instructions;

public class UpdateRoomAreasInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UpdateRoomAreas;
    
    public ushort Area { get; set; }

    public override void Load(BinaryReader reader)
    {
        Area = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Area);
    }

    public override string ToString()
    {
        return $"{Opcode:G} area {Area}";
    }
}