/* global __dirname */
/* global process */

var fs = require("fs");
var path = require("path");
var babel = require("babel-core");
var template = require("babel-template");
var spawn = require('child_process').spawn;

// Custom plugin to simulate macro expressions
var transformMacroExpressions = {
  visitor: {
    StringLiteral(path) {
      if (!path.node.macro)
          return;
  
      try {
        var buildArgs = {};
        var buildMacro = template(path.node.value);
        for (var i = 0; i < path.node.args.length; i++) {
            buildArgs["$" + i] = path.node.args[i];
        }
        path.replaceWithMultiple(buildMacro(buildArgs));
      }
      catch (err) {
          console.log("BABEL ERROR: Failed to parse macro: " + path.node.value);
          process.exit(1); 
      }
    }
  }
};

// In pattern matching targets, some variables may have the same name
// but we can safely remove the duplicates after do expressions have
// been resolved and the variable declarations hoisted
var removeDuplicatedVarDeclarators = {
  visitor: {
    VariableDeclaration(path) {
      var buffer = [];
      var duplicated = [];
      
      for (var i = 0; i < path.node.declarations.length; i++) {
          var decl = path.node.declarations[i];
          if (typeof decl.id.name === "string") {
              if (buffer.indexOf(decl.id.name) > -1 && decl.init == null) {
                  duplicated.push(i);
              }
              else {
                  buffer.push(decl.id.name);
              }
          }
      }
  
      try {
        if (duplicated.length > 0) {
            var node = path.node;
            for (var j = duplicated.length - 1; j >= 0; j--) {
                node.declarations.splice(duplicated[j], 1);
            }
            path.replaceWith(node);
        }
      }
      catch (err) {
          console.log("BABEL ERROR: Failed to remove duplicated variables");
          process.exit(1); 
      }
    }
  }
};

var babelPlugins = [
    transformMacroExpressions,
    // "transform-es2015-block-scoping", // This creates too many function wrappers
    "transform-do-expressions",
    removeDuplicatedVarDeclarators,
    "transform-es5-property-mutators",
    "transform-es2015-arrow-functions",
    "transform-es2015-classes",
    "transform-es2015-computed-properties",
    "transform-es2015-for-of",
    "transform-es2015-object-super",
    "transform-es2015-parameters",
    "transform-es2015-shorthand-properties",
    "transform-es2015-spread"
];

function babelify(babelAst) {
    var opts = { plugins: babelPlugins };
    var parsed = babel.transformFromAst(babelAst, null, opts);
    return parsed.code;
}

try {
    var opts = {
        lib: ".",
        outDir: ".",
        symbols: [],
        watch: false
    }

    for (var i=2; i < process.argv.length; i++) {
        var key = process.argv[i].substring(2);
        opts[key] = key == "watch" ? true : opts[key] = process.argv[++i];
    }
    
    var fableCwd = process.cwd();
    var fableCmd = process.platform === "win32" ? "cmd" : "mono";
    var fableCmdArgs = [__dirname + "/fable/Fable.exe"];
	if ( process.platform === "win32") {
	  fableCmdArgs.unshift("/C");
	}

    if (typeof opts.code !== "string") {
        throw "No correct --code argument provided";
    }
    
    fableCmdArgs.push(JSON.stringify(opts));
    console.log(fableCmd + " " + fableCmdArgs.join(" "));
    
    var proc = spawn(fableCmd, fableCmdArgs, { cwd: fableCwd });
    proc.stdin.setEncoding('utf-8');

    // HTTP server
    var response = null;
    var resHeaders = {
        'Content-Type': 'text/plain',
        'Access-Control-Allow-Origin': '*'
    };
    var http = require('http');
    var server = http.createServer(function (req, res) {
        response = res;
        if (req.method == 'POST') {
            var body = '';
            req.on('data', function (data) {
                body += data.toString();
                // 1e6 === 1 * Math.pow(10, 6) === 1 * 1000000 ~~~ 1MB
                if (body.length > 1e6) {
                    // FLOOD ATTACK OR FAULTY CLIENT, NUKE REQUEST
                    body = "";
                    res.writeHead(413, resHeaders).end();
                    req.connection.destroy();
                }
            });
            req.on('end', function () {
                proc.stdin.write(body.replace(/\t/g, "    ").replace(/\n/g, "\\n") + "\n");
            });
        }
        else {
            res.writeHead(405, resHeaders);
            res.end();
        }
    });
    server.listen(5000);

    proc.on('exit', function(code) {
        console.log("Finished with code " + code);
        process.exit(code);
    });    

    proc.stderr.on('data', function(data) {
        var err = data.toString();
        console.log(err);
        if (response != null && !response.finished) {
            response.writeHead(200, "OK", resHeaders);
            response.end(err);
        }
    });    

    var buffer = "";
    proc.stdout.on("data", function(data) {
        var txt = data.toString();
        var json, closing = txt.indexOf("\n");
        if (closing == -1) {
            buffer += txt;
            return;
        }
        else {
            json = buffer + txt.substring(0, closing + 1);
            buffer = txt.substring(closing + 1);
        }
        
        var res;
        try {
            var babelAst = JSON.parse(json);
            res = babelAst.type == "Error"
                ? babelAst.message
                : babelify(babelAst);
        }
        catch (err) {
            console.log(res = err);
        }
        if (response != null && !response.finished) {
            response.writeHead(200, "OK", resHeaders);
            response.end(res);
        }
    });    
}
catch (err) {
    console.log("ARG ERROR: " + err);
    process.exit(1);
}
