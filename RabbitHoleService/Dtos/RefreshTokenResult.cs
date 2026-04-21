namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The refresh token result.
    /// </summary>
    /// <param name="Succeeded">A value indicating whether the token refreshoperation succeeded.</param>
    /// <param name="Tokens">The tokens.</param>
    public record RefreshTokenResult(bool Succeeded, Tokens? Tokens);
}
