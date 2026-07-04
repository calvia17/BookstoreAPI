using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The person model dto mapper interface.
    /// </summary>
    public interface IPersonModelDtoMapper<TEntity, TDto>
    {
        /// <summary>
        /// Maps a user to its dto.
        /// </summary>
        /// <param name="user">The user model.</param>
        /// <returns>The dto.</returns>
        public abstract static TDto ToDto(TEntity user);
    }
}
