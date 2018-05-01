#!/bin/bash
LAKSEBOT_HOME=/home/travis/build/ksj2209/laksebot
cd ${LAKSEBOT_HOME}/src/LakseBot/bin/Release/netcoreapp2.1/ubuntu-x64/publish
chmod a+x LakseBot.dll

if ! [[ -d ${LAKSEBOT_HOME}/dist ]]; then
    mkdir -p ${LAKSEBOT_HOME}/dist
fi

zip -r ${LAKSEBOT_HOME}/dist/laksebot.zip *
sshpass -p $FTP_PWD scp -o StrictHostKeyChecking=no -r ${LAKSEBOT_HOME}/dist/laksebot.zip $FTP_USER@$FTP_HOST:$FTP_DIR
sshpass -p $FTP_PWD ssh -o StrictHostKeyChecking=no $FTP_USER@$FTP_HOST './deploy.sh'
