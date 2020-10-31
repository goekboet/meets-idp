echo "Compile elm."
elm make src/Main.elm --debug --output=../ids/wwwroot/profile_spa/app.js

echo "Copy index.js"
cp src/index.js ../ids/wwwroot/profile_spa/index.js
ls -la ../ids/wwwroot/profile_spa