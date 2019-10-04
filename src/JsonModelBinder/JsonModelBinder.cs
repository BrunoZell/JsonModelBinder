using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BrunoZell.ModelBinding
{
    public class JsonModelBinder : IModelBinder
    {
#if NETCOREAPP1_0 || NETCOREAPP2_0
        private readonly MvcJsonOptions _options;

        public JsonModelBinder(IOptions<MvcJsonOptions> options) =>
            _options = options.Value;
#endif

#if NETCOREAPP3_0
        private readonly MvcNewtonsoftJsonOptions _options;

        public JsonModelBinder(IOptions<MvcNewtonsoftJsonOptions> options) =>
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
                object deserialized = _options?.SerializerSettings == null ?
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType) :
                    JsonConvert.DeserializeObject(serialized, bindingContext.ModelType, _options.SerializerSettings);

                // DataAnnotation Validation. Validate Properties and Fields.
                var validationResultProps = from property in TypeDescriptor.GetProperties(deserialized).Cast<PropertyDescriptor>()
                                            from attribute in property.Attributes.OfType<ValidationAttribute>()
                                            where !attribute.IsValid(property.GetValue(deserialized))
                                            select new {
                                                Property = property.Name,
                                                ErrorMessage = attribute.FormatErrorMessage(String.Empty)
                                            };

                var validationResultFields = from field in TypeDescriptor.GetReflectionType(deserialized).GetFields().Cast<FieldInfo>()
                                             from attribute in field.GetCustomAttributes<ValidationAttribute>()
                                             where !attribute.IsValid(field.GetValue(deserialized))
                                             select new {
                                                 Property = field.Name,
                                                 ErrorMessage = attribute.FormatErrorMessage(String.Empty)
                                             };

                // Add the ValidationResult's to the ModelState
                var errors = validationResultFields.Concat(validationResultFields);
                foreach (var validationResultItem in errors)
                    bindingContext.ModelState.AddModelError(validationResultItem.Property, validationResultItem.ErrorMessage);

                // Set successful binding result
                bindingContext.Result = ModelBindingResult.Success(deserialized);
            }

            return Task.CompletedTask;
        }
    }
}
