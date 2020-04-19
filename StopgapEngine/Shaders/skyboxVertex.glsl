#version 330 core 

layout (std140) uniform Camera {
	mat4 projection;
	mat4 view;
};

layout (location = 0) in vec3 v_pos;

out vec3 texCoords;

void main() {
	vec4 pos = projection * mat4(mat3(view)) * vec4(v_pos, 1.0);

	gl_Position = pos.xyww;
	texCoords = v_pos;
}
