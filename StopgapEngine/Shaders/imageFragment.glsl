#version 330 core

uniform sampler2D colorBuffer;
uniform sampler2D brightnessBuffer;
uniform float exposure = 2.0;

in vec2 uv;

out vec4 FragColor;

vec4 reztex(in sampler2D buf, float rez) {
	return texture(buf, floor(uv * rez) / rez);
}

void main() {
	vec4 color = texture(colorBuffer, uv);

	vec4 bColor = texture(brightnessBuffer, uv);

	color += bColor;

	// tone mapping (HDR -> LDR):
	//color = color / (color + vec4(1.0));
	color = vec4(1.0) - exp(-color * exposure);

	// gamma correction:
	color.rgb = pow(color.rgb, vec3(1.0/2.2));

	FragColor = color;
}