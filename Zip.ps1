Get-ChildItem -Include *Objectmodel.dll -Exclude *Tests.dll -Recurse | Compress-Archive -Update -DestinationPath ($PSScriptRoot + "\IronstoneObjectmodel")
Write-Host "Zip file created"