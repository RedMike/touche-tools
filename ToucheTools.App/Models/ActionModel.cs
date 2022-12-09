﻿using ToucheTools.Models.Instructions;

namespace ToucheTools.App.Models;

public class ActionModel
{
    //TODO: higher level scripting language
    public Dictionary<string, int> Labels { get; set; } = new Dictionary<string, int>(); //points into list of instructions
    public List<BaseInstruction> Instructions { get; set; } = new List<BaseInstruction>();
}