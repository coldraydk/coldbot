rm -rf ./bin/Release
dotnet publish -c Release -r ubuntu-x64
tar -zcvf laksebot.tar.gz -C ./bin/Release/netcoreapp2.1/ubuntu-x64/publish .
scp laksebot.tar.gz root@sejrsgaard-jacobsen.dk:/tmp/
ssh root@sejrsgaard-jacobsen.dk '/opt/deploy-laksebot.sh'
rm laksebot.tar.gz