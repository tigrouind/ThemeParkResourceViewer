
This application allow to view/extract most resources used by Theme Park (1994) DOS version (floppy or CD-ROM).
You need to install [.NET Desktop Runtime 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) to run it.

# Instructions

Original game files are not included here since they are still copyrighted. You can get them from original CD / floppy or buy the game online (eg: GOG).

Click on `File` > `Set game location folder...` and select folder where game is installed. If you create a folder named `DATA` and put it in same place as the executable, it will automatically load files from there.

Support for playing music is experimental. The closest to the game is General Midi. You can get better results with [foobar2000](https://www.foobar2000.org/) and [foo_midi](https://www.foobar2000.org/components/view/foo_midi). 
Make sure you select `Bank: HMI (Theme Park)` in `Playback` > `Decoding` > `MIDI player` settings.

# Floppy version

The files `MUS.TAB`, `MUS.DAT`, `MUSFRA.ANI`, `MUSELE.ANI`, `INPAL.DAT` are RNC compressed and must be decompressed with a specific tool (otherwise they will be ignored). You can use what is provided on this [page](https://github.com/lab313ru/rnc_propack_source/releases).

To run it, put the following lines in a batch file and execute it:
```batch
rnc_propack_x64 u MUS.TAB MUS.TAB
rnc_propack_x64 u MUS.DAT MUS.DAT
rnc_propack_x64 u MUSFRA.ANI MUSFRA.ANI
rnc_propack_x64 u MUSELE.ANI MUSELE.ANI
rnc_propack_x64 u INPAL.DAT INPAL.DAT
```
