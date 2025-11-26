using Microsoft.AspNetCore.Mvc;
using OpenUSSD.Core;
using OpenUSSD.models;

namespace Sample;

/// <summary>
/// USSD Controller demonstrating the OpenUSSD SDK
/// </summary>
[ApiController]
[Route("[controller]")]
public class UssdController : ControllerBase
{
    private readonly UssdApp _ussdApp;

    public UssdController(UssdApp ussdApp)
    {
        _ussdApp = ussdApp;
    }

    /// <summary>
    /// Handle USSD requests from the USSD gateway
    /// </summary>
    /// <param name="request">USSD request containing sessionID, userID, msisdn, userData, etc.</param>
    /// <returns>USSD response with message and session continuation flag</returns>
    [HttpPost]
    public async Task<IActionResult> PostUSSD([FromBody] UssdRequestDto request)
    {
        var response = await _ussdApp.HandleRequestAsync(request);
        return Ok(response);
    }
}