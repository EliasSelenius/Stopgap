﻿#version 330 core

layout (location = 0) in vec2 vPos;

out vec2 uv;

void main() {
	uv = (vPos + vec2(1.0)) * 0.5;
	gl_Position = vec4(vPos, 0.0, 1.0);
}