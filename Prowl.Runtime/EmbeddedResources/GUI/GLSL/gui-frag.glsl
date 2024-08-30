#version 330 core

uniform sampler2D FontTexture;

in vec4 in_var_COLOR0;
in vec2 in_var_TEXCOORD0;

out vec4 outputColor;

void main() {
    outputColor = in_var_COLOR0 * texture(FontTexture, in_var_TEXCOORD0);
}
