Moonscraper Chart Editor, a song editor for Guitar Hero style rhythm games.

> **⚠️ NOTE: THIS IS NOT THE APPLICATION PROGRAM, THESE ARE THE SOURCE FILES. ⚠️**
>
> If you are looking to download Moonscraper Chart Editor please see the
> [releases page](https://github.com/FireFox2000000/Moonscraper-Chart-Editor/releases).

## Compiling from Source
Follow the instructions below for your desired platform to build and run from source.

### All Platforms
1. Download and install Unity 2018.4.23f1
2. Run Unity and open the project folder with it
3. Use the menu option Build Processes > Build Full Releases
Note that for Linux builds that the "Auto Reference" flag on Bass.Net.Linux needs to be manually toggled on before building, and disabled when making Windows builds. Suspected bug with Unity not filtering the platforms correctly. 

### Runtime dependencies (Windows)
Required runtime dependencies are included with the build.

### Runtime dependencies (Linux)
The application requires the following dependencies to be installed:
- `ffmpeg sdl2 libx11-6 libgtk-3-0`
- `libbass` (included with the build)

A [`PKGBUILD` file for Arch Linux](aur/PKGBUILD) is included in the repository.

Other distribution packagers can use the `PKGBUILD` file for reference.

## Who do I talk to?
* Alexander "FireFox" Ong
* YouTube- https://www.youtube.com/user/FireFox2000000
* Discord- FireFox#8188
* Twitter- https://twitter.com/FireFox2000000

## License
- See [attribution.txt](Assets/Documentation/attribution.txt) for third party libraries and resources included in this repository.
- See [LICENSE](LICENSE).
