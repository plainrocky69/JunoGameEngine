#version 330 core

uniform ProjectionMatrixBuffer {
    mat4 projection_matrix;
};

in vec2 in_var_POSITION0;
in vec2 in_var_TEXCOORD0;
in vec4 in_var_COLOR0;

out vec4 color;
out vec2 texCoord;

void main() {
    gl_Position = projection_matrix * vec4(in_var_POSITION0, 0, 1);
    color = in_var_COLOR0;
    texCoord = in_var_TEXCOORD0;
}
