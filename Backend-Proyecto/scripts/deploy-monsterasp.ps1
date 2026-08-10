param(
    [string]$EnvFile = (Join-Path $PSScriptRoot "..\monsterasp.local.env")
)

$ErrorActionPreference = "Stop"
$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if (-not (Test-Path $EnvFile)) {
    Write-Error "No existe $EnvFile. Copie monsterasp.local.env.example y complete la contraseña de Web Deploy."
}

Get-Content $EnvFile | ForEach-Object {
    if ($_ -match '^\s*#' -or $_ -match '^\s*$') { return }
    $name, $value = $_ -split '=', 2
    Set-Item -Path "Env:$name" -Value $value.Trim()
}

if (-not $env:MONSTERASP_PASSWORD) {
    Write-Error "Defina MONSTERASP_PASSWORD en monsterasp.local.env"
}

Write-Host "Publicando SIGASJ API a MonsterASP ($env:MONSTERASP_SITE)..." -ForegroundColor Cyan

Push-Location $projectRoot
try {
    $devPath = Join-Path $projectRoot "appsettings.Development.json"
    $basePath = Join-Path $projectRoot "appsettings.json"
    $prodPath = Join-Path $projectRoot "appsettings.Production.json"

    if (-not (Test-Path $devPath)) {
        Write-Error "Falta appsettings.Development.json con la cadena de Supabase."
    }

    $dev = Get-Content $devPath -Raw | ConvertFrom-Json
    $base = Get-Content $basePath -Raw | ConvertFrom-Json

    if (-not $dev.ConnectionStrings.DefaultConnection) {
        Write-Error "ConnectionStrings.DefaultConnection vacio en appsettings.Development.json"
    }

    $production = [ordered]@{
        Logging = [ordered]@{
            LogLevel = [ordered]@{
                Default = "Warning"
                "Microsoft.AspNetCore" = "Warning"
            }
        }
        AllowedHosts = "*"
        ConnectionStrings = [ordered]@{
            DefaultConnection = $dev.ConnectionStrings.DefaultConnection
        }
        Jwt = [ordered]@{
            Secret = $base.Jwt.Secret
            Issuer = $base.Jwt.Issuer
            Audience = $base.Jwt.Audience
            ExpirationHours = $base.Jwt.ExpirationHours
        }
        Admin = [ordered]@{
            Usuario = $base.Admin.Usuario
            Contrasena = $base.Admin.Contrasena
        }
        Cors = [ordered]@{
            AllowedOrigins = @(
                "https://sigasjiv.netlify.app",
                "https://sigasj.netlify.app",
                "http://sigasj.runasp.net",
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
        }
    }

    $production | ConvertTo-Json -Depth 6 | Set-Content $prodPath -Encoding utf8
    Write-Host "appsettings.Production.json generado para el deploy (no se commitea)." -ForegroundColor DarkGray

    dotnet publish `
        -c Release `
        --runtime win-x86 `
        --self-contained false `
        /p:PublishProfile=MonsterASP `
        "/p:Password=$($env:MONSTERASP_PASSWORD)"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish fallo con codigo $LASTEXITCODE"
    }

    Write-Host ""
    Write-Host "Deploy completado." -ForegroundColor Green
    Write-Host "Pruebe: http://sigasj.runasp.net/swagger"
}
finally {
    Pop-Location
}
