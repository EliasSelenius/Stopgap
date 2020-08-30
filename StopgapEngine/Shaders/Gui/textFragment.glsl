#version 440 core

uniform sampler2D font_atlas;

uniform vec4 color;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in vec2 uv;

void main() {
	vec4 c = color;

    c *= texture(font_atlas, uv);
    if (c.a < 0.1) discard;		

	FragColor = c;
	BrightColor = vec4(0.0);
}