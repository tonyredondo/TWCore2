﻿162echo "Remove previous build"
rm -r app

echo "Creating folder structure..."
mkdir app

echo "Publishing project..."
dotnet clean -c Release -r linux-x64
dotnet build -c Release -r linux-x64
dotnet publish -c Release -r linux-x64 -v q -o ./app/

echo "Building docker image"
docker build -t twcore_objectviewer:2.1.168 .
docker tag twcore_objectviewer:2.1.168 tonyredondo/twcore_objectviewer:2.1.168
docker push tonyredondo/twcore_objectviewer:2.1.168

echo "Remove build artifacts"
rm -r app