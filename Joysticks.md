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
player.position = window.JoystickAxisLeft(0) * window.deltaTime * speed;
```
