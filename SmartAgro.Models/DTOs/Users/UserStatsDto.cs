namespace SmartAgro.Models.DTOs.Users
{
    public class UserStatsDto
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int Administradores { get; set; }
        public int Clientes { get; set; }
        public int RegistrosEsteMes { get; set; }
        public int RegistrosHoy { get; set; }
        public double PorcentajeActivos => TotalUsuarios > 0 ? (double)UsuariosActivos / TotalUsuarios * 100 : 0;
    }
}