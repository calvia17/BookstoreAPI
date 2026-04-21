namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The authentication result record.
    /// </summary>
    /// <param name="Succeeded">A value indicating whether the authentication was successful.</param>
    /// <param name="ErrorCode">The error code.</param>
    /// <param name="ErrorDescription">The error description.</param>
    /// <param name="Tokens">The tokens.</param>
    public record AuthenticationResult(bool Succeeded, AuthenticationFailure? ErrorCode, string? ErrorDescription, Tokens? Tokens);
}
