#!/bin/bash

function build_image {
    image_name=$1
    dockerfile=$2

    if [[ "$(docker images -q $image_name 2> /dev/null)" == "" ]]; then
        echo "Building $image_name"
        docker build -t $image_name -f $dockerfile .
    else
        echo "$image_name already exists"
    fi
}

build_image "db-image" "db.Dockerfile" &&
build_image "rabbit-image" "rabbit.Dockerfile" &&

docker run -d -p 5432:5432 --name db-container db-image
docker run -d -p 5672:5672 -p 15672:15672 --name rabbit-container rabbit-image

