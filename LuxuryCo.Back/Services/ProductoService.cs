using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LuxuryCo.Back.Services;

public class ProductoService : IProductoService
{
    private readonly LuxuryCoDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductoService(LuxuryCoDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync(bool adminView = false)
    {
        // Usamos proyección directa para evitar SELECT * en Marca (logo_url puede no existir aún)
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Imagenes.Where(i => i.principal == true))
            .AsQueryable();

        if (!adminView)
            query = query.Where(p => p.activo == true);

        var productos = await query.ToListAsync();

        // Cargar nombres de marcas de forma segura (sin traer logo_url)
        var marcaIds = productos.Where(p => p.id_marca.HasValue).Select(p => p.id_marca.Value).Distinct().ToList();
        var marcas = await _context.Marcas
            .Where(m => marcaIds.Contains(m.id_marca))
            .Select(m => new { m.id_marca, m.nombre })
            .ToListAsync();
        var marcaMap = marcas.ToDictionary(m => m.id_marca, m => m.nombre);

        return productos.Select(p => new ProductoDto
        {
            IdProducto = p.id_producto,
            Nombre = p.nombre,
            Descripcion = p.descripcion,
            Precio = p.precio,
            Stock = p.stock,
            Activo = p.activo,
            CategoriaNombre = p.Categoria?.nombre,
            MarcaNombre = p.id_marca.HasValue && marcaMap.ContainsKey(p.id_marca.Value) ? marcaMap[p.id_marca.Value] : null,
            ImagenPrincipalUrl = p.Imagenes.FirstOrDefault()?.url_imagen,
            Seccion = p.seccion
        });
    }

    public async Task<IEnumerable<ProductoDto>> GetByCategoryAsync(string categoryName, bool adminView = false)
    {
        // Filtramos por el campo 'seccion' (ej: "Hombre", "Mujer", "Accesorios")
        // O validamos mediante la relación de llave foránea (Categoria.nombre)
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Imagenes.Where(i => i.principal == true))
            .Where(p => (p.seccion != null && p.seccion.ToLower() == categoryName.ToLower()) ||
                        (p.Categoria != null && p.Categoria.nombre.ToLower() == categoryName.ToLower()))
            .AsQueryable();

        if (!adminView)
            query = query.Where(p => p.activo == true);

        var productos = await query.ToListAsync();

        var marcaIds = productos.Where(p => p.id_marca.HasValue).Select(p => p.id_marca.Value).Distinct().ToList();
        var marcas = await _context.Marcas
            .Where(m => marcaIds.Contains(m.id_marca))
            .Select(m => new { m.id_marca, m.nombre })
            .ToListAsync();
        var marcaMap = marcas.ToDictionary(m => m.id_marca, m => m.nombre);

        return productos.Select(p => new ProductoDto
        {
            IdProducto = p.id_producto,
            Nombre = p.nombre,
            Descripcion = p.descripcion,
            Precio = p.precio,
            Stock = p.stock,
            Activo = p.activo,
            CategoriaNombre = p.Categoria?.nombre,
            MarcaNombre = p.id_marca.HasValue && marcaMap.ContainsKey(p.id_marca.Value) ? marcaMap[p.id_marca.Value] : null,
            ImagenPrincipalUrl = p.Imagenes.FirstOrDefault()?.url_imagen,
            Seccion = p.seccion
        });
    }

    public async Task<IEnumerable<ProductoDto>> SearchAsync(string querySearch, bool adminView = false)
    {
        var q = querySearch.ToLower();
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Imagenes.Where(i => i.principal == true))
            .AsQueryable();

        if (!adminView)
            query = query.Where(p => p.activo == true);

        var productos = await query.ToListAsync();

        var marcaIds = productos.Where(p => p.id_marca.HasValue).Select(p => p.id_marca.Value).Distinct().ToList();
        var marcas = await _context.Marcas
            .Where(m => marcaIds.Contains(m.id_marca))
            .Select(m => new { m.id_marca, m.nombre })
            .ToListAsync();
        var marcaMap = marcas.ToDictionary(m => m.id_marca, m => m.nombre);

        // Filter by name, description OR brand name
        var filteredProductos = productos.Where(p => 
            p.nombre.ToLower().Contains(q) || 
            (p.descripcion != null && p.descripcion.ToLower().Contains(q)) ||
            (p.id_marca.HasValue && marcaMap.ContainsKey(p.id_marca.Value) && marcaMap[p.id_marca.Value].ToLower().Contains(q))
        ).ToList();

        return filteredProductos.Select(p => new ProductoDto
        {
            IdProducto = p.id_producto,
            Nombre = p.nombre,
            Descripcion = p.descripcion,
            Precio = p.precio,
            Stock = p.stock,
            Activo = p.activo,
            CategoriaNombre = p.Categoria?.nombre,
            MarcaNombre = p.id_marca.HasValue && marcaMap.ContainsKey(p.id_marca.Value) ? marcaMap[p.id_marca.Value] : null,
            ImagenPrincipalUrl = p.Imagenes.FirstOrDefault()?.url_imagen,
            Seccion = p.seccion
        });
    }

    public async Task<ProductoDto> GetByIdAsync(int id)
    {
        var p = await _context.Productos
            .Include(prod => prod.Categoria)
            .Include(prod => prod.Imagenes)
            .FirstOrDefaultAsync(prod => prod.id_producto == id);

        if (p == null) throw new Exception("Producto no encontrado");

        string? marcaNombre = null;
        if (p.id_marca.HasValue)
        {
            marcaNombre = await _context.Marcas
                .Where(m => m.id_marca == p.id_marca.Value)
                .Select(m => m.nombre)
                .FirstOrDefaultAsync();
        }

        return new ProductoDto
        {
            IdProducto = p.id_producto,
            Nombre = p.nombre,
            Descripcion = p.descripcion,
            Precio = p.precio,
            Stock = p.stock,
            Activo = p.activo,
            CategoriaNombre = p.Categoria?.nombre,
            MarcaNombre = marcaNombre,
            ImagenPrincipalUrl = p.Imagenes.OrderBy(i => i.orden).FirstOrDefault()?.url_imagen,
            ImagenesUrls = p.Imagenes.OrderBy(i => i.orden).Select(i => i.url_imagen).Where(u => !string.IsNullOrEmpty(u)).ToList()!,
            ImagenesDetalle = p.Imagenes.OrderBy(i => i.orden).Select(i => new ImagenDto {
                IdImagen = i.id_imagen,
                UrlImagen = i.url_imagen,
                Principal = i.principal,
                Orden = i.orden
            }).ToList(),
            Seccion = p.seccion
        };
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateUpdateDto createDto)
    {
        var producto = new Producto
        {
            nombre = createDto.Nombre,
            descripcion = createDto.Descripcion,
            precio = createDto.Precio,
            stock = createDto.Stock,
            id_categoria = createDto.IdCategoria,
            id_marca = createDto.IdMarca,
            seccion = createDto.Seccion,
            activo = true,
            fecha_creacion = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(producto.id_producto);
    }

    public async Task<ProductoDto> UpdateAsync(int id, ProductoCreateUpdateDto updateDto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) throw new Exception("Producto no encontrado");

        producto.nombre = updateDto.Nombre;
        producto.descripcion = updateDto.Descripcion;
        producto.precio = updateDto.Precio;
        producto.stock = updateDto.Stock;
        producto.id_categoria = updateDto.IdCategoria;
        producto.id_marca = updateDto.IdMarca;
        producto.seccion = updateDto.Seccion;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) throw new Exception("Producto no encontrado");

        // Borrado físico (real)
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleStateAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) throw new Exception("Producto no encontrado");

        producto.activo = !producto.activo;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UploadImagesAsync(int idProducto, List<IFormFile> imagenes)
    {
        var producto = await _context.Productos.Include(p => p.Imagenes).FirstOrDefaultAsync(p => p.id_producto == idProducto);
        if (producto == null) throw new Exception("Producto no encontrado");

        if (imagenes == null || imagenes.Count == 0) return false;

        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "productos");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        bool hasPrincipal = producto.Imagenes.Any(i => i.principal);

        foreach (var file in imagenes)
        {
            if (file.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var imgRecord = new ImagenProducto
                {
                    id_producto = idProducto,
                    url_imagen = "/uploads/productos/" + uniqueFileName,
                    principal = !hasPrincipal // La primera en subirse se vuelve principal si no hay otra
                };
                
                if (!hasPrincipal) hasPrincipal = true; // Siguientes serán secundarias
                _context.ImagenesProducto.Add(imgRecord);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteImageAsync(int idProducto, int idImagen)
    {
        var imagen = await _context.ImagenesProducto.FirstOrDefaultAsync(i => i.id_imagen == idImagen && i.id_producto == idProducto);
        if (imagen == null) throw new Exception("Imagen no encontrada o no pertenece al producto");

        // Borrado físico del archivo
        if (!string.IsNullOrEmpty(imagen.url_imagen))
        {
            var fileName = Path.GetFileName(imagen.url_imagen);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", "productos", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.ImagenesProducto.Remove(imagen);

        // Si la imagen era principal, asignamos la siguiente a principal (si hay)
        if (imagen.principal)
        {
            var otra = await _context.ImagenesProducto.Where(i => i.id_producto == idProducto && i.id_imagen != idImagen).OrderBy(i => i.orden).FirstOrDefaultAsync();
            if (otra != null) otra.principal = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrincipalImageAsync(int idProducto, int idImagen)
    {
        var imagenes = await _context.ImagenesProducto.Where(i => i.id_producto == idProducto).ToListAsync();
        var imagenASeleccionar = imagenes.FirstOrDefault(i => i.id_imagen == idImagen);
        if (imagenASeleccionar == null) throw new Exception("Imagen no encontrada");

        foreach (var img in imagenes)
        {
            img.principal = false;
        }
        
        imagenASeleccionar.principal = true;
        imagenASeleccionar.orden = -1; // Fuerza a ser la primera

        await _context.SaveChangesAsync();
        return true;
    }
}
