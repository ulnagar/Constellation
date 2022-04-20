using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Helpers.ModelBinders
{
    public class FlagEnumModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var propertyType = bindingContext.ModelType;

            if (propertyType.IsEnum && propertyType.GetCustomAttributes<FlagsAttribute>().Any())
            {
                var formValues = bindingContext.ActionContext.HttpContext.Request.Form;
                var propertyValues = formValues.Where(entry => entry.Key.Contains(bindingContext.ModelName)).Select(entry => entry.Value.ToString()).ToList();

                if (propertyValues.Count > 1)
                {
                    // Create flag value from posted values
                    var flagValue = propertyValues.Aggregate(0, (current, v) => current | (int)Enum.Parse(propertyType, v));

                    bindingContext.Result = ModelBindingResult.Success(flagValue);
                    //return Task.FromResult(Enum.ToObject(propertyType, flagValue));
                }

                if (propertyValues.Count == 1)
                {
                    bindingContext.Result = ModelBindingResult.Success((int)Enum.Parse(propertyType, propertyValues.First()));
                    //return Task.FromResult(Enum.ToObject(propertyType, propertyValues.First()));
                }
            }

            return Task.CompletedTask;
        }
    }
}
