using System.IdentityModel.Tokens.Jwt;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The tokens.
    /// </summary>
    /// <param name="AccessToken">The access token.</param>
    /// <param name="RefreshToken">The refresh token.</param>
    public record Tokens(string AccessToken, string RefreshToken);
}
