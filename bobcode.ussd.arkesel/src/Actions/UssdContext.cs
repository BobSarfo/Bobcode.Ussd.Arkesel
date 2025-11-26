using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Actions;

public class UssdContext
{
    public UssdRequestDto Request { get; set; }
    public UssdSession Session { get; set; }
    public string? ContextActionKey { get; set; }


    public UssdContext(UssdRequestDto req,UssdSession session)
    {
        Request = req; Session = session;
    }
}