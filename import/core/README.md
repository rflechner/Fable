# fable-import

Fable core lib & bindings for native JS objects, browser and node APIs

## Installation

```sh
$ npm install --save fable-core
```

## Usage

The default file when you import `fable-core` uses universal modules (UMD)
which can be understood by node (commonjs) or require.js (amd). There's also
a minified version `fable-core.min.js`, and if you need the library in other
module formats you can import `fable-core/es2015` or `fable-core/commonjs` instead.

### In a F# project (.fsproj)

```xml
  <ItemGroup>
    <Reference Include="node_modules/fable-core/Fable.Core.dll" />
  </ItemGroup>
```

### In a F# script (.fsx)

```fsharp
#r "node_modules/fable-core/Fable.Core.dll"

open Fable.Core
open Fable.Import
```

### require.js

When targeting the browser and using AMD instead of a module bundler,
you can load Fable's core lib with [require.js](http://requirejs.org) as follows:

```html
<script src="node_modules/requirejs/require.js"></script>
<script>
requirejs.config({
    // Set the baseUrl to the path of the compiled JS code
    baseUrl: 'out',
    paths: {
        // Explicit path to core lib (relative to baseUrl, omit .js)
        'fable-core': '../node_modules/fable-core/fable-core.min'
    }
});
// Load the entry file of the app (use array, omit .js)
requirejs(["app"]);
</script>
```
