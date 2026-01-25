# Script to fix HTTP 500 by setting correct connection string reference
# Root cause: ConnectionStrings__DefaultConnection is NULL

Write-Host "========================================"
Write-Host "Fixing HTTP 500 in Production WebApp"
Write-Host "========================================"
Write-Host ""

$rgName = "RG-TesoreriaLAMAMedellin-Prod"
$webAppName = "app-tesorerialamamedellin-prod"
$kvName = "kvtesorerialamamdln"
$secretName = "sql-connectionstring"

# Get the secret URI
Write-Host "Getting secret URI from Key Vault..."
$secretUri = az keyvault secret show --vault-name $kvName --name $secretName --query 'id' -o tsv
Write-Host "Secret URI: $secretUri"
Write-Host ""

# Build the Key Vault reference
$kvRef = "@Microsoft.KeyVault(SecretUri=$secretUri)"
Write-Host "Building Key Vault reference:"
Write-Host "Value: $kvRef"
Write-Host "Length: $($kvRef.Length) chars"
Write-Host ""

# Set the app setting
Write-Host "Setting app setting in WebApp..."
az webapp config appsettings set `
  --name $webAppName `
  --resource-group $rgName `
  --settings "ConnectionStrings__DefaultConnection=$kvRef" `
  --output none

Write-Host "✅ App setting updated"
Write-Host ""

# Verify it was set
Write-Host "Verifying the setting..."
$setting = az webapp config appsettings list `
  --name $webAppName `
  --resource-group $rgName `
  --query "[?name=='ConnectionStrings__DefaultConnection'].value" -o tsv

if ($setting) {
    Write-Host "✅ Setting confirmed: $setting"
} else {
    Write-Host "❌ Setting is still null or empty!"
}
Write-Host ""

# Restart the web app
Write-Host "Restarting WebApp..."
az webapp restart --name $webAppName --resource-group $rgName
Write-Host "✅ WebApp restarted"
Write-Host ""

# Wait and test
Start-Sleep -Seconds 5
Write-Host "Testing endpoint..."
try {
    $response = Invoke-WebRequest -Uri "https://$webAppName.azurewebsites.net/" -UseBasicParsing -SkipHttpErrorCheck -TimeoutSec 10
    Write-Host "✅ Response Status: $($response.StatusCode)"
    if ($response.StatusCode -eq 200) {
        Write-Host "🎉 SUCCESS! HTTP 500 is resolved!"
    }
} catch {
    Write-Host "❌ Test failed: $_"
}
