$projectPath = "C:\Source\BigBookOfRecipes"
$localUrl = "http://127.0.0.1:5015"
$port = 5015

Set-Location $projectPath

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

Write-Host "Starting ngrok tunnel..."
$ngrokProcess = Start-Process `
    -FilePath "ngrok" `
    -ArgumentList "http $port" `
    -PassThru `
    -WindowStyle Minimized

Write-Host "Waiting for ngrok public URL..."

$publicUrl = $null

for ($i = 0; $i -lt 20; $i++) {
    try {
        $tunnels = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels"
        $publicUrl = $tunnels.tunnels |
            Where-Object { $_.proto -eq "https" } |
            Select-Object -First 1 -ExpandProperty public_url

        if ($publicUrl) {
            break
        }
    }
    catch {
        Start-Sleep -Seconds 1
    }

    Start-Sleep -Seconds 1
}

if ($publicUrl) {
    Write-Host ""
    Write-Host "ngrok public URL:" -ForegroundColor Green
    Write-Host $publicUrl -ForegroundColor Cyan
    Write-Host ""

    Set-Clipboard $publicUrl
    Write-Host "Copied ngrok URL to clipboard." -ForegroundColor Green

    Start-Process $publicUrl
}
else {
    Write-Host "Could not fetch ngrok public URL. Opening local URL instead." -ForegroundColor Yellow
    Start-Process $localUrl
}

Write-Host "Opening local browser..."
Start-Process $localUrl

Write-Host "Running application..."
dotnet run --urls $localUrl

Write-Host "Stopping ngrok..."
if ($ngrokProcess -and !$ngrokProcess.HasExited) {
    Stop-Process -Id $ngrokProcess.Id -Force
}

Read-Host "Press Enter to exit"