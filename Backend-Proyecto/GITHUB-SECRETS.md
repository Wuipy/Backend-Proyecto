# GitHub Secrets — Backend SIGASJ → MonsterASP

Manual MonsterASP: [Deploy via Github actions](https://help.monsterasp.net/books/github/page/how-to-deploy-website-via-github-actions)

**Solo Wuipy** (dueno del repo) crea los secrets en:

**GitHub → Backend-Proyecto → Settings → Secrets and variables → Actions → New repository secret**

Workflow: `.github/workflows/publish.yml`  
Se dispara con push a **`master`** o **`josh`**, o manualmente en **Actions → Run workflow**.

---

## Secrets obligatorios — Web Deploy

| Secret | Valor |
|--------|--------|
| `WEBSITE_NAME` | `site75093` |
| `SERVER_COMPUTER_NAME` | `https://site75093.siteasp.net:8172` |
| `SERVER_USERNAME` | `site75093` |
| `SERVER_PASSWORD` | *(contrasena Web Deploy del panel MonsterASP)* |

---

## Secrets obligatorios — aplicacion

| Secret | Valor |
|--------|--------|
| `CONNECTIONSTRINGS__DEFAULTCONNECTION` | `Host=aws-1-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.wgedltybvsgmlbbnxnkr;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true` |
| `JWT__SECRET` | Clave segura de 32+ caracteres |
| `ADMIN__CONTRASENA` | Contrasena admin en produccion |
| `CORS__ALLOWEDORIGINS__0` | `https://sigasjiv.netlify.app` |

> Username Supabase con pooler: `postgres.wgedltybvsgmlbbnxnkr` (no solo `postgres`).

---

## Como ejecutar el deploy

1. Wuipy crea los **8 secrets** arriba
2. Push a `master` o `josh` **o** Actions → **Build, publish and deploy to MonsterASP.NET** → **Run workflow**
3. El workflow compila (`win-x86`), genera `appsettings.Production.json` y sube a MonsterASP
4. Al final prueba `http://sigasj.runasp.net/api/health`

---

## Verificar manualmente

```
http://sigasj.runasp.net/api/health
http://sigasj.runasp.net/swagger
```

---

## Errores comunes

| Error en Actions | Causa |
|------------------|--------|
| `Falta secret CONNECTIONSTRINGS__...` | Secret no creado en GitHub |
| Web Deploy unauthorized | `SERVER_PASSWORD` incorrecto |
| Health check fallo | Supabase/JWT mal configurados |
| Workflow no corre al push | Push fue a rama distinta de `master`/`josh` |
