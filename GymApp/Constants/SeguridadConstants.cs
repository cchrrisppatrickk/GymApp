namespace GymApp.Constants
{
    public static class TipoClaim
    {
        public const string Permiso = "Permiso";
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Recepcionista = "Recepcionista";
        public const string Entrenador = "Entrenador";
    }

    public static class AppPermisos
    {
        public const string AdminAccesoTotal = "AdminAccesoTotal";

        // Pagos
        public const string PagosVer = "Pagos.Ver";
        public const string PagosCrear = "Pagos.Crear";
        public const string PagosEditar = "Pagos.Editar";
        public const string PagosAnular = "Pagos.Anular";

        // Membresias
        public const string MembresiasVer = "Membresias.Ver";
        public const string MembresiasCrear = "Membresias.Crear";
        public const string MembresiasEditar = "Membresias.Editar";
        public const string MembresiasEliminar = "Membresias.Eliminar";

        // Ventas
        public const string VentasVer = "Ventas.Ver";
        public const string VentasCrear = "Ventas.Crear";
        public const string VentasEliminar = "Ventas.Eliminar";
        public const string VentasAnular = "Ventas.Anular";

        // Productos
        public const string ProductosVer = "Productos.Ver";
        public const string ProductosCrear = "Productos.Crear";
        public const string ProductosEditar = "Productos.Editar";
        public const string ProductosEliminar = "Productos.Eliminar";

        // Planes
        public const string PlanesVer = "Planes.Ver";
        public const string PlanesCrear = "Planes.Crear";
        public const string PlanesEditar = "Planes.Editar";
        public const string PlanesEliminar = "Planes.Eliminar";

        // Turnos
        public const string TurnosVer = "Turnos.Ver";
        public const string TurnosCrear = "Turnos.Crear";
        public const string TurnosEditar = "Turnos.Editar";
        public const string TurnosEliminar = "Turnos.Eliminar";
        public const string TurnosAnular = "Turnos.Anular";

        // Congelamientos
        public const string CongelamientosVer = "Congelamientos.Ver";
        public const string CongelamientosCrear = "Congelamientos.Crear";
        public const string CongelamientosEliminar = "Congelamientos.Eliminar";
        public const string CongelamientosAnular = "Congelamientos.Anular";

        // Acceso
        public const string AccesoVer = "Acceso.Ver";
        public const string AccesoEliminar = "Acceso.Eliminar";

        // Pases Diarios
        public const string PasesDiariosVer = "PasesDiarios.Ver";
        public const string PasesDiariosCrear = "PasesDiarios.Crear";
        public const string PasesDiariosEliminar = "PasesDiarios.Eliminar";
        public const string PasesDiariosAnular = "PasesDiarios.Anular";

        // Usuarios
        public const string UsuariosVer = "Usuarios.Ver";
        public const string UsuariosCrear = "Usuarios.Crear";
        public const string UsuariosEditar = "Usuarios.Editar";
        public const string UsuariosEliminar = "Usuarios.Eliminar";

        // Roles
        public const string RolesVer = "Roles.Ver";
        public const string RolesCrear = "Roles.Crear";
        public const string RolesEditar = "Roles.Editar";
        public const string RolesEliminar = "Roles.Eliminar";

        // Dashboard
        public const string DashboardVer = "Dashboard.Ver";
    }

    public static class AppPoliticas
    {
        // Pagos
        public const string RequiereVerPagos = "RequiereVerPagos";
        public const string RequiereCrearPagos = "RequiereCrearPagos";
        public const string RequiereEditarPagos = "RequiereEditarPagos";
        public const string RequiereAnularPagos = "RequiereAnularPagos";

        // Membresias
        public const string RequiereVerMembresias = "RequiereVerMembresias";
        public const string RequiereCrearMembresias = "RequiereCrearMembresias";
        public const string RequiereEditarMembresias = "RequiereEditarMembresias";
        public const string RequiereEliminarMembresias = "RequiereEliminarMembresias";

        // Ventas
        public const string RequiereVerVentas = "RequiereVerVentas";
        public const string RequiereEliminarVentas = "RequiereEliminarVentas";

        // Productos
        public const string RequiereVerProductos = "RequiereVerProductos";
        public const string RequiereEliminarProductos = "RequiereEliminarProductos";

        // Planes
        public const string RequiereVerPlanes = "RequiereVerPlanes";
        public const string RequiereEliminarPlanes = "RequiereEliminarPlanes";

        // Turnos
        public const string RequiereVerTurnos = "RequiereVerTurnos";
        public const string RequiereEliminarTurnos = "RequiereEliminarTurnos";

        // Congelamientos
        public const string RequiereVerCongelamientos = "RequiereVerCongelamientos";
        public const string RequiereCrearCongelamientos = "RequiereCrearCongelamientos";
        public const string RequiereEliminarCongelamientos = "RequiereEliminarCongelamientos";

        // Acceso
        public const string RequiereVerAcceso = "RequiereVerAcceso";
        public const string RequiereEliminarAcceso = "RequiereEliminarAcceso";

        // Pases Diarios
        public const string RequiereVerPasesDiarios = "RequiereVerPasesDiarios";
        public const string RequiereEliminarPasesDiarios = "RequiereEliminarPasesDiarios";

        // Usuarios
        public const string RequiereVerUsuarios = "RequiereVerUsuarios";
        public const string RequiereCrearUsuarios = "RequiereCrearUsuarios";
        public const string RequiereEditarUsuarios = "RequiereEditarUsuarios";
        public const string RequiereEliminarUsuarios = "RequiereEliminarUsuarios";

        // Roles
        public const string RequiereVerRoles = "RequiereVerRoles";
        public const string RequiereEliminarRoles = "RequiereEliminarRoles";

        // Dashboard
        public const string RequiereVerDashboard = "RequiereVerDashboard";
    }
}
