#version 300 es

// Inputs from raylib
uniform vec2 playerPos;   // Player position in screen coords
uniform float radius;     // View radius
uniform vec2 screenSize;  // Screen width & height

// Provided by raylib
in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;
uniform sampler2D texture0;

void main()
{
    vec2 fragCoord = fragTexCoord * screenSize;

    float dist = distance(fragCoord, playerPos);
    float visibility = smoothstep(radius + 20.0, radius, dist);

    vec4 texColor = texture(texture0, fragTexCoord);
    finalColor = vec4(texColor.rgb, texColor.a * visibility);
}