namespace ToucheTools.App.Config;

public class RunConfig
{
    /// <summary>
    /// Executable called when 'Run' is clicked.
    /// Example for ScummVM: C:\\Files\\Software\\ScummVM\\scummvm.exe
    /// </summary>
    public string ExecutablePath { get; set; } = "";

    /// <summary>
    /// Executable arguments, as format string where {0} is replaced with the DAT file path in double quotes
    /// Example for ScummVM: -p {0} touche:touche
    /// </summary>
    public string ExecutableArgumentFormatString { get; set; } = "";
}