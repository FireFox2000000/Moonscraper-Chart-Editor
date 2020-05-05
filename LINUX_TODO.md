# What's needed to get Linux working on Moonscraper?

## SDL (Plugin)
  Moonscraper relies on SDL2 for it's input management.
  ```see \Assets\Scripts\Engine\OSNative\MessageBox``` via the ```SDL_ShowSimpleMessageBox``` function.
  
  However for Linux, an SDL2 cannot be directly downloaded from [the official download link](https://www.libsdl.org/download-2.0.php), 
  as it states "Please contact your distribution maintainer for updates".
  A potential runtime library for SDL2 has been provided under ```Assets/Plugins/SDL/Linux```, however for actual builds 
  these libraries may need to be copied into the same location as the executable. Untested how this will actually work.
  
  If there are issues with the C# wrapper it may be a good idea to visit [the github page for SDL issues on Linux](https://github.com/flibitijibibo/SDL2-CS/issues?q=linux) for a solution.
  
## Window handle
  A ```NativeWindow_Linux``` skeleton class has already been created, all it needs is implementation of the
  SetApplicationWindowPointerByName method, which takes the expected window name of Moonscraper and is 
  expected to call the base class ```functionSetWindowPtrFromNative(windowPtr)``` to set the base class
  ```sdlWindowPtr``` variable, which can be passed around to ```NativeMessageBoxLinux```.
  
## Message box
  A ```NativeMessageBoxLinux``` skeleton class has already been created, all it needs is implementation of the interface methods.
  
## FileExplorer
  The native file explorer has an already untested solution based on [this repo](https://github.com/gkngkc/UnityStandaloneFileBrowser).
  However the methods I have copy-pasted are untested. It would also be nice to somehow pass the window handle from above
  down to this file explorer so that the windows are parented properly, although not strictly required.
  
  The solution from gkngkc's repro also only provides a 64bit version of the plugin. Meaning without a 32bit version of this then Linux builds will have to be restricted to 64bit only. Not that big of a deal, but again, would be nice for completeness' sake.
  
## Conclusion
That is what is definitely known to get this working. There may be other issues that may crop up upon getting actual
builds working to to circumstances not allowing me to easily test Linux myself at present.
