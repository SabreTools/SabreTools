# ![SabreTools Logo](images/sabretools-rect.png)

[![Build and Test](https://github.com/SabreTools/SabreTools/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/SabreTools/SabreTools/actions/workflows/build_and_test.yml)

## Introduction

The SabreTools project started as a desktop port of some core features of Wizard of DATz Redux, but it has since grown into a fully-featured DAT management tool. Below is a brief description of the features available in the suite. For more information about each feature, see the help text in each of the programs. For more information about the parent project, see [here](https://github.com/SabreTools/wizzardRedux).

> "tbh...  i have no real idea what sabretools is xD. my best guess  some prehistoric sort of swiss army knife" - **Executer**

## Features

For the most complete set of information, see the the [Wiki](https://github.com/SabreTools/SabreTools/wiki) or the [README.1ST](https://raw.githubusercontent.com/SabreTools/SabreTools/main/SabreTools/README.1ST) file. Currently, the SabreTools suite consists of the following programs:

### SabreTools / SabreTools Libraries

As the core of the SabreTools suite, the C# libraries provide nearly all of the functionality, while the main executable acts as a reference implementation. Below is a list of features included in the libraries and exposed though the executable:

* **Dir2DAT / DATFromDir** - Create a DAT from a file, folder, or mix of both
  * Archives and files can be filtered selectively
  * Aaruformat, Archives, and CHDs can have external hashes used
  * Multiple input archive formats are supported: 7zip, GZip, RAR, TAR, and ZIP/ZIP64
  * Multiple hashing algorithms available, up to SHA-512
  * Created DATs can be output in multiple formats at once, including ClrMamePro, Logiqx XML, and RomCenter
  * All DAT fields can be set for the output
  * Files with copier headers can be checked for additional hashes
* **DAT Conversion** - Convert DATs between numerous formats, including standards such as ClrMamePro, Logiqx XML, and RomCenter
  * Converted DATs can be output in multiple formats at once
  * Filtering can be performed based on criteria such as item type, sizes, hashes, and names
  * Input DATs can optionally be merged and diffed with each other
  * All DAT fields can be updated during the conversion
* **DAT Splitting** - Split DATs based on multiple different criteria
  * Split by user-defined extensions into two DATs
  * Split by best-available hash, e.g. SHA-1
  * Split a SuperDAT on the lowest available directory level
  * Split by DAT item types
* **DAT Statistics** - Get statistics on a DAT or folder of DATs
  * Includes items such as rom and game counts
  * Includes the total uncompressed size of all items
  * Optionally includes statistics on individual DATs in a folder
  * Reports can be output in standard text, CSV, and/or HTML formats
* **Rebuild From DAT** - Act as a simple rebuilder for files
  * Two options for rebuild origin: standard folder and Romba depot
  * Multiple output formats: unarchived, TAR, TorrentZip, 7zip, TorrentGZ
  * Can rebuild from multiple DATs at the same time
  * Cross-check with multiple hashing algorithms, up to SHA-512
  * Can output a fixdat based on the rebuilt files
  * Aaruformat and CHDs can be rebuilt either to folders or to TorrentGZ archives
* **Verify From DAT** - Act as a simple verifier for files
  * Two options for verification target: standard folder and Romba depot
  * Exact verification and hash-only verification both available
  * Can verify from multiple DATs at the same time
  * Cross-check with mutliple hashing algorithms, up to SHA-512
  * Aaruformat and CHDs can be treated like files

This tool has a comprehensive list of command line parameters that can be used to do the above and much more.

### Headerer

A small tool that allows users to extract, store, and remove copier headers for a variety of systems. Optionally, the stored headers can be replaced on the original files using a separate command. Each of the headers are stored in a Sqlite database file that links each header to the unheadered hash of the original file. The following copier header types are supported:

* Atari 7800
* Atari Lynx
* Commodore 64 PSID music
* NEC PC-Engine / TurboGrafx 16
* Nintendo Famicom Disk System
* Nintendo Entertainment System / Famicom
* Super Nintendo Entertainment System / Super Famicom
* Super Nintendo Entertainment System / Super Famicom SPC music

This tool is no longer hosted in this repository. If you wish to use this tool, you can find it [here](https://github.com/SabreTools/SabreTools.Skippers/).

## Builds

For the most recent stable build, download the latest release here: [Releases Page](https://github.com/SabreTools/SabreTools/releases)

For the latest WIP build here: [Rolling Release](https://github.com/SabreTools/SabreTools/releases/rolling)

## Contributions

Any feature suggestions or code contributions are accepted. Please take the time to look over the existing code to see proper coding and documentation patterns.

Contributors are attributed in the code and in the README.
