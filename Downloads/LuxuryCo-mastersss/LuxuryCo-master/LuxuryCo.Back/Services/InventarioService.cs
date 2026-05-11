using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class InventarioService : IInventarioService
{
    private readonly LuxuryCoDbContext _context;

    public InventarioService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventarioSedeDto>> GetResumenAsync(int? idSede = null)
    {
        var query = _context.InventariosSede
            .Include(i => i.Producto).ThenInclude(p => p.Imagenes)
            .Include(i => i.Sede)
            .AsQueryable();

        if (idSede.HasValue)
            query = query.Where(i => i.id_sede == idSede.Value);

        return await query
            .OrderBy(i => i.Sede.nombre)
            .ThenBy(i => i.Producto.nombre)
            .Select(i => new InventarioSedeDto
            {
                IdInventario    = i.id_inventario,
                IdProducto      = i.id_producto,
                ProductoNombre  = i.Producto.nombre,
                ProductoImagen  = i.Producto.Imagenes != null
                                    ? i.Producto.Imagenes.Where(img => img.principal).Select(img => img.url_imagen).FirstOrDefault()
                                       ?? i.Producto.Imagenes.Select(img => img.url_imagen).FirstOrDefault()
                                    : null,
                IdSede          = i.id_sede,
                SedeNombre      = i.Sede.nombre,
                CantidadDisponible = i.cantidad_disponible,
                UmbralMinimo    = i.umbral_minimo
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<InventarioSedeDto>> GetBajoStockAsync()
    {
        var todos = await GetResumenAsync();
        return todos.Where(i => i.BajoStock);
    }

    public async Task<InventarioSedeDto> AjustarStockAsync(InventarioAjusteDto dto, int idUsuario)
    {
        var registro = await _context.InventariosSede
            .FirstOrDefaultAsync(i => i.id_producto == dto.IdProducto && i.id_sede == dto.IdSede);

        if (registro == null)
        {
            registro = new InventarioSede
            {
                id_producto          = dto.IdProducto,
                id_sede              = dto.IdSede,
                cantidad_disponible  = dto.CantidadNueva,
                umbral_minimo        = dto.UmbralMinimo
            };
            _context.InventariosSede.Add(registro);
        }
        else
        {
            registro.cantidad_disponible = dto.CantidadNueva;
            registro.umbral_minimo       = dto.UmbralMinimo;
            _context.InventariosSede.Update(registro);
        }

        await _context.SaveChangesAsync();

        await _context.Entry(registro).Reference(r => r.Producto).LoadAsync();
        await _context.Entry(registro).Reference(r => r.Sede).LoadAsync();

        return new InventarioSedeDto
        {
            IdInventario       = registro.id_inventario,
            IdProducto         = registro.id_producto,
            ProductoNombre     = registro.Producto.nombre,
            IdSede             = registro.id_sede,
            SedeNombre         = registro.Sede.nombre,
            CantidadDisponible = registro.cantidad_disponible,
            UmbralMinimo       = registro.umbral_minimo
        };
    }

    public async Task<IEnumerable<AbastecimientoDto>> GetHistorialAsync(int? idSede = null, int? idProducto = null)
    {
        var query = _context.HistorialesAbastecimiento
            .Include(h => h.Producto)
            .Include(h => h.Proveedor)
            .Include(h => h.Sede)
            .AsQueryable();

        if (idSede.HasValue)    query = query.Where(h => h.id_sede == idSede.Value);
        if (idProducto.HasValue) query = query.Where(h => h.id_producto == idProducto.Value);

        return await query
            .OrderByDescending(h => h.fecha_ingreso)
            .Select(h => new AbastecimientoDto
            {
                IdAbastecimiento = h.id_abastecimiento,
                IdProducto       = h.id_producto,
                ProductoNombre   = h.Producto.nombre,
                IdProveedor      = h.id_proveedor,
                ProveedorNombre  = h.Proveedor != null ? h.Proveedor.nombre : "Sin proveedor",
                IdSede           = h.id_sede,
                SedeNombre       = h.Sede.nombre,
                Cantidad         = h.cantidad,
                CostoUnitario    = h.costo_unitario,
                FechaIngreso     = h.fecha_ingreso,
                Notas            = h.notas
            })
            .ToListAsync();
    }

    public async Task<AbastecimientoDto> RegistrarEntradaAsync(AbastecimientoCreateDto dto, int idUsuario)
    {
        // 1. Registrar en el historial
        var historial = new HistorialAbastecimiento
        {
            id_producto         = dto.IdProducto,
            id_proveedor        = dto.IdProveedor,
            id_sede             = dto.IdSede,
            id_usuario_registra = idUsuario,
            cantidad            = dto.Cantidad,
            costo_unitario      = dto.CostoUnitario,
            fecha_ingreso       = DateTime.UtcNow,
            notas               = dto.Notas
        };
        _context.HistorialesAbastecimiento.Add(historial);

        // 2. Actualizar (o crear) el inventario de esa sede
        var inventario = await _context.InventariosSede
            .FirstOrDefaultAsync(i => i.id_producto == dto.IdProducto && i.id_sede == dto.IdSede);

        if (inventario == null)
        {
            inventario = new InventarioSede
            {
                id_producto         = dto.IdProducto,
                id_sede             = dto.IdSede,
                cantidad_disponible = dto.Cantidad,
                umbral_minimo       = 5
            };
            _context.InventariosSede.Add(inventario);
        }
        else
        {
            inventario.cantidad_disponible += dto.Cantidad;
            _context.InventariosSede.Update(inventario);
        }

        await _context.SaveChangesAsync();

        await _context.Entry(historial).Reference(h => h.Producto).LoadAsync();
        await _context.Entry(historial).Reference(h => h.Sede).LoadAsync();
        if (historial.id_proveedor.HasValue)
            await _context.Entry(historial).Reference(h => h.Proveedor).LoadAsync();

        return new AbastecimientoDto
        {
            IdAbastecimiento = historial.id_abastecimiento,
            IdProducto       = historial.id_producto,
            ProductoNombre   = historial.Producto.nombre,
            IdProveedor      = historial.id_proveedor,
            ProveedorNombre  = historial.Proveedor?.nombre ?? "Sin proveedor",
            IdSede           = historial.id_sede,
            SedeNombre       = historial.Sede.nombre,
            Cantidad         = historial.cantidad,
            CostoUnitario    = historial.costo_unitario,
            FechaIngreso     = historial.fecha_ingreso,
            Notas            = historial.notas
        };
    }
}
