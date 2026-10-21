using System.Text.Json.Serialization;

namespace RabbitHoleService.Events
{
    /// <summary>
    /// The book event.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
    [JsonDerivedType(typeof(BooksAddedEvent), "Added")]
    [JsonDerivedType(typeof(BookDeletedEvent), "Deleted")]
    [JsonDerivedType(typeof(BooksUpdatedEvent), "Updated")]
    [JsonDerivedType(typeof(RecommendationsRequestedEvent), "RecommendationsRequested")]
    public abstract class BookEvent
    {
    }
}
