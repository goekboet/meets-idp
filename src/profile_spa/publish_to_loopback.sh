#!/bin/bash

./publish.sh

docker container create --name dummy -v loopback_static_files:/root hello-world
docker cp $PWD/dist dummy:/root/ids
docker rm dummy