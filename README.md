# Wushu Utilities

This repository contains various tools related to Age of Wushu.

## Wushu.Utils.Package

This project allows you to unpack and repack `.package`-files. The application can unpack one file, or repack one directory (recursively) at a time. For multiple files or directories, it's recommended to write your own script that calls this application several times.

### Usage

From the command line, run the executable with the following parameters:

```
unpack <source file> <destination directory> # Unpack a package file into a directory
repack <source directory> <destination file> # Repack a directory into a package file
help                                         # Shows the help message
```

Caveats:
- When unpacking, if the destination directory does not exist, it is automatically created.
- When repacking, if the destination file already exists, it is overwritten.
- The repacker is configured to maximize compression efficiency. The outputted package files can be smaller than Age of Wushu's original package files.

### Automation

The application is designed to run in a CI/CD environment. On success, the application exits with the exit code `0`. On error, the application exits with the exit code `1`.
