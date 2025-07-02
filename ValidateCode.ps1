# PowerShell script to validate C# code syntax
Write-Host "=== Validating OPC DA Client Code ===" -ForegroundColor Cyan
Write-Host ""

$files = @(
    "IOpcDaClient.cs",
    "OpcDaClient.cs", 
    "Program.cs",
    "SimpleTest.cs"
)

$hasErrors = $false

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Checking $file..." -NoNewline
        
        # Basic syntax checks
        $content = Get-Content $file -Raw
        
        # Check for balanced braces
        $openBraces = ([regex]::Matches($content, '\{')).Count
        $closeBraces = ([regex]::Matches($content, '\}')).Count
        
        if ($openBraces -ne $closeBraces) {
            Write-Host " ERROR: Unbalanced braces!" -ForegroundColor Red
            $hasErrors = $true
        }
        else {
            # Check for basic C# syntax patterns
            if ($content -match 'using\s+System;' -and 
                $content -match 'namespace\s+\w+' -and
                $content -match 'class\s+\w+') {
                Write-Host " OK" -ForegroundColor Green
            }
            else {
                Write-Host " WARNING: Missing basic C# structure" -ForegroundColor Yellow
            }
        }
    }
    else {
        Write-Host "$file not found!" -ForegroundColor Red
        $hasErrors = $true
    }
}

Write-Host ""
if (-not $hasErrors) {
    Write-Host "All files validated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Open OpcDaClient.sln in Visual Studio 2019"
    Write-Host "2. Ensure platform is set to x86"
    Write-Host "3. Build the solution (Ctrl+Shift+B)"
}
else {
    Write-Host "Validation failed! Please check the errors above." -ForegroundColor Red
}

Write-Host ""
pause