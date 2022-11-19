namespace ToucheTools.App.ViewModels;

public class LogData
{
    private const int MaxNewLineCount = 500;
    private Queue<(bool, string)> _logs = new Queue<(bool, string)>();

    public void Info(string message)
    {
        _logs.Enqueue((false, message));
        DequeueUntilBelowLimit();
    }
    
    public void Error(string message)
    {
        _logs.Enqueue((true, message));
        DequeueUntilBelowLimit();
    }

    public List<(bool, string)> List()
    {
        var list = _logs.ToList();
        list.Reverse();
        return list;
    }

    private void DequeueUntilBelowLimit()
    {
        var ok = true;
        do
        {
            var tempLogs = _logs.ToList();
            var newLineCount = tempLogs.Sum(s => s.Item2.Count(c => c == '\n') + 1);
            if (newLineCount > MaxNewLineCount)
            {
                _logs = new Queue<(bool, string)>();
                //_logs.Dequeue();
                ok = false;
            }
        } while (!ok);
    }
}