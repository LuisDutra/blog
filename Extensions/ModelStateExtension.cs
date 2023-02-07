using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions;

public static class ModelStateExtension
{
    public static List<string> GetErros(this ModelStateDictionary modelState)
    {
        var result = new List<string>();
        
        foreach (var value in modelState.Values)
            result.AddRange(value.Errors.Select(error => error.ErrorMessage));

        return result;
    }
}