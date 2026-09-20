namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The recommendation request status.
    /// </summary>
    public enum RecommendationRequestStatus
    {
        /// <summary>
        /// The pending status.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The completed status.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// The failed status.
        /// </summary>
        Failed = 2
    }
}
