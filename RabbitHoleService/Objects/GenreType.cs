namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The genre type enum.
    /// </summary>
    public enum GenreType
    {
        /// <summary>
        /// Fantasy genre.
        /// </summary>
        Fantasy = 0,

        /// <summary>
        /// Science fiction genre.
        /// </summary>
        ScienceFiction = 1,

        /// <summary>
        /// Romance genre.
        /// </summary>
        Romance = 2,

        /// <summary>
        /// Horror genre.
        /// </summary>
        Horror = 3,

        /// <summary>
        /// Mystery genre.
        /// </summary>
        Mystery = 4,

        /// <summary>
        /// Thriller genre.
        /// </summary>
        Thriller = 5,

        /// <summary>
        /// Action genre.
        /// </summary>
        Action = 6,

        /// <summary>
        /// Adventure genre.
        /// </summary>
        Adventure = 7,

        /// <summary>
        /// Psychological thriller genre.
        /// </summary>
        PsychologicalThriller = 8,

        /// <summary>
        /// Dystopian fiction genre.
        /// </summary>
        DystopianFiction = 9,
    }
}

// Better to assign numbers even if C# can do it by default because if we add a new genre type int he middle of this list,
// the genres in teh database would point to the wrong genre.