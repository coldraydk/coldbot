LAKSEBOT_HOME=/home/travis/build/ksj2209/laksebot
ROOT=`pwd`
cd ${LAKSEBOT_HOME}/src/LakseBot/bin/Release/netcoreapp2.1/ubuntu-x64/publish
chmod a+x LakseBot.dll
zip -r ${ROOT}/dist/laksebot.zip *
sshpass -p $FTP_PWD scp -o StrictHostKeyChecking=no -r ${ROOT}/dist/laksebot.zip $FTP_USER@$FTP_HOST:$FTP_DIR
sshpass -p $FTP_PWD ssh -o StrictHostKeyChecking=no $FTP_USER@$FTP_HOST './deploy'
