// SmartAgro.API/Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAgro.Models.DTOs.Users;
using SmartAgro.Models.DTOs;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<PaginatedUsersDto> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? roleFilter = null, bool? isActive = null)
        {
            var query = _userManager.Users.AsQueryable();

            // Filtro por término de búsqueda
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Nombre.Contains(searchTerm) ||
                                        u.Apellidos.Contains(searchTerm) ||
                                        u.Email!.Contains(searchTerm));
            }

            // Filtro por estado activo
            if (isActive.HasValue)
            {
                query = query.Where(u => u.Activo == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var users = await query
                .OrderBy(u => u.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    Email = u.Email!,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    FechaRegistro = u.FechaRegistro,
                    Activo = u.Activo
                })
                .ToListAsync();

            // Obtener roles para cada usuario
            foreach (var user in users)
            {
                var usuario = await _userManager.FindByIdAsync(user.Id);
                if (usuario != null)
                {
                    user.Roles = (await _userManager.GetRolesAsync(usuario)).ToList();
                }
            }

            // Filtro por rol (después de obtener los roles)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => u.Roles.Contains(roleFilter)).ToList();
                totalCount = users.Count; // Recalcular el total después del filtro por rol
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            }

            return new PaginatedUsersDto
            {
                Users = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        public async Task<UserListDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserListDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellidos = user.Apellidos,
                Email = user.Email!,
                Telefono = user.Telefono,
                Direccion = user.Direccion,
                FechaRegistro = user.FechaRegistro,
                Activo = user.Activo,
                Roles = roles.ToList()
            };
        }

        public async Task<ServiceResult> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return ServiceResult.ErrorResult("El email ya está registrado");
                }

                // Verificar si el rol existe
                var roleExists = await _roleManager.RoleExistsAsync(createUserDto.Rol);
                if (!roleExists)
                {
                    return ServiceResult.ErrorResult("El rol especificado no existe");
                }

                var user = new Usuario
                {
                    UserName = createUserDto.Email,
                    Email = createUserDto.Email,
                    Nombre = createUserDto.Nombre,
                    Apellidos = createUserDto.Apellidos,
                    Telefono = createUserDto.Telefono,
                    Direccion = createUserDto.Direccion,
                    Activo = createUserDto.Activo,
                    EmailConfirmed = true, // Confirmar email automáticamente
                    FechaRegistro = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!result.Succeeded)
                {
                    return ServiceResult.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // Asignar rol
                var roleResult = await _userManager.AddToRoleAsync(user, createUserDto.Rol);
                if (!roleResult.Succeeded)
                {
                    // Si falla la asignación del rol, eliminar el usuario creado
                    await _userManager.DeleteAsync(user);
                    return ServiceResult.ErrorResult($"Error al asignar el rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }

                return ServiceResult.SuccessResult("Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult("Usuario no encontrado");
                }

                // Verificar si el email ya está en uso por otro usuario
                var existingUser = await _userManager.FindByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return ServiceResult.ErrorResult("El email ya está en uso por otro usuario");
                }

                // Actualizar datos del usuario
                user.Nombre = updateUserDto.Nombre;
                user.Apellidos = updateUserDto.Apellidos;
                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
                user.Telefono = updateUserDto.Telefono;
                user.Direccion = updateUserDto.Direccion;
                user.Activo = updateUserDto.Activo;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ServiceResult.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // Actualizar roles
                if (updateUserDto.Roles != null && updateUserDto.Roles.Any())
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    // Remover roles actuales
                    if (currentRoles.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeResult.Succeeded)
                        {
                            return ServiceResult.ErrorResult($"Error al remover roles actuales: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    // Verificar que los nuevos roles existen
                    foreach (var role in updateUserDto.Roles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            return ServiceResult.ErrorResult($"El rol '{role}' no existe");
                        }
                    }

                    // Agregar nuevos roles
                    var addResult = await _userManager.AddToRolesAsync(user, updateUserDto.Roles);
                    if (!addResult.Succeeded)
                    {
                        return ServiceResult.ErrorResult($"Error al asignar nuevos roles: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                    }
                }

                return ServiceResult.SuccessResult("Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult("Usuario no encontrado");
                }

                // Verificar si es el último administrador
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains("Admin"))
                {
                    var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                    if (allAdmins.Count <= 1)
                    {
                        return ServiceResult.ErrorResult("No se puede eliminar el último administrador del sistema");
                    }
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return ServiceResult.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return ServiceResult.SuccessResult("Usuario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ToggleUserStatusAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult("Usuario no encontrado");
                }

                // Si se intenta desactivar un usuario activo que es admin, verificar que no sea el último
                if (user.Activo)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains("Admin"))
                    {
                        // Contar administradores activos (excluyendo el usuario actual)
                        var allUsers = await _userManager.Users.Where(u => u.Activo && u.Id != userId).ToListAsync();
                        var activeAdminCount = 0;

                        foreach (var otherUser in allUsers)
                        {
                            var otherUserRoles = await _userManager.GetRolesAsync(otherUser);
                            if (otherUserRoles.Contains("Admin"))
                            {
                                activeAdminCount++;
                            }
                        }

                        if (activeAdminCount == 0)
                        {
                            return ServiceResult.ErrorResult("No se puede desactivar el último administrador activo del sistema");
                        }
                    }
                }

                // Cambiar el estado
                user.Activo = !user.Activo;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return ServiceResult.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                var status = user.Activo ? "activado" : "desactivado";
                return ServiceResult.SuccessResult($"Usuario {status} exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error al cambiar estado del usuario: {ex.Message}");
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(string userId, ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult("Usuario no encontrado");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return ServiceResult.ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return ServiceResult.SuccessResult("Contraseña restablecida exitosamente");
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error al restablecer contraseña: {ex.Message}");
            }
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            try
            {
                var totalUsuarios = await _userManager.Users.CountAsync();
                var usuariosActivos = await _userManager.Users.CountAsync(u => u.Activo);
                var usuariosInactivos = totalUsuarios - usuariosActivos;

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var registrosEsteMes = await _userManager.Users
                    .CountAsync(u => u.FechaRegistro >= inicioMes);

                var hoy = DateTime.Today;
                var registrosHoy = await _userManager.Users
                    .CountAsync(u => u.FechaRegistro.Date == hoy);

                // Contar usuarios por rol de manera más eficiente
                var administradores = 0;
                var clientes = 0;

                var adminRole = await _roleManager.FindByNameAsync("Admin");
                var clienteRole = await _roleManager.FindByNameAsync("Cliente");

                if (adminRole != null)
                {
                    administradores = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                }

                if (clienteRole != null)
                {
                    clientes = (await _userManager.GetUsersInRoleAsync("Cliente")).Count;
                }

                return new UserStatsDto
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = usuariosActivos,
                    UsuariosInactivos = usuariosInactivos,
                    Administradores = administradores,
                    Clientes = clientes,
                    RegistrosEsteMes = registrosEsteMes,
                    RegistrosHoy = registrosHoy
                };
            }
            catch (Exception ex)
            {
                // En caso de error, devolver estadísticas vacías
                return new UserStatsDto
                {
                    TotalUsuarios = 0,
                    UsuariosActivos = 0,
                    UsuariosInactivos = 0,
                    Administradores = 0,
                    Clientes = 0,
                    RegistrosEsteMes = 0,
                    RegistrosHoy = 0
                };
            }
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            try
            {
                return await _roleManager.Roles
                    .Select(r => r.Name!)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // En caso de error, devolver lista vacía
                return new List<string>();
            }
        }
    }
}