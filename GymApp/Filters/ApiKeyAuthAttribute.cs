using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GymApp.Filters;

/// <summary>
/// Filtro de autorización stateless basado en API Key.
/// Valida el header <c>X-API-KEY</c> contra el valor configurado en <c>ApiSettings:ApiKey</c>.
/// Uso: decorar el controlador o action con [ApiKeyAuth].
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Verificar que el header existe en la petición
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var receivedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = "API Key ausente. Incluya el header 'X-API-KEY' en su petición."
            });
            return;
        }

        // 2. Obtener la API Key configurada mediante DI (evita acoplar el atributo al constructor)
        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();

        var expectedApiKey = configuration["ApiSettings:ApiKey"];

        // 3. Comparación segura (evita timing attacks con comparación carácter a carácter)
        if (string.IsNullOrWhiteSpace(expectedApiKey) ||
            !CryptographicEquals(receivedApiKey!, expectedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = "API Key inválida o no autorizada."
            });
            return;
        }

        // 4. Validación exitosa → continuar con el pipeline
        await next();
    }

    /// <summary>
    /// Comparación en tiempo constante para mitigar timing attacks.
    /// </summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;

        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
}
