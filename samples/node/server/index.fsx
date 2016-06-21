(**
 - title: Static HTTP server
 - tagline: Minimal demo using node.js to host static files

This script is a minimal example showing how to use Fable to create a node.js HTTP server that
hosts static files from the current directory using the `serve-static` package.
You can view the [source code](https://github.com/fsprojects/Fable/blob/master/samples/node/server/index.fsx),
[package.json](https://github.com/fsprojects/Fable/blob/master/samples/node/server/package.json) and
[fableconfig.json](https://github.com/fsprojects/Fable/blob/master/samples/node/server/fableconfig.json) on
GitHub. This page shows the full source code of the demo.

## Configuring Fable and packages

Aside from the F# source code, the [directory with the 
sample](https://github.com/fsprojects/Fable/blob/master/samples/node/server/) also contains
`package.json` and `fableconfig.json` files that configure node.js dependencies and specify
parameters for the Fable compiler. In this demo, we're using the [finalhandler package](https://github.com/pillarjs/finalhandler)
and [servestatic package](https://github.com/expressjs/serve-static) to serve static files via HTTP.

We also need `fable-core`, which contains F# mappings for core JavaScript functionality:

    [lang=js]
    {
      "private": true,
      "dependencies": {
        "fable-core": "^0.0.8",
        "finalhandler": "^0.4.1",
        "serve-static": "^1.10.2"
      },
      "devDependencies": {},
      "engines": {
        "fable": ">=0.2.0"
      }
    }

When you compile the project by running `fable`, it reads the following `fableconfig.json` file.
This instructs Fable to process the `index.fsx` file (alternatively, you can specify an F# project
file using `.fsproj`):

    {
        "module": "commonjs",
        "sourceMaps": true,
        "projFile": "./index.fsx",
        "outDir": "out",
        "scripts": {
            "prebuild": "npm install"
        }
    }

## Referencing Fable and dependencies

This sample accesses a couple of JavaScript libraries. To do that, we need to load the
Fable core library which provides F# functions and operators for calling JavaScript.
For more information, see the [Interacting with JavaScript](/docs/interacting.html) page.

The following loads the `Fable.Core.dll` from `node_modules` and imports the
two node.js dependencies:
*)
#r "node_modules/fable-core/Fable.Core.dll"
open System
open Fable.Core
open Fable.Import
open Fable.Import.Node

let finalhandler = require.Invoke("finalhandler")
let serveStatic = require.Invoke("serve-static")
(**
The `Fable.Import.Node` namespace contains mappings for global node.js objects. For example,
we can access the `argv` parameters of the application and get the default port (note that
`process` is a reserved keyword in F#, so we need to escape it using back-ticks):
*)
let port =
    match ``process``.argv with
    | args when args.Count >= 3 -> int args.[2]
    | _ -> 8080
(**
In JavaScript, you would create static server by calling `serveStatic("./")`. In Fable, the 
imported `serverStatic` value is not typed as a function and so we cannot call it directly.
Fable provides `$` operator that lets you write untyped function invocations.

The following implements the server and listens on the specified `port`:
*)
let server =
    let serve = serveStatic $ "./"
    let server =
        // As this lambda has more than one argument, it must be
        // converted to delegate so it's called back correctly
        http.createServer(Func<_,_,_>(fun req res ->
            let isDone = finalhandler $ (req, res)
            serve $ (req, res, isDone)
            |> ignore))
    server.listen port
(**

To report that the server is running, we can use `System.Console.WriteLine` from the
.NET library, but Fable also provides support for F# `printfn` function:
*)
printfn "Server running at localhost:%i" port
