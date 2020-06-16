#version 330 core

layout(location = 0) out vec3 gPosition;
layout(location = 1) out vec3 gNormal;
layout(location = 2) out vec4 gAlbedoSpec;

in VS_OUT {
	vec2 uv;
	vec3 normal;
	vec3 fragPos;
};


void main() {
	gPosition = fragPos;
	gNormal = normal; // normalize?
	gAlbedoSpec.rgb = vec3(1.0);
	gAlbedoSpec.a = 1.0;
}