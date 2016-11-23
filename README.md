# aiv-fast2d
Hardware accelerated 2D library

```cs
// open a window and get a OpenGL context
Window window = new Window(1024, 576, "Title");

// open a window in full screen mode
Window window = new Window(1920, 1080, "Title", true);

// open a window in fullscreen mode with the current native resolution
Window window = new Window("Title");
```

```cs
// load a Texture in the graphics card
Texture texture = new Texture("path_to_image_file");
```

```cs
// create a mesh formed by two triangles and use it as a sprite
// the mesh returned is a 100x100 pixels quad
Sprite sprite = new Sprite(100, 100);

// move the sprite
sprite.position = new Vector2(x, y);
//rotate the sprite on the z axis (in degrees)
sprite.EulerRotation = 90;
// scale a sprite
sprite.scale = new Vector2(0.5f, 0.5f);
```

```cs
// draw a sprite/mesh with the specified texture
sprite.DrawTexture(texture);
// draw only a part of the the texture in the sprite/mesh
sprite.DrawTexture(texture, xoffset, yoffset, width, height);
```
