namespace ToucheTools.Models.Instructions;

public class UpdateRoomAreasInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UpdateRoomAreas;
    public override int Width => 2;
    
    public short Area { get; set; }

    public override void Load(BinaryReader reader)
    {
        Area = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Area);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Area}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        Area = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} area {Area} with redrawing";
    }
}