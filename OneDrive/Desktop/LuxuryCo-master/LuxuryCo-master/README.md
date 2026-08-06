# LuxuryCo

LuxuryCo es una plataforma de comercio electrónico de alta gama que ofrece una experiencia de compra personalizada mediante la integración de un asistente de Inteligencia Artificial (AI Stylist). El sistema cuenta con un panel de administración completo, autenticación de usuarios, gestión de inventario y un widget conversacional de recomendaciones de estilo integrado en el portal del cliente.

## 🚀 Características Principales

- **E-Commerce Completo**: Catálogo de productos, gestión de marcas y procesamiento de carrito de compras.
- **Panel de Administración**: Gestión CRUD (Crear, Leer, Actualizar, Eliminar) para usuarios, productos y marcas con roles de acceso (Admin/Usuario).
- **AI Stylist (Asistente Inteligente)**: Widget de chat integrado en el frontend que mantiene un historial persistente de conversaciones en base de datos, proporcionando recomendaciones personalizadas de productos.
- **Autenticación y Seguridad**: Sistema de registro y login seguro, con manejo de roles y sesiones.
- **Diseño Moderno y Responsivo**: Interfaz construida con vistas MVC estructuradas, diseño visual atractivo para comercio de lujo.

## 🛠️ Tecnologías Utilizadas

- **Frontend**: ASP.NET Core MVC (`LuxuryCo.Front`), HTML5, CSS3, JavaScript. Vistas Razor (`.cshtml`).
- **Backend**: ASP.NET Core (`LuxuryCo.Back`), C#.
- **Base de Datos**: Entity Framework Core (`LuxuryCo.Database`), PostgreSQL / SQL Server. Modelado de base de datos relacional.
- **Integración de IA**: Servicios de IA para el chat del estilista con soporte de memoria conversacional en base de datos.

## 📁 Estructura del Proyecto

La solución está dividida en tres capas principales:

- `LuxuryCo.Front/`: Proyecto web MVC que maneja la interfaz de usuario, vistas, controladores del cliente y recursos estáticos (`wwwroot`).
- `LuxuryCo.Back/`: Lógica de negocio, servicios (ej. `IAiService`) y lógica de autenticación/API.
- `LuxuryCo.Database/`: Contexto de Entity Framework (`LuxuryCoDbContext`), modelos de dominio (ej. `HistorialChatAi`, `Productos`, `Usuarios`) y migraciones.

## ⚙️ Configuración y Ejecución

### Prerrequisitos
- [.NET SDK](https://dotnet.microsoft.com/download) (versión 7.0 u 8.0)
- Servidor de base de datos SQL Server o PostgreSQL configurado.
- IDE recomendado: Visual Studio 2022 o Visual Studio Code.

### Pasos para ejecutar localmente

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/tu-usuario/LuxuryCo.git
   cd LuxuryCo
   ```

2. **Configurar la Base de Datos**:
   - Actualiza la cadena de conexión (`DefaultConnection`) en los archivos `appsettings.json` para apuntar a tu servidor de base de datos local.
   - Aplica las migraciones de Entity Framework para generar las tablas:
     ```bash
     dotnet ef database update --project LuxuryCo.Database --startup-project LuxuryCo.Front
     ```

3. **Ejecutar la aplicación**:
   - Desde la terminal en la raíz de la solución, puedes compilar y ejecutar el proyecto frontend:
     ```bash
     cd LuxuryCo.Front
     dotnet run
     ```
   - Abre tu navegador en la URL indicada por la consola (ej. `http://localhost:5000` o `https://localhost:7001`).

## 🤝 Contribución

Las contribuciones son bienvenidas. Si deseas mejorar el proyecto, por favor abre un issue para discutir los cambios o envía un Pull Request directamente.

---
*Desarrollado para ofrecer la mejor experiencia en compras de lujo impulsadas por Inteligencia Artificial.*
