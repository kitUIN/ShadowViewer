$64Package = ".\ShadowViewer\bin\x64\Release\net6.0-windows10.0.19041.0\AppPackages"
$86Package = ".\ShadowViewer\bin\x86\Release\net6.0-windows10.0.19041.0\AppPackages"
$ARM64Package = ".\ShadowViewer\bin\ARM64\Release\net6.0-windows10.0.19041.0\AppPackages"
$Packages = @($64Package,$86Package,$ARM64Package)
$PackagesPath = ".\Packages"
if (!(Test-Path -Path $PackagesPath)) {
    New-Item -ItemType Directory -Path $PackagesPath
}
foreach ($element in $Packages) {
    if(Test-Path -Path $element)
    {
        Get-ChildItem $element | ForEach-Object -Process {
            if($_ -is [System.IO.DirectoryInfo])
            {
                $file = $_.name + ".zip"
                $ZipFile = Join-Path -Path $PackagesPath -ChildPath $file
                $source = Join-Path -Path $element -ChildPath $_.name
                if(Test-Path -Path $ZipFile)
                {
                    Remove-Item -Path $ZipFile
                }
                Compress-Archive -Path $source -DestinationPath $ZipFile
                Remove-Item -Path $source -Recurse
            }
        }
    }
}
