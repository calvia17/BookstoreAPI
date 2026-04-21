namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The authentication failure enum.
    /// </summary>
    public enum AuthenticationFailure
    {
        /// <summary>
        /// Invalid credentials.
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// Account locked due to multiple failed login attempts.
        /// </summary>
        AccountLocked
    }
}
