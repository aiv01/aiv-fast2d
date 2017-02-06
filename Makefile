all: fast2d android ios

fast2d:
	nuget pack Aiv.Fast2D.nuspec

android:
	nuget pack Aiv.Fast2D.Android.nuspec

ios:
	nuget pack Aiv.Fast2D.iOS.nuspec

