namespace ToucheTools.App.ViewModels;

public class LogData
{
    private List<(bool, string)> _logs = new List<(bool, string)>();

    public void Info(string message)
    {
        _logs.Add((false, message));
    }
    
    public void Error(string message)
    {
        _logs.Add((true, message));
    }

    public List<(bool, string)> List()
    {
        return _logs;
    }
}