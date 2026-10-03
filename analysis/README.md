# PS3 Free Realms IDA setup

- PS3 binary: `D:\RPCS3\dev_hdd0\game\NPEA00299\USRDIR\game.elf`.
- Working analysis copy: `game.elf` in this directory; IDA database: `game.elf.i64`.
- PC reference: `C:\Users\bobya\Downloads\forgelight idbs\FreeRealms_2014-03-13.exe.i64`.
- Loader: [krystalgamer/ps3_aero_loader](https://github.com/krystalgamer/ps3_aero_loader), rebuilt against [HexRaysSA/ida-sdk](https://github.com/HexRaysSA/ida-sdk) tag `v9.3.0-sdk.3` for IDA 9.3. The local source and SDK are under `../tools/`.
- Installed loader files: `%APPDATA%\Hex-Rays\IDA Pro\loaders\ps3_aero_loader.dll` and `ps3.xml`.

The PS3 ELF is big-endian PPC64 with CellOS ABI 102. Its ELF entry `0xEEB540` is a function descriptor, not executable code. The descriptor points to code at `0x10230` and TOC `0xF1F758`. IDA decompiled `0x10230`, resolved PS3 import names such as `.cellSysutilUnregisterCallback`, and completed analysis with 30,903 functions. The working database was saved after analysis. Use the code address from a descriptor when decompiling a function.

The first direct load in the RPCS3 game directory crashed during analysis. Work from the copy here, with the rebuilt PS3 loader installed. The prior database beside the RPCS3 binary is not the working analysis database.
