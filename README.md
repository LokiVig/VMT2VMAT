# ![VMT2VMAT-Icon](https://github.com/user-attachments/assets/a9abf17a-905a-46b2-8f96-04259479c47b) VMT2VMAT
This program is a simple C# project that translates a Source 1 VMT file to the Source 2 equivalent, a VMAT file. \
It will utilize VTFEdit to automatically turn its respective VTF files into the different, supported file extensions.

## How To Use
Using CMD you can launch the provided executable file with specified arguments. \
Those arguments are:
- "-vmt" - Specifies the path to a valid VMT file. (REQUIRED!)
- "-vmat" - Specifies the path to the resulting VMAT file.
- "-version" - Specifies which version of Source 2 we should translate to.
  - hla - Translates to a Half-Life: Alyx compatible VMAT file.
  - cs2 - Translates to a Counter-Strike 2 compatible VMAT file.
  - sbox - Translates to a s&box compatible VMAT file.
- "-fileextension" - Specifies what filetype the textures should feature.
    - tga - Textures will be exported and specified as targa (TGA) files.
    - png - Textures will be exported and specified as PNG files.
    - jpg / jpeg - Textures will be exported and specified as JPG files.
