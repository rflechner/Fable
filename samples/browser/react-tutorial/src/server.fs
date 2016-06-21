module Server

open System
open Models
open Fable.Core
open Fable.Import

module Util =
    let handleError onErr onSuccess err =
        match unbox err with
        | Some err -> onErr err |> ignore
        | None -> onSuccess() |> ignore
        
    let failExit err =
        Console.WriteLine (string err)
        Node.``process``.exit 1.

open Util

let port =
    match unbox Node.``process``.env?PORT with
    | Some x -> x | None -> 8080

// When debuggin, use webpack-dev-server to allow Hot Module Reloading
#if DEV_SERVER
Node.``process``.env?WEBPACK_DEV_SERVER <- true
let config = Node.require.Invoke("../webpack.config")
let webpack = Node.require.Invoke("webpack")
let WebpackDevServer = Node.require.Invoke("webpack-dev-server")

// If webpack-dev-server is active, deviate the app server to port 3000
// webpack-dev-server will act as a proxy for /api/* calls (see webpack.config.js file)
let appPort = 3000

let webpackDevServer = createNew WebpackDevServer (webpack $ config, config?devServer)    
webpackDevServer?listen $ (port, "localhost", Func<_,_>(fun err res ->
    err |> handleError failExit (fun () ->
        printfn "Webpack server started: http://localhost:%i/" port)
)) |> ignore
#else
let appPort = port
#endif

// App server
let app = express.Invoke()
let COMMENTS_FILE = Node.path.join(Node.``process``.cwd(), "comments.json")

// Just use dynamic programming to set body-parser middleware
let bodyParser = Node.require.Invoke("body-parser")

// As app.use expects a ParamArray, the explicit unbox is necessary
// so the compiler doesn't think it's an array
bodyParser?json $ ()
|> unbox<express.RequestHandler> |> app.``use``
|> ignore

bodyParser?urlencoded $ (createObj ["extended" ==> true])
|> unbox<express.RequestHandler> |> app.``use``
|> ignore

// Serve static files from public folder
Node.path.join(Node.``process``.cwd(), "public")
|> express.``static``.Invoke
|> fun sta -> app.``use``("/", sta)
|> ignore

// Additional middleware which will set headers that we need on each request.
app.``use``(fun (req: express.Request) (res: express.Response) (next: obj->unit) ->
    // Set permissive CORS header - this allows this server to be used only as
    // an API server in conjunction with something like webpack-dev-server.
    res.setHeader("Access-Control-Allow-Origin", "*")
    // Disable caching so we'll always get the latest comments.
    res.setHeader("Cache-Control", "no-cache")
    box <| next())
|> ignore

app.get(U2.Case1 "/api/comments/", fun req res _ ->
    box <| Node.fs.readFile(COMMENTS_FILE, fun err data ->
        err |> handleError failExit (fun () ->
            string data |> JS.JSON.parse |> ($) res?json)))
|> ignore

app.post(U2.Case1 "/api/comments/", fun req res _ ->
    box <| Node.fs.readFile(COMMENTS_FILE, fun err data ->
        err |> handleError failExit (fun () ->
            let comments: ResizeArray<_> =
                string data |> JS.JSON.parse |> unbox
            // NOTE: In a real implementation, we would likely rely on a database or
            // some other approach (e.g. UUIDs) to ensure a globally unique id. We'll
            // treat Date.now() as unique-enough for our purposes.
            comments.Add {
                id = Some System.DateTime.Now
                author = string req?body?author
                text = string req?body?text
            }
            Node.fs.writeFile(
                COMMENTS_FILE,
                // The F# compiler gets confused about which overload of JSON.stringify
                // to use here so just apply the method dynamically
                JS.JSON.stringify $ (comments, None, 4.),
                fun err -> err |> handleError failExit (fun () -> res?json $ comments)))))
|> ignore

app.listen(appPort, unbox (fun () ->
    printfn "API server started: http://localhost:%i/" appPort))
|> ignore
