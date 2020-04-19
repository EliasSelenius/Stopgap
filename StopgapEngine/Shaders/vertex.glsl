#version 330 core


layout (std140) uniform Camera {
	mat4 projection;
	mat4 view;
};


uniform mat4 obj_transform;
uniform float time;

layout (location = 0) in vec3 v_pos;
layout (location = 1) in vec2 v_uv;
layout (location = 2) in vec3 v_normal;


out VS_OUT {
	vec2 uv;
	vec3 normal;
	vec3 fragPos;
};

void main() {
	uv = v_uv;
	normal = normalize((obj_transform * vec4(v_normal, 0.0)).xyz);
	fragPos = (obj_transform * vec4(v_pos, 1.0)).xyz;

	gl_Position = projection * view * obj_transform * vec4(v_pos, 1.0);
}