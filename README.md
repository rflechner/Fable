# F# |> BABEL

### The compiler that emits JavaScript you can be proud of. [Try it out!](http://fsprojects.github.io/Fable/repl.html)
[![Mono Build Status](https://travis-ci.org/fsprojects/Fable.svg "Mono Build Status")](https://travis-ci.org/fsprojects/Fable) [![.NET Build Status](https://ci.appveyor.com/api/projects/status/vlmyxg64my74sik5?svg=true ".NET Build Status")](https://ci.appveyor.com/project/alfonsogarciacaro/fable)

Fable brings together the power of the [F# compiler](http://fsharp.github.io/FSharp.Compiler.Service/)
and [Babel](http://babeljs.io) to make JavaScript a true backend for F#.
Some of its main features are:

- Works directly on F# source code, no compilation to .NET bytecode needed
- Optimizes F# code to generate as clean JavaScript as possible
- Passes location data to Babel to generate source maps
- Compatible with all Babel plugins and other JS development tools, like Webpack
- [Support for most of the F# core library and a bit of .NET Base Class Library](docs/compatibility.md)
- Tiny core library included (less than 10KB minified and gzipped) with no runtime
- Organizes code using ES6 modules 
- [Interacts seamlessly with other JavaScript libraries](docs/interacting.md)
- Bonus: [compile NUnit tests to Mocha](docs/testing.md)

## Philosophy

- **Clean code**: The compiler must emit JavaScript as readable as possible
  which plays well with other JS code and development tools, even if this
  means sacrificing a bit F# semantics. 
- **Unopinionated**: The project shouldn't compromise with a specific platform
  and adapt to standard JavaScript in any environment: browser or node.
- **Open to the community**: If the project is to survive, it needs the support of the community. For that, maintainers should compromise to keep the project documented and make it easy for people to contribute. A plugin system is also planned.

## Installing

The project is still in testing phase and hasn't been packaged yet. If you wan to try it out,
make sure you have [F# 4.0 with .NET/Mono](http://fsharp.org) and an updated version of [node.js](https://nodejs.org) installed.
Download the repo and run:
```
build.cmd   // on windows    
./build.sh  // on unix
```
If everything works, follow [these instructions](docs/compiling.md) to compile a F# project or script file to JS.

## Contributing

At the moment, the best way to contribute is to battle-test the project, report issues,
create samples and help promoting it. A [plugin system](docs/plugins.md) is also available
to allow you extend Fable according to you needs.

Issues with the label `discussion` will be also added to ask the opinion of the community
on different topics like the logo, roadmap, etc.

## Caveats

- **Options are erased** in compiled code. This has several benefits like removing overhead
  and interacting with native JS functions in a safer way (`null` will be `None`).
  However, it will lead to unexpected results if you do weird things like wrapping `null` in `Some`.
  For practical purposes, Fable considers `null`, `undefined`, `None` and `unit` to be the same thing.

- **Information about generic types is not included** in the generated JavaScript, so code that
  depends on this information to be known at runtime for method dispatching may have unexpected behaviour.

- At the moment, comparison for non-primitive values defaults to JS reference comparison so there's
  **no structural comparison** for records, unions, tuples or collections. This may change in the future
  upon users' feedback.

- **Map and Set** default to the new ES6 Map and Set classes which must increase performance,
  but with the downside that adding and removing operations are **mutable**.

- **Debugging the F# code is not perfect**. For example, you may see the debugger jump directly
  to the last expression in many functions. This is normal and due to the optimizations performed
  on the generated code.

To know more, read [Compatibility](docs/compatibility.md).

## Acknowledgements

Of course, this project wouldn't have been possible without the fantastic work of the [F# compiler](http://fsharp.github.io/FSharp.Compiler.Service/)
and [Babel](http://babeljs.io) teams. I hope they feel proud seeing how their work has met in
a very unexpected way, giving developers even more possibilities to build great apps.

The awesome F# community has played a big role in making this possible. I've met incredible
people and it's impossible to list all the names without forgetting anyone, but I'd like to
give a particular mention to [Zach Bray](https://github.com/ZachBray) for his work on [FunScript](http://funscript.info/), [Don Syme](https://github.com/dsyme) (the fact that the designer
of the language himself shows interest in your work, no matter how humble it is, is really a big push!)
and [Krzysztof Cieślak](https://github.com/Krzysztof-Cieslak) (I always have to look up the name to spell it correctly) because he's shown that
F# is a perfect fit for a [big project targeting JS](http://ionide.io/).

And finally I'd like to thank my partner (is it too old-fashioned to say wife?) for bearing with me
everyday.
