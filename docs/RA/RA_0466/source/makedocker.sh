#/bin/bash

APPNAME="pydas"

echo Cleanup first
rm -f $APPNAME.tar.gz

echo Docker image build
docker build -t $APPNAME .

echo Docker image save to tar.gz
docker image save $APPNAME:latest | gzip > $APPNAME.tar.gz

echo Docker image cleanup on this system
docker image rm $APPNAME:latest

echo Docker image list for control
docker images
