import { dotnet } from './_framework/dotnet.js'
const canvas = document.getElementById('canvas');
canvas.style.position = "absolute";
canvas.style.top = 0;
canvas.style.right = 0;
canvas.style.bottom = 0;
canvas.style.left = 0;
if (!canvas) {
    console.error("Canvas element not found");
}
const width = (Math.floor(window.innerWidth * 15 / 16)).toString();

const height = (Math.floor(window.innerHeight * 15 / 16)).toString();
const { getAssemblyExports, getConfig, runMain } = await dotnet
    .withApplicationArguments(width, height)
    .create();


const config = await getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
window.requestAnimationFrame = window.requestAnimationFrame || // not sure if these do anything
                               window.mozRequestAnimationFrame ||
                               window.webkitRequestAnimationFrame ||             
                               window.msRequestAnimationFrame; 
dotnet.instance.Module['canvas'] = canvas;

function mainLoop() {
    exports.Labyrinth.Program.RunGame();
    
    window.requestAnimationFrame(mainLoop);
}

await runMain(); 
window.requestAnimationFrame(mainLoop);