# AutoDemoRescue

A tool for people who use the JK2MV/NWH/EternalJK2MV AutoDemo (cl_autodemo 1) feature, but sometimes forget to press the saveDemo bind.

This tool will monitor your LastDemo folder and rescue any demos you forget to manually save, into a folder "AutoDemoRescue" next to your "LastDemo" folder, tagged with the date and time it was rescued.

## Prerequisites

This program has self-contained builds and small builds, as you can see in the Release section.

The self-contained builds contain the .NET 5.0 runtime, which is pure bloat, but you don't need to install anything to use them.

To use the small builds, install the Microsoft .NET 5.0 Runtime (if you don't have it installed yet): <https://dotnet.microsoft.com/en-us/download/dotnet/5.0>

## How to use

Open the config.ini file. It should look like this:

```
[main]
autoStart=0
path=
C:\examplepath\nwhclient\nwh\demos\LastDemo
C:\examplepath\eternalclient\eternaljk2\demos\LastDemo
C:\examplepath\jk2mvclient\base\demos\LastDemo
```

You can change the values to suit your personal usecase.

### autoStart

You can change this to 1 to have the application automatically start when you start your computer. Off by default. 

Only tested on Windows, but should work on Linux too. Probably doesn't work on Mac.

If you had this active previously and want to disable it, you can set it to 0 and run the program, it should remove itself.

### path

As you can see, you can have multiple paths set up for multiple clients.

You can overwrite each example path with your own, or add any amount of new paths.

The path must be a full path pointing to the LastDemo folder of a client where AutoDemo saves its temporary files.


### Run the program

When your config.ini is fully set up, simply run the program and leave it active in the background. It will monitor your demo folder and rescue any files if needed.

Press any key in the opened program window to close the program.

Be aware that: *IF YOU CLOSE THE PROGRAM OR FORGET TO START IT, IT WILL NOT WORK*