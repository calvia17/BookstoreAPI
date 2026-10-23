namespace RabbitHoleService.Dtos.Auth
{
    /// <summary>
    /// The blacklist token request DTO.
    /// </summary>
    /// <param name="JwtId">The jwt id.</param>
    /// <param name="Expiration">The expiration date.</param>
    public record BlacklistTokenRequestDto(Guid JwtId, DateTimeOffset Expiration);
}
