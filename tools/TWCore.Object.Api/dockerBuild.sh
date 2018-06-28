﻿echo "Remove previous build"
rm -r app

echo "Creating folder structure..."
mkdir app

echo "Publishing project..."
dotnet publish -c Release -r linux-x64 -v q -o ./app/

echo "Building docker image"
docker build -t twcore_objectviewer:2.1.144 .
docker tag twcore_objectviewer:2.1.144 tonyredondo/twcore_objectviewer:2.1.144
docker push tonyredondo/twcore_objectviewer:2.1.144

echo "Remove build artifacts"
rm -r app