﻿echo "Remove previous build"
rm -r app

echo "Creating folder structure..."
mkdir app

echo "Publishing project..."
dotnet clean -c Release -r linux-x64
dotnet build -c Release -r linux-x64
dotnet publish -c Release -r linux-x64 -v q -o ./app/

echo "Building docker image"
docker build -t twcore_diagnostics:2.1.169 .
docker tag twcore_diagnostics:2.1.169 tonyredondo/twcore_diagnostics:2.1.169
docker push tonyredondo/twcore_diagnostics:2.1.169

echo "Remove build artifacts"
rm -r app