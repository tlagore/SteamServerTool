# Steam Server Tool
## Updates (Nov. 2024):
 - Got lazy with this project/life caught up. I've been using it to successfully run a Satisfactory server for quite some time. I switch between Valheim and Satisfactory just by changing the sst_config.json contents. It's quite robust and restarts automatically on VM reboots or if the server crashes.
   - Probaby not going to update this project unless I specifically want changes for running my own server
 - AutoUpdate logic was broken, should now work, but as a result the server is currently hardcoded to a satisfactory installation.
   - Annoyingly Satisfactory starts its own process when it starts, so the `Process` that was used to "start" the satisfactory server is not the one that is actually running the server. As a result, need to do a process lookup by name in order to shut it down
   - To genericize this, could add the "process name" as a part of the sst_config.json, and do process lookup by name for *every* server that is run to do a succesful shutdown
 - Added a few Quality of Life changes to the service itself so it actually starts up when run from the Windows Services window

## Old readme stuff:
This tool is being written in response to the steam public documentation pointing to this repo: https://github.com/C0nw0nk/SteamCMD-AutoUpdate-Any-Gameserver as the recommended method for maintaining auto-update.

This is my attempt to make the solution slightly more elegant by putting the logic in an executable that can be run as a service and gracefully exitted (cleaning up the running server by sending sigterm [ctrl+c] which is often the preferred method of shutting down a server so it can clean up). 

Furthermore the existing solution is in one large .bat file that is fairly difficult to read (not necessarily a fault of the author, bat language is just gross). I like to read programs before I blindly run them, I'm sure most people don't - but for those who do I'm trying to write this in a readable format such that they can understand what's happening and see that the code is not malicious.

I'll think about digitally signing the code should this pick up steam (lol).

## Possibly future features
Cool features that have no ETA or even reality of existing, but sounds need:

 - performing backups
 - possibly consider wrapping the tool in a GUI 

# Running the tool

Currently this tool only handles 1 server, I'll consider extending support to mutliple servers if there is demand.

`sst_config.json` is the only thing that needs to be filled out,

## Windows only

As this is being written on the .NET Framework, it is only available to Windows for the time being. I will look at porting this to .NET (Formerly .NET Core) to allow for cross platform usage.
