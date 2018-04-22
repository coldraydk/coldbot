chmod a+x /home/travis/build/ksj2209/laksebot/src/Laksebot/bin/Release/netcoreapp2.1/ubuntu-x64/publish/LakseBot.dll
sshpass -p $FTP_PWD scp -o StrictHostKeyChecking=no -r /home/travis/build/ksj2209/laksebot/src/LakseBot/bin/Release/netcoreapp2.1/ubuntu-x64/publish/* $FTP_USER@$FTP_HOST:$FTP_DIR
sshpass -p $FTP_PWD ssh -o StrictHostKeyChecking=no $FTP_USER@$FTP_HOST 'sudo systemctl restart laskebot.service'
