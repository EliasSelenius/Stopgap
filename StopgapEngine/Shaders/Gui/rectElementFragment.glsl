#version 440 core

uniform sampler2D font_atlas;

uniform vec4 color;

out vec4 out_color;
in vec2 uv;

void main() {
	vec4 c = color;

	float r = 0.0;

	if (uv.y < r && uv.x < r && distance(uv, vec2(r)) > r) discard;
	if (uv.y > 1.0-r && uv.x > 1.0-r && distance(uv, vec2(1.0-r)) > r) discard;

	if (uv.y > 1.0-r && uv.x < r && distance(uv, vec2(r, 1.0-r)) > r) discard;
	if (uv.y < r && uv.x > 1.0-r && distance(uv, vec2(1.0-r, r)) > r) discard;

	out_color = c;
}