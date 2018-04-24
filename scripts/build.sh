#!/bin/bash
cd src/LakseBot
rm -rf ./bin/Release
dotnet publish -c Release -r ubuntu-x64