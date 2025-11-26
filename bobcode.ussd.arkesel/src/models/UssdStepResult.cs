namespace Bobcode.Ussd.Arkesel.Models;

public class UssdStepResult
{
    public bool ContinueSession { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NextStep { get; set; }
    public bool GoHome { get; set; }


    public static UssdStepResult Continue(string msg, string? next = null)
        => new() { Message = msg, ContinueSession = true, NextStep = next };


    public static UssdStepResult End(string msg)
        => new() { Message = msg, ContinueSession = false };


    public static UssdStepResult Home()
        => new() { GoHome = true, ContinueSession = true };

    public static UssdStepResult NavigateTo(string nodeId)
        => new() { ContinueSession = true, NextStep = nodeId };
}