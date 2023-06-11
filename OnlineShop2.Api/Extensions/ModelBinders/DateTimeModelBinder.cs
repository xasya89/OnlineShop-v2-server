using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace OnlineShop2.Api.Extensions.ModelBinders
{
    public class DateTimeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(DateTime) ||
                context.Metadata.ModelType == typeof(DateTime?))
            {
                return new DateTimeModelBinder();
            }

            return null;
        }
    }
    public class DateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var dateStr = valueProviderResult.FirstValue;

            if (!DateTime.TryParse(dateStr, new CultureInfo("ru-RU"), DateTimeStyles.None, out DateTime date))
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "DateTime should be in format 'dd.MM.yyyy HH:mm:ss'");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(date);
            return Task.CompletedTask;
        }
    }
}
