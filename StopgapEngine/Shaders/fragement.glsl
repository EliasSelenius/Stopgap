#version 330 core


in VS_OUT {
	vec2 uv;
	vec3 normal;
	vec3 fragPos;
} vInput;



//======DATASTRUCTURES========

struct PointLight {
	vec3 pos;
	
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;
};

struct DirLight {
	vec3 dir;
	
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct Material {

	sampler2D diffuseTexture;
	vec3 diffuseColor;

	vec3 emission;

	vec3 specular;
	float shininess;
};

//=============================


vec3 CalcDirLight(DirLight light, vec3 n, vec3 viewDir, Material material) {
	vec3 lightDir = normalize(-light.dir);
	// diffuse
	float diffscale = max(dot(n, lightDir), 0.0);
	// specular
	vec3 halfwaydir = normalize(lightDir + viewDir);
	float specscale = pow(max(dot(n, halfwaydir), 0.0), material.shininess * 128.0 * 3.0);

	vec3 c = vec3(texture(material.diffuseTexture, vInput.uv)) + material.diffuseColor;

	vec3 ambient = light.ambient * c;
	vec3 diffuse = light.diffuse * c * diffscale;
	vec3 specular = light.specular * material.specular * specscale;

	return (material.emission + ambient + diffuse + specular);
}

/*
// note this only uses Phong and not blinn-phong
vec3 CalcPointLight(PointLight light, vec3 n, vec3 fp, vec3 viewDir, Material material) {
	vec3 lightDir = normalize(light.pos - fp);
	// diffuse
	float diffscale = max(dot(n, lightDir), 0.0);
	// specular
	float specscale = pow(max(dot(viewDir, reflect(-lightDir, n)), 0.0), material.shininess * 128.0);
	// attenuation
	float lightdist = length(light.pos - fp);
	float attenuation = 1.0 / (light.constant + light.linear * lightdist + light.quadratic * (lightdist * lightdist));


	vec3 ambient = light.ambient * material.ambient * attenuation;
	vec3 diffuse = light.diffuse * material.diffuse * diffscale * attenuation;
	vec3 specular = light.specular * material.specular * specscale * attenuation;
	return ambient + diffuse + specular;
}*/


uniform DirLight dirLight;
//uniform PointLight pointLight;
uniform vec3 cam_pos;

uniform Material material;
uniform vec4 tint;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

void main() {
	vec3 color = vec3(0.0);
	vec3 camdir = normalize(cam_pos - vInput.fragPos);

	color += CalcDirLight(dirLight, vInput.normal, camdir, material);
	//color += CalcPointLight(pointLight, normal, fragPos, camdir);
	
	FragColor = vec4(color, 1.0) + tint;


	// create brightness color
	float brightness = dot(FragColor.rgb, vec3(0.2126, 0.7152, 0.0722));
	BrightColor = vec4(FragColor.rgb * step(1.0, brightness), 1.0);
	//BrightColor = vec4(material.emission, 1.0);
}


