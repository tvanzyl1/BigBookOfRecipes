Set-Location "C:\Source\BigBookOfRecipes"

Write-Host "Restoring project..."
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "Building project..."
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "Launching browser..."
Start-Process "http://127.0.0.1:5015"

Write-Host "Running application..."
dotnet run --urls http://127.0.0.1:5015

Read-Host "Press Enter to exit"