using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

#if NETCOREAPP1_0 || NETCOREAPP2_0
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
#endif

namespace BrunoZell.ModelBinding
{
    public class JsonModelBinder : IModelBinder
    {
#if NETCOREAPP1_0 || NETCOREAPP2_0
        private readonly MvcJsonOptions _options;

        public JsonModelBinder(IOptions<MvcJsonOptions> options) =>
            _options = options.Value;
#endif

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            // Test if a value is received
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None) {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                // Deserialize from string
                string serialized = valueProviderResult.FirstValue;

                // Use custom json options defined in startup if available
#if NETCOREAPP1_0 || NETCOREAPP2_0
                object deserialized = _options?.SerializerSettings == null ?
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType) :
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType, _options.SerializerSettings);
#else
                object deserialized = JsonConvert.DeserializeObject(serialized, bindingContext.ModelType);
#endif

                // Set successful binding result
                bindingContext.Result = ModelBindingResult.Success(deserialized);
            }

            return Task.CompletedTask;
        }
    }
}
