﻿#version 440 core

// vec2 position, vec2 size
uniform vec4 rectTransform;

layout (location = 0) in vec4 posuv;

out vec2 uv;

void main() {
	
	vec2 vp = posuv.xy;

	vec2 pos = rectTransform.xy + vp * rectTransform.zw;

	gl_Position = vec4(pos, 0.0, 1.0);
	uv = posuv.zw;
}