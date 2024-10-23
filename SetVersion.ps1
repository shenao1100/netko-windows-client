$commitHash = git rev-parse --short HEAD
(Get-Content .\\Netko.Desktop\\Netko.Desktop.withoutVersion.csproj) -replace "COMMIT_HASH", $commitHash | Set-Content .\\Netko.Desktop\\Netko.Desktop.csproj
(Get-Content .\\Netko\\Netko.withoutVersion.csproj) -replace "COMMIT_HASH", $commitHash | Set-Content .\\Netko\\Netko.csproj
