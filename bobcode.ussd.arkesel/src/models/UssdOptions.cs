namespace Bobcode.Ussd.Arkesel.Models;

/// <summary>
/// Configuration options for USSD application behavior
/// </summary>
public class UssdOptions
{
    /// <summary>
    /// Session timeout duration. Default is 2 minutes.
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Message displayed when user enters an invalid option
    /// </summary>
    public string InvalidInputMessage { get; set; } = "Invalid option. Please try again.";

    /// <summary>
    /// Default message displayed when session ends without specific message
    /// </summary>
    public string DefaultEndMessage { get; set; } = "Thank you for using our service.";

    /// <summary>
    /// Command to navigate back to previous menu (e.g., "0")
    /// </summary>
    public string BackCommand { get; set; } = "0";

    /// <summary>
    /// Command to navigate to home/root menu (e.g., "00")
    /// </summary>
    public string HomeCommand { get; set; } = "00";

    /// <summary>
    /// Enable pagination for long menu lists
    /// </summary>
    public bool EnablePagination { get; set; } = false;

    /// <summary>
    /// Number of items to display per page when pagination is enabled
    /// </summary>
    public int ItemsPerPage { get; set; } = 5;

    /// <summary>
    /// Command to navigate to next page (e.g., "#")
    /// </summary>
    public string NextPageCommand { get; set; } = "#";

    /// <summary>
    /// Command to navigate to previous page (e.g., "*")
    /// </summary>
    public string PreviousPageCommand { get; set; } = "*";

    /// <summary>
    /// Enable automatic back navigation handling
    /// </summary>
    public bool EnableAutoBackNavigation { get; set; } = true;

    /// <summary>
    /// Enable session resumption. When true, if a new session request comes in
    /// but an existing session exists, the user will be asked whether to resume
    /// the existing session or start fresh.
    /// </summary>
    public bool EnableSessionResumption { get; set; } = false;

    /// <summary>
    /// Header message displayed when asking user to resume or start fresh
    /// </summary>
    public string ResumeSessionPrompt { get; set; } = "You have an active session.";

    /// <summary>
    /// Label for the resume option (default: "Resume")
    /// </summary>
    public string ResumeOptionLabel { get; set; } = "Resume";

    /// <summary>
    /// Label for the start fresh option (default: "Start Fresh")
    /// </summary>
    public string StartFreshOptionLabel { get; set; } = "Start Fresh";
}

