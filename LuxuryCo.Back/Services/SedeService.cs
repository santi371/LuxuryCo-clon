using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class SedeService : ISedeService
{
    private readonly LuxuryCoDbContext _context;

    public SedeService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SedeDto>> GetAllAsync()
    {
        var sedes = await _context.Sedes.ToListAsync();
        return sedes.Select(s => new SedeDto
        {
            IdSede = s.id_sede,
            Nombre = s.nombre,
            Ciudad = s.ciudad,
            Direccion = s.direccion,
            Telefono = s.telefono,
            Activa = s.activa,
            FechaCreacion = s.fecha_creacion
        }).OrderBy(s => s.Nombre);
    }

    public async Task<SedeDto> GetByIdAsync(int id)
    {
        var s = await _context.Sedes.FindAsync(id);
        if (s == null) throw new Exception("Sede no encontrada");

        return new SedeDto
        {
            IdSede = s.id_sede,
            Nombre = s.nombre,
            Ciudad = s.ciudad,
            Direccion = s.direccion,
            Telefono = s.telefono,
            Activa = s.activa,
            FechaCreacion = s.fecha_creacion
        };
    }

    public async Task<SedeDto> CreateAsync(SedeCreateUpdateDto createDto)
    {
        var nuevaSede = new Sede
        {
            nombre = createDto.Nombre,
            ciudad = createDto.Ciudad,
            direccion = createDto.Direccion,
            telefono = createDto.Telefono,
            activa = createDto.Activa,
            fecha_creacion = DateTime.UtcNow
        };

        _context.Sedes.Add(nuevaSede);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(nuevaSede.id_sede);
    }

    public async Task<SedeDto> UpdateAsync(int id, SedeCreateUpdateDto updateDto)
    {
        var sede = await _context.Sedes.FindAsync(id);
        if (sede == null) throw new Exception("Sede no encontrada");

        sede.nombre = updateDto.Nombre;
        sede.ciudad = updateDto.Ciudad;
        sede.direccion = updateDto.Direccion;
        sede.telefono = updateDto.Telefono;
        sede.activa = updateDto.Activa;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> ToggleStateAsync(int id)
    {
        var sede = await _context.Sedes.FindAsync(id);
        if (sede == null) throw new Exception("Sede no encontrada");

        sede.activa = !sede.activa;
        await _context.SaveChangesAsync();
        return true;
    }
}
