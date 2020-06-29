#version 440 core

uniform sampler2D font_atlas;

uniform vec4 color;

out vec4 out_color;
in vec2 uv;

void main() {
	vec4 c = color;

	out_color = c;
}