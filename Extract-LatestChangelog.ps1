# Extract-LatestChangelog.ps1
# 从 CHANGELOG.md 中提取最新版本段落并输出到 LATEST_CHANGELOG.md

param(
    [string]$InputFile = "CHANGELOG.md",
    [string]$OutputFile = "LATEST_CHANGELOG.md"
)

if (-not (Test-Path $InputFile)) {
    Write-Error "找不到文件：$InputFile"
    exit 1
}

$lines = Get-Content $InputFile

# 找到所有以 "## " 开头的版本标题
$headers = $lines | Select-String '^## '

if ($headers.Count -eq 0) {
    Write-Error "CHANGELOG.md 中没有找到任何版本标题（## 开头）"
    exit 1
}

# 第一个版本标题（最新）
$start = $headers[0].LineNumber

# 如果只有一个版本标题
if ($headers.Count -eq 1) {
    $latest = $lines[$start-1..($lines.Length-1)]
}
else {
    # 有多个版本，截取到下一个版本标题之前
    $next = $headers[1].LineNumber
    $latest = $lines[$start-1..($next-2)]
}

$latest | Set-Content $OutputFile

Write-Output "最新版本内容已写入：$OutputFile"
