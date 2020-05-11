docker run \
    --mount type=bind,source="$(pwd)"/rotation,target=/app/rotation \
    test/ids-pki:0.0.1