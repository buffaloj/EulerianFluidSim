# EulerianFluidSim
A visual 2d fluid simulation with smoke/dye flowing thru a pipe with an obstacle in the middle to add cool turbulence patterns.

![](fluidsim.gif)

# How to Run on Windows
1. Pull down the code
2. Open EulerianFluidSim.sln in Visual Studio
3. Set the Startup Project of choice:
  - SimRunnerApp.Winforms (best performance)
  - SimRunnerApp.Maui
  - SimRunnerApp.Blazor
3. Run

# Available Platforms
The sim runs on windows with Winforms, crossplatform with .NetMaui, and in a browser with Blazor.

# Performance
The sim runs on all platforms purely on the CPU and the simulations is fairly intensive. The resulting per-platform framerate mainly depends on how long it takes to flip snapshots to the screen.

Winforms offers the best framerate overall. 

The Maui version is slower, but still useable on Windows.  Very slow on Android.

The web version is very very slow.  The simulation might run slower in WebAssembly than native .Net would, but haven't tested either way.  Delivering the image bits to Javascript seems to be the limiting factor.





