using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class ProveedorService : IProveedorService
{
    private readonly LuxuryCoDbContext _context;

    public ProveedorService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProveedorDto>> GetAllAsync()
    {
        return await _context.Proveedores
            .OrderBy(p => p.nombre)
            .Select(p => new ProveedorDto
            {
                IdProveedor = p.id_proveedor,
                Nombre = p.nombre,
                Contacto = p.contacto,
                Telefono = p.telefono,
                Email = p.email,
                Activo = p.activo
            })
            .ToListAsync();
    }

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        var p = await _context.Proveedores.FindAsync(id);
        if (p == null) return null;

        return new ProveedorDto
        {
            IdProveedor = p.id_proveedor,
            Nombre = p.nombre,
            Contacto = p.contacto,
            Telefono = p.telefono,
            Email = p.email,
            Activo = p.activo
        };
    }

    public async Task<ProveedorDto> CreateAsync(ProveedorCreateUpdateDto dto)
    {
        var proveedor = new Proveedor
        {
            nombre = dto.Nombre,
            contacto = dto.Contacto,
            telefono = dto.Telefono,
            email = dto.Email,
            activo = true
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return new ProveedorDto
        {
            IdProveedor = proveedor.id_proveedor,
            Nombre = proveedor.nombre,
            Contacto = proveedor.contacto,
            Telefono = proveedor.telefono,
            Email = proveedor.email,
            Activo = proveedor.activo
        };
    }

    public async Task<ProveedorDto> UpdateAsync(int id, ProveedorCreateUpdateDto dto)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) throw new Exception("Proveedor no encontrado");

        proveedor.nombre = dto.Nombre;
        proveedor.contacto = dto.Contacto;
        proveedor.telefono = dto.Telefono;
        proveedor.email = dto.Email;

        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();

        return new ProveedorDto
        {
            IdProveedor = proveedor.id_proveedor,
            Nombre = proveedor.nombre,
            Contacto = proveedor.contacto,
            Telefono = proveedor.telefono,
            Email = proveedor.email,
            Activo = proveedor.activo
        };
    }

    public async Task<bool> ToggleActivoAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return false;

        proveedor.activo = !proveedor.activo;
        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return false;

        // Borrado lógico (desactivar)
        proveedor.activo = false;
        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();
        return true;
    }
}
