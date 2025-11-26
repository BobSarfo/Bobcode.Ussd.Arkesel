namespace Bobcode.Ussd.Arkesel.Models;

public class MenuOption
{
    public string Input { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? TargetStep { get; set; }
    public string? ActionKey { get; set; }

    /// <summary>
    /// Indicates this option accepts any user input (wildcard).
    /// When true, this option will match any input that doesn't match other specific options.
    /// </summary>
    public bool IsWildcard { get; set; }
}