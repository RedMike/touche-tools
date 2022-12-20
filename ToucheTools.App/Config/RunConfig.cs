namespace ToucheTools.App.Config;

public class RunConfig
{
    /// <summary>
    /// Executable called when 'Run' is clicked
    /// </summary>
    public string ExecutablePath { get; set; } = @"C:\Files\Software\ScummVM\scummvm.exe";

    /// <summary>
    /// Executable arguments, as format string where {0} is replaced with the DAT file path in double quotes
    /// </summary>
    public string ExecutableArgumentFormatString { get; set; } = @"-p {0} touche";
}