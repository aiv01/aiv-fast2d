Using Joysticks with Aiv.Fast2D
-------------------------------

Aiv.Fast2D supports up to 4 jousticks/gamepad and emulates a subset of XBox 360 layout:

* AxisLeft, the left analog axis
* AxisRight, the right analog axis
* Right, right dpad
* Left, left dpad
* Up, up dpad
* Down, down dpad
* A, the a button
* B, the b button
* X, the x button
* Y, the y button
* Start, Back and 'Big Button'


To get the list of connected joysticks you can call:

```cs
string []joysticks = window.Joysticks;
```

an array of 4 strings will be returned. When null is set in an index, it means the joystick is not connected for that port

Debugging
---------

A good way to debug joysticks is using the JoystickDebug() method:

```cs
for(int i=0;i<4;i++) {
    Console.WriteLine(window.JoystickDebug(i);
}
```

Axis
----

The two axis returns a Vector2 struct

```cs
Vector2 leftAnalog = window.JoystickAxisLeft(0);
```

will return the left axis status of the first joystick.

It means you can move an object in one step:

```cs
// move the player with the left analog
player.position += window.JoystickAxisLeft(0) * window.deltaTime * speed;

// move the camera with the right one
camera.position += window.JoystickAxisRight(0) * window.deltaTime * cameraSpeed;
```

DPAD
----

This is the directional pad. Each of the directions is a bool set to true when the button is pressed:

```cs
// this time we use the second joystick attached to the system (if available)
bool isGoingUp = window.JoystickUp(1);
bool isGoingDown = window.JoystickDown(1);
bool isGoingLeft = window.JoystickLeft(1);
bool isGoingRight = window.JoystickRight(1);
```

Buttons
-------

The same as DPAD but for the classic 4 buttons (a,b,x,y).

```cs
bool pressA = window.JoystickA(0);
bool pressB = window.JoystickB(0);
bool pressX = window.JoystickX(0);
bool pressY = window.JoystickY(0);
bool pressStart = window.JoystickStart(0);
bool pressBack = window.JoystickBack(0);
bool pressBigButton = window.JoystickBigButton(0);
```

Supported Hardware
------------------

On windows you can use Xbox 360 and Xbox one controllers without additional drivers.

For PS3/PS4 controllers you need this driver: https://github.com/nefarius/ScpToolkit#installation-how-to

Cheap USB controllers should work out of the box

On Mac PS3/PS4 controllers are automatically supported, while for Xbox 360/One you need this driver: https://github.com/360Controller/360Controller/releases
