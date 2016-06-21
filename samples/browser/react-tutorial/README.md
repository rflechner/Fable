# Fable React example

This is just an adaptation of [React tutorial]() and [react-hot-boilerplate](https://github.com/gaearon/react-hot-boilerplate)
to show how the workflow of building a React app with Fable would be.

> Remember to run `npm install` before the first compilation!

## Server

An [express](http://expressjs.com) server is available to interact with the app. When debugging,
a webpack dev server will also be launched to rebuild the app bundle on
changes and allow HMR (see below). Check `server.fs`.

## Client

The same F# project includes the code for client-side making it easier to
share code like business models (see `public/models.fs`). The project includes
a ReactHelper API with a DSL to make it easier to build React components from F#.
Check `public/components.fs`.

> The React DSL (`Fable.ReactHelper.fs`) is still subject to changes. 
After stabilization, it will be distributed through npm.

## Hot Module Reloading
The app uses [webpack](https://webpack.github.io) and a variation of [react-hot-loader](https://www.npmjs.com/package/react-hot-loader)
to allow HMR and improve the debug process. The necessary configuration is already defined
in `fableconfig.json` and `webpack.config.js`, so you just need to build the
project with the `debug` target (`fable -d` for short) and wait a few seconds until
the server launches. Visit `http://localhost:8080/` and then try modifying the `render`
function of one of the components in `public/components.fs`. See how the page loads
new code **without refreshing** on file saving. It's magic!

> Give also a try to [React Developer Tools](https://github.com/facebook/react-devtools)!