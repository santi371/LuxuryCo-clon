using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class MarcaService : IMarcaService
{
    private readonly LuxuryCoDbContext _context;

    public MarcaService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarcaDto>> GetAllAsync()
    {
        return await _context.Marcas
            .Select(m => new MarcaDto
            {
                IdMarca = m.id_marca,
                Nombre = m.nombre,
                Descripcion = m.descripcion ?? string.Empty,
                LogoUrl = m.logo_url ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<MarcaDto?> GetByIdAsync(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null) return null;

        return new MarcaDto
        {
            IdMarca = marca.id_marca,
            Nombre = marca.nombre,
            Descripcion = marca.descripcion ?? string.Empty,
            LogoUrl = marca.logo_url ?? string.Empty
        };
    }

    public async Task<MarcaDto> CreateAsync(MarcaDto marcaDto)
    {
        var nuevaMarca = new Marca
        {
            nombre = marcaDto.Nombre,
            descripcion = marcaDto.Descripcion,
            logo_url = marcaDto.LogoUrl
        };

        _context.Marcas.Add(nuevaMarca);
        await _context.SaveChangesAsync();

        marcaDto.IdMarca = nuevaMarca.id_marca;
        return marcaDto;
    }

    public async Task<MarcaDto> UpdateAsync(int id, MarcaDto marcaDto)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null) throw new Exception("Marca no encontrada");

        marca.nombre = marcaDto.Nombre;
        marca.descripcion = marcaDto.Descripcion;
        marca.logo_url = marcaDto.LogoUrl;

        _context.Marcas.Update(marca);
        await _context.SaveChangesAsync();

        return marcaDto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null) return false;

        _context.Marcas.Remove(marca);
        await _context.SaveChangesAsync();
        return true;
    }
}
