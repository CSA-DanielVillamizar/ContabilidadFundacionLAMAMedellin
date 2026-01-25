# CRITICAL FIX REQUIRED: HTTP 500 in Production WebApp

## Root Cause Found
The `ConnectionStrings__DefaultConnection` app setting is currently **NULL** or **INCOMPLETE**.

When checked with `az webapp config appsettings list`, it shows:
```
ConnectionStrings__DefaultConnection: null
```

OR shows an incomplete Key Vault reference (missing closing parenthesis):
```
ConnectionStrings__DefaultConnection: @Microsoft.KeyVault(SecretUri=https://kvtesorerialamamdln.vault.azure.net/secrets/sql-connectionstring/eb8ee9796a2f480cabe4db5b30d56da2
```

## Immediate Fix
Run the following commands in PowerShell (Azure CLI):

```powershell
# 1. Set the correct connection string reference
$secretUri = "https://kvtesorerialamamdln.vault.azure.net/secrets/sql-connectionstring/eb8ee9796a2f480cabe4db5b30d56da2"
$kvRef = "@Microsoft.KeyVault(SecretUri=$secretUri)"

az webapp config appsettings set `
  --name app-tesorerialamamedellin-prod `
  --resource-group RG-TesoreriaLAMAMedellin-Prod `
  --settings "ConnectionStrings__DefaultConnection=$kvRef"

# 2. Verify it was set correctly
az webapp config appsettings list `
  --name app-tesorerialamamedellin-prod `
  --resource-group RG-TesoreriaLAMAMedellin-Prod `
  --query "[?name=='ConnectionStrings__DefaultConnection'].value" -o tsv

# Should output: @Microsoft.KeyVault(SecretUri=https://kvtesorerialamamdln.vault.azure.net/secrets/sql-connectionstring/eb8ee9796a2f480cabe4db5b30d56da2)

# 3. Restart the web app
az webapp restart `
  --name app-tesorerialamamedellin-prod `
  --resource-group RG-TesoreriaLAMAMedellin-Prod

# 4. Test the endpoint (wait 5 seconds after restart)
Start-Sleep -Seconds 5
Invoke-WebRequest -Uri "https://app-tesorerialamamedellin-prod.azurewebsites.net/" -UseBasicParsing -SkipHttpErrorCheck | Select-Object StatusCode
```

## Why This Fixes HTTP 500

1. **Program.cs** requires `ConnectionStrings__DefaultConnection` to load the SQL connection string
2. The app reads this setting and checks if it contains `Authentication=Active Directory Default`
3. If the setting is NULL or malformed, DbContext initialization fails → **HTTP 500**
4. By setting it to a proper Key Vault reference, the app can:
   - Resolve the secret using the WebApp's Managed Identity (no plaintext secrets!)
   - Extract the connection string
   - Auto-add AAD authentication if not present
   - Successfully initialize DbContext

## Security Note
- The Key Vault reference uses **Managed Identity (RBAC)** to fetch the secret
- The WebApp MI already has **"Key Vault Secrets User"** role on the Key Vault
- No plaintext secrets are stored in app settings ✅

## Verification
After running the fix, the site should respond with **HTTP 200** instead of **HTTP 500**.

If you still see 500:
- Check `az webapp log tail` for detailed errors
- Verify the secret URI is exactly correct (copy-paste from Key Vault portal)
- Ensure there are no extra spaces in the reference
