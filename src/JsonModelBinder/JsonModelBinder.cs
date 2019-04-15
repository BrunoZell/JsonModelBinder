using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BrunoZell.ModelBinding
{
    public class JsonModelBinder : IModelBinder
    {
        private readonly MvcJsonOptions _options;

        public JsonModelBinder(IOptions<MvcJsonOptions> options) =>
            _options = options.Value;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Test if a value is received
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None) {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                // Deserialize from string
                string serialized = valueProviderResult.FirstValue;

                // Use custom json options defined in startup if available
                object deserialized = _options?.SerializerSettings == null ?
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType) :
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType, _options.SerializerSettings);

                // Set successful binding result
                bindingContext.Result = ModelBindingResult.Success(deserialized);
#if NET451
                return Task.FromResult(0);
#else
                return Task.CompletedTask;
#endif
            }
#if NET451
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
