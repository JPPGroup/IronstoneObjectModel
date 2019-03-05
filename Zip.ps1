Get-ChildItem -Include *ObjectModel.dll -Exclude *Tests.dll -Recurse | Compress-Archive -Update -DestinationPath ($PSScriptRoot + "\IronstoneObjectModel")
Write-Host "Zip file created"