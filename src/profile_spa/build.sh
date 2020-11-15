echo "Compile elm."
elm make src/Main.elm --debug --output=../ids/wwwroot/profile_spa/app.js

echo "Copy index.js"
cp src/index.js ../ids/wwwroot/profile_spa/index.js

echo "Copy profile_spa.css"
cp src/profile_spa.css ../ids/wwwroot/profile_spa/profile_spa.css

echo "Copy favicon"
cp favicon.ico ../ids/wwwroot/favicon.ico

echo "Copy materialize"
cp -r materialize ../ids/wwwroot

tree --du ../ids/wwwroot