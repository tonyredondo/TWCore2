﻿echo "Remove previous build"
rm -r app

echo "Creating folder structure..."
mkdir app

echo "Publishing project..."
dotnet publish -c Release -r linux-x64 -v q -o ./app/

echo "Building docker image"
docker build -t twcore_diagnostics:2.0.147 .
docker tag twcore_diagnostics:2.0.147 tonyredondo/twcore_diagnostics:2.0.147
docker push tonyredondo/twcore_diagnostics:2.0.147

echo "Remove build artifacts"
rm -r app