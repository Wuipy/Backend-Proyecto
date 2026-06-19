# Despliegue en MonsterASP.net — SIGASJ API

Hosting: [MonsterASP.net](https://www.monsterasp.net/) · .NET 9 · Web Deploy · Supabase (PostgreSQL externo)

## Opcion A — GitHub Actions (recomendada para el equipo)

Manual oficial MonsterASP: [Deploy via Github actions](https://help.monsterasp.net/books/github/page/how-to-deploy-website-via-github-actions)

El workflow del repo es `.github/workflows/publish.yml` y usa estos **secrets** (creados por el dueno del repo):

| Secret | Valor |
|--------|--------|
| `WEBSITE_NAME` | `site75093` |
| `SERVER_COMPUTER_NAME` | `https://site75093.siteasp.net:8172` |
| `SERVER_USERNAME` | `site75093` |
| `SERVER_PASSWORD` | Contrasena Web Deploy |

Detalle completo en **[GITHUB-SECRETS.md](./GITHUB-SECRETS.md)** (incluye secrets opcionales de Supabase/JWT).

Push a `main` o `josh` → deploy automatico. O **Actions → Build, publish and deploy to MonsterASP.NET → Run workflow**.

---

## Opcion B — Panel MonsterASP (manual)

Si prefieres configurar en el hosting en lugar de GitHub Secrets:

**Control panel → Websites → site75093 → Scripting → Environment Variables**

| Variable | Valor |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena Supabase (Session pooler, puerto 5432) |
| `Jwt__Secret` | Clave segura de 32+ caracteres |
| `Jwt__Issuer` | `SIGASJ` |
| `Jwt__Audience` | `SIGASJ-Frontend` |
| `Admin__Usuario` | `admin` |
| `Admin__Contrasena` | Contraseña segura de producción |
| `Cors__AllowedOrigins__0` | URL pública del frontend |

Reinicia el sitio después de guardar.

---

## Opcion C — Publicar desde tu PC (PowerShell)

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Web Deploy (viene con Visual Studio) o [Microsoft Web Deploy V3](https://www.iis.net/downloads/microsoft/web-deploy)

### Pasos

```powershell
cd Backend-Proyecto
copy monsterasp.local.env.example monsterasp.local.env
# Edite monsterasp.local.env con la contraseña de Web Deploy (NO la suba a Git)

.\scripts\deploy-monsterasp.ps1
```

Alternativa manual:

```powershell
dotnet publish -c Release /p:PublishProfile=MonsterASP /p:Password=SU_PASSWORD_WEBDEPLOY
```

### Visual Studio

1. Clic derecho en el proyecto → **Publish**
2. **Import profile** → archivo `.publishSettings` del panel MonsterASP  
   (o use el perfil `Properties/PublishProfiles/MonsterASP.pubxml`)
3. **Publish**

---

## 3. Verificar

Abra en el navegador:

```
https://site75093.runasp.net/api/health
```

Respuesta esperada:

```json
{ "status": "ok", "servicio": "SIGASJ API", "fecha": "..." }
```

---

## 4. Frontend

Configure la URL del API en producción:

```env
VITE_API_BASE_URL=https://site75093.runasp.net
```

Y agregue esa URL en `Cors__AllowedOrigins__0` en MonsterASP.

---

## 5. Solución de problemas

| Síntoma | Qué revisar |
|---------|-------------|
| 500 al iniciar | Logs en MonsterASP → Error log. Falta `ConnectionStrings__DefaultConnection` |
| `password authentication failed for user "postgres"` | Username incorrecto en Supabase (use `postgres.xxxx`) |
| CORS en el navegador | `Cors__AllowedOrigins__0` = URL exacta del frontend (con `https://`) |
| Web Deploy falla | Web Deploy activado, puerto 8172, contraseña correcta |

---

## Seguridad

- **No** guarde contraseñas de Web Deploy ni de Supabase en Git.
- Cambie la contraseña de Web Deploy si se expuso en chat o capturas.
- Use contraseñas distintas para admin, Web Deploy y Supabase.
