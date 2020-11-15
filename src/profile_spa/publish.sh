#!/bin/bash

echo "Compile elm."
    elm make src/Main.elm \
        --optimize \
        --output ../ids/wwwroot/profile_spa/app.js 

echo "Minify elm."
    uglifyjs ../ids/wwwroot/profile_spa/app.js -c "pure_funcs=[F2,F3,F4,F5,F6,F7,F8,F9,A2,A3,A4,A5,A6,A7,A8,A9],pure_getters,keep_fargs=false,unsafe_comps,unsafe" \
        | uglifyjs --mangle -o ../ids/wwwroot/profile_spa/app.js

echo "Copy index.js"
cp src/index.js ../ids/wwwroot/profile_spa/index.js

echo "Copy profile_spa.css"
cp src/profile_spa.css ../ids/wwwroot/profile_spa/profile_spa.css

echo "Copy favicon"
cp favicon.ico ../ids/wwwroot/favicon.ico

echo "Copy materialize"
cp -r materialize ../ids/wwwroot

tree --du ../ids/wwwroot