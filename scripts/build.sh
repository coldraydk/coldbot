#!/bin/bash
cd src/coldbot
rm -rf ./bin/Release
dotnet publish -c Release -r ubuntu-x64