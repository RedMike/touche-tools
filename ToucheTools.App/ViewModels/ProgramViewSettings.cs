using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class ProgramViewSettings
{
    private readonly DatabaseModel _databaseModel;
    
    public int ActiveProgram { get; private set; }
    public List<int> Programs { get; private set; }
    
    
    public List<string> InstructionsView { get; private set; }

    public ProgramViewSettings(DatabaseModel model)
    {
        _databaseModel = model;
        Programs = _databaseModel.Programs.Keys.ToList();
        ActiveProgram = Programs.First();
        
        GenerateView();
    }

    public void SetActiveProgram(int program)
    {
        if (!Programs.Contains(program))
        {
            throw new Exception("Unknown program: " + program);
        }

        ActiveProgram = program;
        GenerateView();
    }

    private void GenerateView()
    {
        var program = _databaseModel.Programs[Programs[ActiveProgram]];
        InstructionsView = program.Instructions.Select(pair => pair.Value.ToString()).ToList();
    }
}