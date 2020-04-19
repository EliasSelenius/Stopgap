#version 440 core

uniform sampler2D font_atlas;
uniform bool is_text;

uniform vec4 color;

out vec4 out_color;
in vec2 uv;

void main() {
	

	vec4 c = color;

	if (is_text) {
		c *= texture(font_atlas, uv);
		if (c.a < 0.5) discard;		
	} else {
		float r = 0.0;

		if (uv.y < r && uv.x < r && distance(uv, vec2(r)) > r) discard;
		if (uv.y > 1.0-r && uv.x > 1.0-r && distance(uv, vec2(1.0-r)) > r) discard;

		if (uv.y > 1.0-r && uv.x < r && distance(uv, vec2(r, 1.0-r)) > r) discard;
		if (uv.y < r && uv.x > 1.0-r && distance(uv, vec2(1.0-r, r)) > r) discard;
	}

	out_color = c;
}