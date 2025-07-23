A video game in development. Currently, it contains a maze that you have to traverse.

[GitHub Pages](https://prattbj.github.io/Labyrinth)

Controls: 
- WASD for movement
- M to view map, ESC to close map
- Left Click to shoot


command for publishing wasm:
 ```
 dotnet publish -c Release /p:DefineConstants=WASM
 ```

 command to serve on web
 ```
 dotnet serve --mime .wasm=application/wasm --mime .js=text/javascript --mime .json=application/json --directory bin\Release\net9.0\browser-wasm\AppBundle\
 ```

 command for building for Windows:

 ```
 dotnet publish -c Release
 ```