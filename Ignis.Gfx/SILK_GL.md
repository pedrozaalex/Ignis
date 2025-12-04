This guide serves as a comprehensive reference for using OpenGL with **Silk.NET**, based on the provided tutorial source code. It covers project setup, core abstractions, shader management, mathematical transformations, and advanced rendering techniques like lighting and model loading.

---

# Silk.NET OpenGL Reference Guide

## 1. Project Configuration

### Dependencies
Silk.NET is modular. For a standard OpenGL 3.3+ application, your `.csproj` should reference these packages:

*   **Silk.NET.Windowing**: Manages the window, context creation, and the main loop.
*   **Silk.NET.Input**: Handles Keyboard, Mouse, and Gamepad input.
*   **Silk.NET.OpenGL**: The raw bindings to the OpenGL API.
*   **Silk.NET.Assimp**: (Optional) For loading 3D models.
*   **SixLabors.ImageSharp**: (Optional) For loading textures.

### Critical Settings
OpenGL interaction often requires pointer arithmetic. You must enable unsafe blocks in your project file.

```xml
<PropertyGroup>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

---

## 2. Windowing & The Main Loop

Silk.NET uses an `IWindow` interface to abstract the platform-specific window creation.

### Initialization Pattern
```csharp
using Silk.NET.Windowing;
using Silk.NET.Maths;

// 1. Configure
var options = WindowOptions.Default;
options.Size = new Vector2D<int>(800, 600);
options.Title = "My GL Engine";

// 2. Create
IWindow window = Window.Create(options);

// 3. Subscribe to Events
window.Load += OnLoad;          // Runs once on startup
window.Update += OnUpdate;      // Runs every frame (logic)
window.Render += OnRender;      // Runs every frame (draw)
window.FramebufferResize += OnResize; 

// 4. Run
window.Run();
```

### The OpenGL Context
You obtain the API instance inside `OnLoad`. This instance (`GL`) is used for all subsequent calls.

```csharp
private static GL Gl;

private static void OnLoad() {
    Gl = GL.GetApi(window);
}
```

---

## 3. Core Abstractions (Buffers & VAOs)

Raw OpenGL requires repetitive boilerplate. The provided tutorials use Generic classes to abstract this.

### BufferObject (`VBO` & `EBO`)
Wraps `GenBuffer`, `BindBuffer`, and `BufferData`. It handles uploading C# arrays (Span) to GPU memory.

*   **Target:** `BufferTargetARB.ArrayBuffer` (Vertices) or `ElementArrayBuffer` (Indices).
*   **Usage:**
    ```csharp
    // VBO
    Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
    // EBO
    Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
    ```

### VertexArrayObject (`VAO`)
The VAO stores the state of how the GPU should interpret the VBO data (layout).

*   **Key Method:** `VertexAttribPointer`
    *   **index:** Shader layout location (e.g., `layout (location = 0)`).
    *   **count:** Components per vertex (e.g., 3 for x,y,z).
    *   **type:** Data type (usually `Float`).
    *   **vertexSize:** Total size of one vertex (Stride) in element count (e.g., 5 for x,y,z,u,v).
    *   **offSet:** Offset in the array where this attribute starts.

```csharp
// Example: Vertex (x,y,z) at loc 0, UV (u,v) at loc 1
Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);
Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0); 
Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
```

---

## 4. Shaders

Silk.NET provides direct bindings to compile and link GLSL shaders.

### The Shader Class Workflow
1.  **Read Source:** `File.ReadAllText(path)`.
2.  **Create Shader:** `Gl.CreateShader(ShaderType.VertexShader)`.
3.  **Compile:** `Gl.CompileShader`.
    *   *Check Errors:* `Gl.GetShaderInfoLog`.
4.  **Create Program:** `Gl.CreateProgram`.
5.  **Attach & Link:** `Gl.AttachShader`, `Gl.LinkProgram`.
    *   *Check Errors:* `Gl.GetProgramInfoLog`.
6.  **Cleanup:** Detach and delete individual shader objects after linking.

### Setting Uniforms
Uniforms communicate data from C# to GLSL.
```csharp
// Primitive
public void SetUniform(string name, int value) {
    int loc = _gl.GetUniformLocation(_handle, name);
    _gl.Uniform1(loc, value);
}

// Matrices (System.Numerics.Matrix4x4)
public unsafe void SetUniform(string name, Matrix4x4 value) {
    int loc = _gl.GetUniformLocation(_handle, name);
    // count=1, transpose=false
    _gl.UniformMatrix4(loc, 1, false, (float*) &value); 
}
```

---

## 5. Textures

Textures involve loading an image file, extracting raw bytes, and sending them to OpenGL.

### Libraries
*   **StbImageSharp**: Lightweight, good for simple loading.
*   **SixLabors.ImageSharp**: More robust, used in later tutorials.

### Workflow
1.  **Generate:** `_gl.GenTexture()`.
2.  **Bind:** `_gl.BindTexture(TextureTarget.Texture2D, handle)`.
3.  **Parameters:** Set Wrapping (Repeat/Clamp) and Filtering (Linear/Nearest).
4.  **Upload:**
    ```csharp
    // Using ImageSharp to get a pointer to pixel data
    img.ProcessPixelRows(accessor => {
       fixed (void* data = accessor.GetRowSpan(y)) {
           gl.TexSubImage2D(..., data);
       }
    });
    ```
5.  **Mipmaps:** `_gl.GenerateMipmap`.

### Binding
In the render loop, you must activate a texture slot before binding.
```csharp
Gl.ActiveTexture(TextureUnit.Texture0);
Gl.BindTexture(TextureTarget.Texture2D, _handle);
Shader.SetUniform("uTexture", 0); // Tell shader to read from slot 0
```

---

## 6. Math & Coordinate Systems

Silk.NET relies heavily on `System.Numerics` for data types, which are hardware accelerated.

### The Pipeline
1.  **Model Matrix:** Transforms local object coordinates to World Space (Translation, Rotation, Scale).
2.  **View Matrix:** Transforms World Space to Camera Space.
3.  **Projection Matrix:** Transforms Camera Space to Clip Space (Perspective/Orthographic).

### Helper Class (`MathHelper`)
OpenGL expects radians, but humans prefer degrees.
```csharp
public static float DegreesToRadians(float degrees) => MathF.PI / 180f * degrees;
```

### Camera Implementation
A Camera class abstracts the View and Projection matrices.
*   **LookAt Matrix:** `Matrix4x4.CreateLookAt(Position, Position + Front, Up)`
*   **Perspective Matrix:** `Matrix4x4.CreatePerspectiveFieldOfView(...)`
*   **Euler Angles:** Pitch (Y-axis look), Yaw (X-axis look). To avoid Gimbal lock, calculate direction vectors using Sin/Cos of Pitch/Yaw.

---

## 7. Lighting

The tutorials implement **Phong Lighting** (Ambient + Diffuse + Specular).

### GLSL Structure
*   **Ambient:** `lightColor * ambientStrength`
*   **Diffuse:** `max(dot(normal, lightDir), 0.0) * lightColor`
*   **Specular:** `pow(max(dot(viewDir, reflectDir), 0.0), shininess)`

### Materials & Light Maps
Instead of single colors, textures are used for lighting properties:
*   **Diffuse Map:** The texture color of the object.
*   **Specular Map:** Black and white texture defining how shiny specific parts of the object are.

In the shader, you sample these maps:
```glsl
vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
vec3 specular = light.specular * (spec * texture(material.specular, TexCoords).rgb);
```

---

## 8. Model Loading (Assimp)

Loading complex 3D models (.obj, .fbx) requires parsing geometry. `Silk.NET.Assimp` handles this.

### The Mesh Class
A container for the renderable data extracted from Assimp:
*   `List<Vertex>` (Position, Normal, TexCoords)
*   `List<uint>` (Indices)
*   `List<Texture>`

It creates its own `VAO`, `VBO`, and `EBO` internally upon instantiation.

### The Model Class
1.  **Import:** `_assimp.ImportFile(path, ...)`
2.  **Process Node:** Recursively traverse the Assimp node tree.
3.  **Process Mesh:**
    *   Convert `assimpMesh->MVertices` to `Vertex` struct.
    *   Convert `assimpMesh->MFaces` to Indices array.
    *   Load Material Textures associated with the mesh.

---

## 9. Common Render Loop Pattern

A typical `OnRender` method in Silk.NET looks like this:

```csharp
private static unsafe void OnRender(double deltaTime)
{
    // 1. Clear Screen & Depth Buffer
    Gl.Enable(EnableCap.DepthTest);
    Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

    // 2. Bind Shader
    Shader.Use();

    // 3. Update Uniforms (Matrices)
    var view = Camera.GetViewMatrix();
    var projection = Camera.GetProjectionMatrix();
    Shader.SetUniform("uView", view);
    Shader.SetUniform("uProjection", projection);

    // 4. Bind Textures
    Texture.Bind(TextureUnit.Texture0);

    // 5. Bind Geometry (VAO) and Draw
    Vao.Bind();
    // Indexed Draw
    Gl.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, null);
    // OR Array Draw
    // Gl.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);
}
```

## 10. Key Best Practices identified in code
1.  **Disposable Pattern:** All OpenGL objects (Buffers, Textures, Shaders, VAOs) implement `IDisposable` to call `Gl.Delete*`. Always dispose of them in the `OnClose` event.
2.  **Unsafe Code:** Silk.NET bindings for functions taking pointers (like `VertexAttribPointer` or `DrawElements` with offset) require `unsafe` contexts.
3.  **State Machine:** OpenGL is a state machine. If you bind a VAO, it stays bound until you bind another (or 0). The `Bind()` methods in the abstractions help manage this explicitly.