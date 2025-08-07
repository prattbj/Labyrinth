#version 300 es
precision mediump float;

// Uniforms
uniform vec2 playerPos;
uniform float radius;
uniform vec2 screenSize;

// Inputs
in vec2 fragTexCoord;
in vec4 fragColor;

// Texture sampler
uniform sampler2D texture0;

// Output
out vec4 finalColor;

void main()
{
    vec2 fragCoord = fragTexCoord * screenSize;

    float dist = distance(fragCoord, playerPos);
    float visibility = smoothstep(radius + 20.0, radius, dist);

    vec4 texColor = texture(texture0, fragTexCoord);
    finalColor = vec4(texColor.rgb, texColor.a * visibility);
}