# aiv-fast2d
Hardware accelerated 2D library

```cs
// open a window and get a OpenGL context
Window window = new Window(1024, 576, "Title");
```

```cs
// load a Texture in the graphics card
Texture texture = new Texture("path_to_texture_file");
```

```cs
// create a mesh formed by two triangles and use it as a sprite
// the mesh returned is a 100x100 pixels quad
Sprite sprite = new Sprite(100, 100);
```

```cs
// draw a sprite/mesh with the specified texture
sprite.DrawTexture(texture);
```
