﻿namespace ToucheTools.Models.Instructions;

public class StartTalkInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartTalk;
    public override int Width => 4;
    
    public short Character { get; set; }
    public short Num { get; set; }

    public bool DoNothing => Num == 750;

    public bool CurrentCharacter => !DoNothing && Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Num);
    }

    public override string ToString()
    {
        if (DoNothing)
        {
            return $"{Opcode:G} do nothing";
        }
        return $"{Opcode:G} {(CurrentCharacter ? "current to next" : (Character + " to " + Num))}";
    }
}