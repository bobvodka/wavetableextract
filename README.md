# wavetableextract

Quick tool to extract wavetable (wt) files to wav files

Currently can only deal with Surge/Bitwig formats.

Picks a "reasonable" sample rate for the resulting samples.

# Usage

- Compile
- Drop resulting executable, dll and runtimeconfig.json into a directory with .wt files you want to process
- Run program

The program will create a directory per .wt file found and place in it all the resulting wav files and a text file with some basic info about them.

Requires .Net 8.0
