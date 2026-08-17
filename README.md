# SIGASJ API

API REST para el portal **SIGASJ** (Sistema de Gestión del Acueducto ASADA San Juan de Santa Cruz). Backend en **Node.js + Express**, conectado al frontend React/Vite.

## Tecnologías

- **Node.js 18+** y **Express**
- **SQLite** (`better-sqlite3`)
- **JWT** para autenticación administrativa
- **bcryptjs** para hash de contraseñas

## Estructura

Misma arquitectura que el frontend: features por dominio, `shared` transversal y rutas públicas/privadas.

```
src/
├── index.js                 # Arranque (como main.tsx)
├── app.js                   # App Express (como App.tsx)
├── routes/
│   ├── publicRoutes.js
│   └── privateRoutes.js
├── features/
│   ├── auth/
│   ├── announcements/
│   ├── gallery/             # public + admin + api
│   ├── transparencia/
│   ├── landing/
│   ├── averias/
│   ├── solicitudes/
│   ├── lecturas/
│   └── usuarios/
└── shared/                  # config, db, errores, middleware
uploads/
```

## Requisitos

- Node.js 18 o superior
- npm

## Instalación y ejecución

```bash
cd Backend-Proyecto
cp .env.example .env
npm install
npm run dev
```

La API queda en `http://localhost:3000` (el frontend Vite hace proxy de `/api` y `/uploads` a este puerto).

La base SQLite (`sigasj.db`) se crea al iniciar, con comunicados, proyectos y usuarios iniciales.

### Credenciales por defecto

| Usuario     | Contraseña     | Rol        |
|-------------|----------------|------------|
| `admin`     | `admin1234`    | admin      |
| `fontanero` | `fontanero1234`| fontanero  |

En el login del frontend de desarrollo se usa `POST /api/auth/dev-token` (token de administradora).

## Configuración

Variables en `.env` (ver `.env.example`):

| Variable | Descripción |
|----------|-------------|
| `PORT` | Puerto HTTP (3000) |
| `DATABASE_PATH` | Archivo SQLite |
| `JWT_SECRET` | Clave JWT (mínimo 32 caracteres en producción) |
| `JWT_EXPIRATION_HOURS` | Duración del token |
| `ADMIN_USUARIO` / `ADMIN_CONTRASENA` | Usuario admin inicial |
| `CORS_ORIGINS` | Orígenes permitidos, separados por coma |

## Endpoints principales

### Salud y autenticación

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/health` | No | Estado del servicio |
| POST | `/api/auth/login` | No | Login admin/fontanero → JWT |
| POST | `/api/auth/dev-token` | No | Token de desarrollo para el frontend |

### Público

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/public/comunicados` | Comunicados para el landing |
| GET | `/api/public/galeria` | Galería activa |
| GET | `/api/public/transparencia` | Publicaciones de transparencia |
| GET | `/api/averias` | Listar reportes de avería |
| POST | `/api/averias` | Crear reporte de avería |
| POST | `/api/solicitudes` | Crear solicitud de servicio |
| GET | `/api/seguimiento/{numero}` | Consulta pública AV/SOL |
| GET | `/api/comunicados` | Comunicados (formato interno) |
| GET | `/api/proyectos` | Listar proyectos |

### Admin (`Authorization: Bearer {token}`)

Galería (`/api/admin/galeria`) y transparencia (`/api/admin/transparencia`): listar, crear (multipart), editar, estado, reordenar y eliminar.

También se mantienen los módulos operativos: actividades de plomería, averías, fontaneros y lecturas de medidor.

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
