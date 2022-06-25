using Microsoft.Extensions.Logging;
using ToucheTools.Exceptions;
using ToucheTools.Loaders;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.Helpers;

public static class ProgramInstructionHelper
{
    private static readonly ILogger Logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ProgramInstructionHelper));
    
    private static readonly Dictionary<ProgramDataModel.Opcode, Type> KnownInstructions =
        new Dictionary<ProgramDataModel.Opcode, Type>();

    static ProgramInstructionHelper()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
                a.GetTypes().Where(t =>
                        t.IsClass && !t.IsAbstract && //is a class 
                        (!t.IsGenericType || t.IsConstructedGenericType) && //that isn't a partial generic
                        typeof(BaseInstruction).IsAssignableFrom(t) //that implements the right interface
                )
            );
        var duplicateOpcodeMappings = new List<string>();
        foreach (var type in types)
        {
            //create an instance just to read the Opcode field (could be improved using decorators)
            var tmp = (BaseInstruction)Activator.CreateInstance(type)!;
            var opcode = tmp.Opcode;
            if (KnownInstructions.ContainsKey(opcode))
            {
                duplicateOpcodeMappings.Add($"{opcode:G} types {KnownInstructions[opcode]}, {type}");
            }

            Logger.Log(LogLevel.Information, "Bound opcode {} to type {}", opcode, type);
            KnownInstructions[opcode] = type;
        }

        if (duplicateOpcodeMappings.Count > 0)
        {
            throw new Exception($"Duplicate opcode mappings: {string.Join(", ", duplicateOpcodeMappings)}");
        }
    }

    public static BaseInstruction Get(ProgramDataModel.Opcode opcode)
    {
        if (!KnownInstructions.ContainsKey(opcode))
        {
            throw new UnknownProgramInstructionException(opcode);
        }

        var instance = (BaseInstruction)Activator.CreateInstance(KnownInstructions[opcode])!;
        return instance;
    }
}