# EulerianFluidSim
A visual 2d fluid simulation with smoke/dye flowing thru a pipe with an obstacle to add cool turbulence patterns.

# Available Platforms
The sim runs on windows with Winforms, crossplatform with .NetMaui, and in a browser with Blazor.

# Performance
The sim runs on all platforms purely on the CPU so the simulations and is intensive.  The output is an array of color bit which need to be dislayed.

Winforms offers the best framerate overall. 

The Maui version is slower, but still useable on Windows.  Very slow on Android.  The slower Windows performance is from how the image bits to delivered to the screen.

The web version is very very slow.  The simulation running in WebAssembly might be a slow point, but this hos not been tested.  Delivering the image bits to Lavascript seems to be the limiting factor.





