namespace Ignis.Gfx.Backends.OpenGL;

/// <summary>
/// Default GLSL shaders for the OpenGL backend.
/// </summary>
internal static class DefaultShaders
{
    public const string Shader3DVertex = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec4 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;
out vec4 VertexColor;

void main()
{
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    TexCoord = aTexCoord;
    VertexColor = aColor;
    gl_Position = uProjection * uView * worldPos;
}
";

    // Simple unlit shader - just outputs vertex color (good for basic samples)
    public const string Shader3DFragment = @"
#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec4 VertexColor;

out vec4 FragColor;

void main()
{
    FragColor = VertexColor;
}
";

    public const string Shader2DVertex = @"
#version 330 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;

uniform mat4 uProjection;

out vec2 TexCoord;
out vec4 VertexColor;

void main()
{
    TexCoord = aTexCoord;
    VertexColor = aColor;
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}
";

    public const string Shader2DFragment = @"
#version 330 core

in vec2 TexCoord;
in vec4 VertexColor;

uniform sampler2D uTexture;
uniform bool uUseTexture;

out vec4 FragColor;

void main()
{
    if (uUseTexture)
        FragColor = texture(uTexture, TexCoord) * VertexColor;
    else
        FragColor = VertexColor;
}
";

    public const string ShaderTextVertex = @"
#version 330 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;

uniform mat4 uProjection;

out vec2 TexCoord;
out vec4 VertexColor;

void main()
{
    TexCoord = aTexCoord;
    VertexColor = aColor;
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}
";

    public const string ShaderTextFragment = @"
#version 330 core

in vec2 TexCoord;
in vec4 VertexColor;

uniform sampler2D uTexture;

out vec4 FragColor;

void main()
{
    float alpha = texture(uTexture, TexCoord).r;
    FragColor = vec4(VertexColor.rgb, VertexColor.a * alpha);
}
";
}

