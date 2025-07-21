// import { dotnet } from './_framework/dotnet.js'

// const { getAssemblyExports, getConfig } = await dotnet
//     .withDiagnosticTracing(false)
//     .create();

// const config = getConfig();
// const exports = await getAssemblyExports(config.mainAssemblyName);

// dotnet.instance.Module['canvas'] = document.getElementById('canvas');

// function mainLoop() {
//     exports.Labyrinth.Program.RunGame();

//     window.requestAnimationFrame(mainLoop);
// }

// await dotnet.run();
// window.requestAnimationFrame(mainLoop);

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
const width = (Math.floor(window.innerWidth * 3 / 4)).toString();

const height = (Math.floor(window.innerHeight * 3 / 4)).toString();
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