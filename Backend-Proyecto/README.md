# SIGASJ API - Backend ASADA San Juan

API REST para el portal **SIGASJ** (Sistema de Gestión del Acueducto ASADA San Juan de Santa Cruz), conectada al frontend React/Vite existente.

## Tecnologías

- **ASP.NET Core 9** (Web API)
- **Entity Framework Core** + **SQLite**
- **JWT** para autenticación administrativa
- **BCrypt** para hash de contraseñas

> Nota: El repositorio ya contenía un proyecto ASP.NET Core. Se extendió esa base en lugar de crear un backend Node.js paralelo, manteniendo la arquitectura solicitada (controllers, services, models, middleware, validators).

## Estructura del proyecto

```
Backend-Proyecto/
├── Controllers/          # Endpoints REST
├── Models/
│   ├── Entities/       # Entidades de base de datos
│   └── DTOs/             # Contratos de entrada/salida
├── Services/             # Lógica de negocio
├── Data/                 # DbContext y seed inicial
├── Middleware/           # Manejo global de errores
├── Configuration/        # JWT, CORS, Admin
├── Utils/                # Formato de fechas, mensajes de estado
├── Program.cs
└── appsettings.json
```

## Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Node.js 18+ (solo para el frontend)

## Instalación y ejecución

### 1. Backend

```bash
cd Backend-Proyecto
dotnet restore
dotnet run --urls "http://localhost:5145"
```

La API quedará disponible en `http://localhost:5145`.

La base de datos SQLite (`sigasj.db`) se crea automáticamente al iniciar, con datos iniciales de comunicados, proyectos y usuario admin.

### 2. Frontend

```bash
cd vite-project
npm install
cp .env.example .env
npm run dev
```

El frontend usa el proxy de Vite (`/api` → `http://localhost:5145`) configurado en `vite.config.js`.

## Configuración

Variables principales en `appsettings.json`:

| Clave | Descripción |
|-------|-------------|
| `ConnectionStrings:DefaultConnection` | Ruta SQLite (`Data Source=sigasj.db`) |
| `Jwt:Secret` | Clave JWT (mínimo 32 caracteres en producción) |
| `Jwt:ExpirationHours` | Duración del token |
| `Admin:Usuario` | Usuario admin inicial |
| `Admin:Contrasena` | Contraseña admin inicial |
| `Cors:AllowedOrigins` | Orígenes del frontend Vite |

Ver `.env.example` como referencia de variables equivalentes.

### Credenciales por defecto

- **Usuario:** `admin`
- **Contraseña:** `admin1234`

## Endpoints

### Salud

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/health` | No | Estado del servicio |

### Autenticación

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/login` | No | Inicio de sesión admin → JWT |

### Público

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/averias` | Listar reportes de avería |
| GET | `/api/averias/{numero}` | Detalle por número (AV-0001) |
| POST | `/api/averias` | Crear reporte de avería |
| POST | `/api/solicitudes` | Crear solicitud de servicio |
| GET | `/api/seguimiento/{numero}` | Consulta pública AV/SOL |
| GET | `/api/comunicados` | Listar comunicados |
| GET | `/api/proyectos` | Listar proyectos |

### Admin (requiere `Authorization: Bearer {token}`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/actividades-plomeria` | Listar actividades |
| POST | `/api/actividades-plomeria` | Crear actividad |
| PUT | `/api/actividades-plomeria/{id}` | Actualizar actividad |
| PATCH | `/api/actividades-plomeria/{id}/estado` | Cambiar estado (ciclo o explícito) |
| DELETE | `/api/actividades-plomeria/{id}` | Eliminar actividad |

## Respuestas de error

```json
{
  "message": "Descripción del error",
  "errors": {
    "campo": ["mensaje de validación"]
  }
}
```

Códigos HTTP: `400`, `401`, `403`, `404`, `500`.

## Ejemplos de prueba (curl)

### Health check

```bash
curl http://localhost:5145/api/health
```

### Login admin

```bash
curl -X POST http://localhost:5145/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"usuario\":\"admin\",\"contrasena\":\"admin1234\"}"
```

### Crear avería

```bash
curl -X POST http://localhost:5145/api/averias \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Juan Perez\",\"telefono\":\"8888-8888\",\"direccion\":\"San Juan centro\",\"tipo\":\"Fuga\",\"descripcion\":\"Fuga visible en la acera\"}"
```

### Listar averías

```bash
curl http://localhost:5145/api/averias
```

### Crear solicitud

```bash
curl -X POST http://localhost:5145/api/solicitudes \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Maria Lopez\",\"cedula\":\"1-2345-6789\",\"telefono\":\"7777-7777\",\"correo\":\"maria@correo.com\",\"direccion\":\"Barrio Norte\",\"tipo\":\"Nueva conexion\",\"descripcion\":\"Solicito nueva conexion de agua potable\"}"
```

### Consultar seguimiento

```bash
curl http://localhost:5145/api/seguimiento/SOL-0001
```

### Crear actividad de plomería (con token)

```bash
TOKEN="pegue_aqui_el_token"

curl -X POST http://localhost:5145/api/actividades-plomeria \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"tipo\":\"Control de Fugas\",\"cliente\":\"Cliente demo\",\"ubicacion\":\"Sector 3\",\"descripcion\":\"Revision de fuga reportada\",\"estado\":\"Pendiente\"}"
```

## Conexión frontend ↔ backend

El frontend consume la API mediante `src/servicios/apiClient.ts` (Axios) y funciones en `landingService.ts` / `authAdmin.ts`.

- Desarrollo: proxy Vite en `/api`
- Producción: configure `VITE_API_BASE_URL` apuntando a la URL pública del backend

## Modelos de datos

| Entidad | Campos principales |
|---------|-------------------|
| **Averia** | numeroSeguimiento, nombre, telefono, correo?, direccion, tipo, descripcion, estado, foto |
| **Solicitud** | numeroSeguimiento, nombre, cedula, telefono, correo, direccion, tipo, descripcion, estado |
| **ActividadPlomeria** | id, tipo, cliente, ubicacion, descripcion, estado, fecha |
| **Comunicado** | fecha, titulo, descripcion, estado |
| **Proyecto** | titulo, descripcion, estado |
| **Usuario** | nombreUsuario, contrasenaHash, rol (admin) |

## Validaciones implementadas

- Campos requeridos en formularios públicos y admin
- Formato de correo electrónico
- Tipos de avería, solicitud y actividad restringidos a valores del frontend
- Estados de actividad: `Pendiente`, `En progreso`, `Completado`
- Números de seguimiento únicos (`AV-0001`, `SOL-0001`)
- Rutas admin protegidas con JWT y rol `admin`
