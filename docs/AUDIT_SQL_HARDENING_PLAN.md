# Análisis de App Settings para Hardening (SQL + Key Vault)

## APP SETTINGS ACTUALES (desde inventario)

```
ASPNETCORE_ENVIRONMENT                    = Production ✅
WEBSITE_RUN_FROM_PACKAGE                  = 0
WEBSITE_HTTPLOGGING_RETENTION_DAYS        = 3
ConnectionStrings__DefaultConnection      = [TEXTO PLANO] Server=tcp:sql-tesorerialamamedellin-prod... ⚠️ INSEGURO
APPINSIGHTS_INSTRUMENTATIONKEY            = [VALOR EN CLARO] 021b6cf5-f1d6-4ec2-ad46-180bf06e7844 ⚠️ INSEGURO
APPLICATIONINSIGHTS_CONNECTION_STRING     = [VALOR EN CLARO] InstrumentationKey=... ⚠️ INSEGURO
ApplicationInsights__ConnectionString     = @Microsoft.KeyVault(SecretUri=https://kvtesorerialamamdln.vault.azure.net/secrets/appi-connectionstring/...) ✅ SEGURO (KV)
ApplicationInsightsAgent_EXTENSION_VERSION = ~2 ✅
XDT_MicrosoftApplicationInsights_*        = (varios)
WEBSITE_HEALTHCHECK_MAXPINGFAILURES       = 10
DIAGNOSTICS_AZUREBLOBRETENTIONINDAYS      = 3
```

## PROBLEMAS DETECTADOS

| Setting | Problema | Acción |
|---------|----------|--------|
| `ConnectionStrings__DefaultConnection` | Texto plano con credenciales SQL | **ELIMINAR** (usar Key Vault) |
| `APPINSIGHTS_INSTRUMENTATIONKEY` | Valor en claro | **ELIMINAR** (usar `ApplicationInsights__ConnectionString` vía KV) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Valor en claro | **ELIMINAR** (usar `ApplicationInsights__ConnectionString` vía KV) |
| `ApplicationInsights__ConnectionString` | Correcto | **MANTENER** (referencia Key Vault) |

## PLAN DE CAMBIOS

### Paso 1: Verificar secreto en Key Vault
- Secreto existente: `sql-connectionstring`
- **Acción**: Confirmar/actualizar con connection string sin credenciales (usar `Authentication=Active Directory Default`)

### Paso 2: Eliminar App Settings en texto plano
Comandos a ejecutar (después de confirmar cambios en código):

```bash
# Eliminar ConnectionStrings__DefaultConnection (la app lo leerá de Key Vault)
az webapp config appsettings delete \
  --name app-tesorerialamamedellin-prod \
  --resource-group RG-TesoreriaLAMAMedellin-Prod \
  --setting-names ConnectionStrings__DefaultConnection

# Eliminar APPINSIGHTS_INSTRUMENTATIONKEY (redundante)
az webapp config appsettings delete \
  --name app-tesorerialamamedellin-prod \
  --resource-group RG-TesoreriaLAMAMedellin-Prod \
  --setting-names APPINSIGHTS_INSTRUMENTATIONKEY

# Eliminar APPLICATIONINSIGHTS_CONNECTION_STRING (redundante, usar ApplicationInsights__ConnectionString vía KV)
az webapp config appsettings delete \
  --name app-tesorerialamamedellin-prod \
  --resource-group RG-TesoreriaLAMAMedellin-Prod \
  --setting-names APPLICATIONINSIGHTS_CONNECTION_STRING
```

### Paso 3: Verificar appsettings.Production.json
Ya tiene:
- `ConnectionStrings.DefaultConnection` = `#{ConnectionString}#` (placeholder, se rellena en deployment)
- `Azure.KeyVaultEndpoint` = correcta
- `Azure.EnableKeyVault` = `true`
- `ApplicationInsights.ConnectionString` no visible en lo que revisamos

**Acción**: Confirmar que el app settings `ApplicationInsights__ConnectionString` se carga de Key Vault (ya está configurado).

## ORDEN DE EJECUCIÓN

1. ✅ Establecer Entra ID admin en SQL (completado)
2. ⏳ Ejecutar script T-SQL en la DB (requiere Azure Data Studio / SSMS con Entra ID)
3. ⏳ Verificar/actualizar secreto `sql-connectionstring` en Key Vault (con `Authentication=Active Directory Default`, sin credenciales)
4. ⏳ Eliminar app settings en texto plano en el WebApp (az CLI)
5. ✅ Build + Test (sin cambios de código, solo verificación)

## NOTAS DE SEGURIDAD

- **No imprimiremos valores de secretos** (solo nombres de secrets y referencias)
- **Managed Identity** (`app-tesorerialamamedellin-prod`) se autentica automáticamente en SQL y Key Vault
- **Rotate connection strings periódicamente** (la app sigue funcionando sin cambios de código)
- **El WebApp necesita reinicio** después de cambios en app settings (puede hacerse vía portal o `az webapp restart`)

