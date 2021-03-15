# Steam Server Tool

This tool is being written in response to the steam public documentation pointing to this repo: https://github.com/C0nw0nk/SteamCMD-AutoUpdate-Any-Gameserver as the recommended method for maintaining auto-update.

This is my attempt to make the solution slightly more elegant by putting the logic in an executable that can be run as a service and gracefully exitted (cleaning up the running server by sending sigterm [ctrl+c] which is often the preferred method of shutting down a server so it can clean up). 

Furthermore the existing solution is in one large .bat file that is fairly difficult to read (not necessarily a fault of the author, bat language is just gross). I like to read programs before I blindly run them, I'm sure most people don't - but for those who do I'm trying to write this in a readable format such that they can understand what's happening and see that the code is not malicious.

I'll think about digitally signing the code should this pick up steam (lol).

## Possibly future features
I'd like to see if I can add some additional helpful server tools if there's reasonable demand

 - performing backups (most likely will happen 
 - possibly a little FTP logic so users can easily copy the backups from wherever the server is running to their home computers.
 - possibly consider wrapping the tool in a GUI 

# Running the tool

Currently this tool only handles 1 server, I'll consider extending support to mutliple servers if there is demand.

`sst_config.json` is the only thing that needs to be filled out,

## Windows only

As this is being written on the .NET Framework, it is only available to Windows for the time being. I will look at porting this to .NET (Formerly .NET Core) to allow for cross platform usage.