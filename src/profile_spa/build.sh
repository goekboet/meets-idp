echo "Compile elm."
elm make src/Main.elm --debug --output=../ids/wwwroot/profile_spa/app.js

echo "Copy index.js"
cp src/index.js ../ids/wwwroot/profile_spa/index.js

echo "Copy profile_spa.css"
cp src/profile_spa.css ../ids/wwwroot/profile_spa/profile_spa.css

ls -la ../ids/wwwroot/profile_spa