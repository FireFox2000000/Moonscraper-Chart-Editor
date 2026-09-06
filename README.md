> **⚠️ NOTE: THIS IS NOT THE APPLICATION PROGRAM, THESE ARE THE SOURCE FILES. ⚠️**
>
> If you are looking to download Moonscraper Chart Editor please see the
> [releases page](https://github.com/FireFox2000000/Moonscraper-Chart-Editor/releases).
>
> The releases page is the only official source for binary distribution. Any alternative sources should NOT be trusted. 

## About
Visit the [About page](https://firefox2000000.github.io/Moonscraper-Chart-Editor/) for more information and download links.

*Note that as Moonscraper Chart Editor 2 is currently in development this repository will no longer receive any major updates.

## Compiling from source 
Follow the instructions below for your desired platform to build and run from source.

### All Platforms
1. Download and install Unity 2018.4.23f1
2. Run Unity and open the project folder with it
3. Use the menu option Build Processes > Build Full Releases
  - Note that 7zip and Inno Setup are required to be installed to build distributables and installers respectively. 

### Runtime dependencies (Windows)
Required runtime dependencies are included with the build.

### Runtime dependencies (Linux)
The application requires the following dependencies to be installed:
- `ffmpeg sdl2 libx11-6 libgtk-3-0`
- `libbass` (included with the build)

A [`PKGBUILD` file for Arch Linux](aur/PKGBUILD) is included in the repository.

Other distribution packagers can use the `PKGBUILD` file for reference.

## License
- See [attribution.txt](https://github.com/FireFox2000000/Moonscraper-Chart-Editor/blob/master/Moonscraper%20Chart%20Editor/Assets/Documentation/attribution.txt) for third party libraries and resources included in this repository.
- See [LICENSE](LICENSE).
- The BASS audio library (a dependency of this application) is a commercial product. While it is free for non-commercial use, please ensure to obtain a valid licence if you plan on distributing any application using it commercially.
- The "Moonscraper" and "Moonscraper Chart Editor" branding and namesake are not covered by the licensing. Copyright to the application and it's branding is automatically granted to only Alexander "FireFox" Ong as per Australian copyright law. Although the source is free to use within the guidance of the linked license, do not use the Moonscraper Chart Editor namesake or branding imagery (such as the logo), attempt to act as the copyright holder, or violate copyright in any way without obtaining explicit permission from myself. 
