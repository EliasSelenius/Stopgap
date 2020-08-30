#version 440 core

uniform sampler2D font_atlas;

uniform vec4 color;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in vec2 uv;

void main() {
	vec4 c = color;

	FragColor = c;
	BrightColor = vec4(0.0);
}