# 1. 强制跳转到脚本所在目录
$currentDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $currentDir

# 2. 定义文件名 (请确保文件名拼写正确)
$fileName = $currentDir + "\ShadowViewer\ShadowViewer_TemporaryKey.pfx"

if (Test-Path $fileName) {
    try {
        $fileBytes = [System.IO.File]::ReadAllBytes($fileName)
        $base64String = [Convert]::ToBase64String($fileBytes)
        
        # 复制到剪贴板
        $base64String | clip
        
        Write-Host "Success! Base64 string copied to clipboard." -ForegroundColor Green
    } catch {
        Write-Host "Error during conversion: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "Error: Cannot find file - $fileName" -ForegroundColor Red
    Write-Host "Current Directory: $(Get-Location)" -ForegroundColor Yellow
}