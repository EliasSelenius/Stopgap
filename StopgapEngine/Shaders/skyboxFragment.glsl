#version 330 core 

uniform samplerCube skybox;

in vec3 texCoords;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

void main() {
	FragColor = texture(skybox, texCoords) * 0.3;
	BrightColor = vec4(0.0);
}