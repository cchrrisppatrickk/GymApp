using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymApp.Controllers
{
    // [Authorize]: Nadie entra aquí (ni a sus hijos) sin Cookie válida.
    [Authorize]
    public class BaseController : Controller
    {
        // Método reutilizable para obtener el ID del usuario logueado en cualquier parte
        protected int CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
                {
                    return userId;
                }
                return 0;
            }
        }

        protected string CurrentUserName
        {
            get { return User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario"; }
        }

        protected string CurrentUserRole
        {
            get { return User.FindFirst(ClaimTypes.Role)?.Value ?? ""; }
        }
    }
}