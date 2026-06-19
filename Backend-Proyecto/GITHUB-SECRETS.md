# GitHub Secrets — Backend SIGASJ

Documentacion alineada con el manual oficial de MonsterASP:

[How to deploy Website via Github actions?](https://help.monsterasp.net/books/github/page/how-to-deploy-website-via-github-actions)

Solo el **dueno del repositorio** (`Wuipy`) crea los secrets en:

**GitHub → Backend-Proyecto → Settings → Secrets and variables → Actions**

Los colaboradores hacen push; el deploy corre con los secrets del repo.

---

## Secrets de Web Deploy (obligatorios — manual MonsterASP)

| Secret | Valor para este proyecto |
|--------|--------------------------|
| `WEBSITE_NAME` | `site75093` |
| `SERVER_COMPUTER_NAME` | `https://site75093.siteasp.net:8172` |
| `SERVER_USERNAME` | `site75093` |
| `SERVER_PASSWORD` | Contrasena de Web Deploy (panel MonsterASP) |

Estos cuatro nombres deben coincidir **exactamente** con el manual. El workflow `.github/workflows/publish.yml` los usa tal cual.

---

## Secrets de la aplicacion (recomendados — SIGASJ)

Para que la API arranque con Supabase y JWT, el dueno del repo puede agregar tambien:

| Secret | Contenido |
|--------|-----------|
| `CONNECTIONSTRINGS__DEFAULTCONNECTION` | Cadena Supabase (Session pooler, puerto 5432) |
| `JWT__SECRET` | Clave JWT (minimo 32 caracteres) |
| `ADMIN__CONTRASENA` | Contrasena admin en produccion |
| `CORS__ALLOWEDORIGINS__0` | URL publica del frontend |

Si faltan, el deploy a MonsterASP puede completarse pero la API fallara al iniciar hasta configurarlos aqui o en el panel MonsterASP (Scripting → Environment Variables).

### Ejemplo Supabase

```
Host=aws-1-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.SU_PROJECT_REF;Password=SU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

> Con pooler use `Username=postgres.SU_PROJECT_REF`, no solo `postgres`.

---

## Flujo

1. Push a `main` o `josh`, o **Actions → Build, publish and deploy to MonsterASP.NET → Run workflow**
2. GitHub compila y publica en `./publish`
3. `simply-web-deploy` sube al sitio con los secrets `WEBSITE_*` / `SERVER_*`
4. Si existen secrets de aplicacion, se genera `appsettings.Production.json` antes del publish

## Verificar

```
https://site75093.runasp.net/api/health
```
