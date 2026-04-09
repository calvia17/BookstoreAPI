using Microsoft.AspNetCore.Mvc.ModelBinding;
using RabbitHoleService.Objects;

namespace RabbitHoleService.ModelBinders
{
    /// <summary>
    /// The genre list model binder.
    /// </summary>
    public class GenreListModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model asynchronously.
        /// </summary>
        /// <param name="bindingContext">The model binding context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var values = valueProviderResult.Values;
            var genres = new HashSet<GenreType>();

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<GenreType>(value, out var genre))
                {
                    genres.Add(genre);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(genres.Count > 0 ? genres : null);
            return Task.CompletedTask;
        }
    }
}
