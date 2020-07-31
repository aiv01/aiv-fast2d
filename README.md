# aiv-fast2d &middot; [![Nuget Version](https://img.shields.io/nuget/v/Aiv.Fast2D?color=blue)](https://www.nuget.org/packages/Aiv.Fast2D) [![Nuget Downloads](https://img.shields.io/nuget/dt/Aiv.Fast2D?color=yellow)](https://www.nuget.org/packages/Aiv.Fast2D) [![Api Doc](https://img.shields.io/badge/api--doc-read-blue)](http://aiv01.github.io/aiv-fast2d/) [![Build Status](https://travis-ci.org/aiv01/aiv-fast2d.svg?branch=master)](https://travis-ci.org/aiv01/aiv-fast2d)

Hardware accelerated 2D library, used by the first year students of AIV ("Accademia Italiana Videogiochi")

It is the base for aiv-fast3d too, that adds support for 2.5d and 3d games.

# Examples
Below a very basic usage example.

> More examples are available in [Example project](./aiv-fast2d-example).

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
sprite.DrawTexture(texture, xOffset, yOffset, width, height);
```


# Documentation
API documentation related to the last version of the library is published [here](http://aiv01.github.io/aiv-fast2d/).

Futhermore some in-depth guides are available for the following topics:
- [Handling Joystick input](./docs/Joysticks.md).
- [Creating Post-Processing effects](./docs/Postprocessing.md).

# Compliance
Library tested on:
* Visual Studio 2019 v16.6.4
* .NET Framework 4.8
* Any Cpu architecture

# Contribution
If you want to contribute to the project, please follow the [guidelines](CONTRIBUTING.md).
