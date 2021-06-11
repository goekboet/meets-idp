#!/bin/bash

docker container create --name dummy -v ids_dbs:/root hello-world
# docker cp $PWD/grants.db dummy:/root/
docker cp $PWD/users-seeded.db dummy:/root/users.db
docker rm dummy
