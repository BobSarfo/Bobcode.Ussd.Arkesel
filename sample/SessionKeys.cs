using Bobcode.Ussd.Arkesel.Models;

namespace Bobcode.Ussd.Arkesel.Sample;

/// <summary>
/// Strongly-typed session keys for the demo bank application
/// </summary>
public static class SessionKeys
{
    public static SessionKey<string?> Recipient => new("recipient");
    public static SessionKey<decimal> Amount => new("amount");
    public static SessionKey<string?> TransferStep => new("transfer_step");
    public static SessionKey<string?> Vote => new("vote");
    public static SessionKey<DateTime> Timestamp => new("timestamp");
    public static SessionKey<DateTime> LastBalanceCheck => new("last_balance_check");
}

