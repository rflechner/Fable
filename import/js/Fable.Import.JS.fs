namespace Fable.Import.JS
open System
open Fable.Core

type PropertyDescriptor =
    abstract configurable: bool option with get, set
    abstract enumerable: bool option with get, set
    abstract value: obj option with get, set
    abstract writable: bool option with get, set
    abstract get: unit -> obj
    abstract set: v: obj -> unit

and PropertyDescriptorMap =
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: s: string -> PropertyDescriptor with get, set

and Object =
    abstract ``constructor``: Function with get, set
    abstract toString: unit -> string
    abstract toLocaleString: unit -> string
    abstract valueOf: unit -> obj
    abstract hasOwnProperty: v: string -> bool
    abstract isPrototypeOf: v: obj -> bool
    abstract propertyIsEnumerable: v: string -> bool
    abstract hasOwnProperty: v: PropertyKey -> bool
    abstract propertyIsEnumerable: v: PropertyKey -> bool

and ObjectConstructor =
    abstract prototype: obj with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?value: obj -> obj
    [<Emit("$0($1...)")>] abstract callSelf: unit -> obj
    [<Emit("$0($1...)")>] abstract callSelf: value: obj -> obj
    abstract getPrototypeOf: o: obj -> obj
    abstract getOwnPropertyDescriptor: o: obj * p: string -> PropertyDescriptor
    abstract getOwnPropertyNames: o: obj -> ResizeArray<string>
    abstract create: o: obj * ?properties: PropertyDescriptorMap -> obj
    abstract defineProperty: o: obj * p: string * attributes: PropertyDescriptor -> obj
    abstract defineProperties: o: obj * properties: PropertyDescriptorMap -> obj
    abstract seal: o: 'T -> 'T
    abstract freeze: o: 'T -> 'T
    abstract preventExtensions: o: 'T -> 'T
    abstract isSealed: o: obj -> bool
    abstract isFrozen: o: obj -> bool
    abstract isExtensible: o: obj -> bool
    abstract keys: o: obj -> ResizeArray<string>
    abstract assign: target: 'T * source: 'U -> obj
    abstract assign: target: 'T * source1: 'U * source2: 'V -> obj
    abstract assign: target: 'T * source1: 'U * source2: 'V * source3: 'W -> obj
    abstract assign: target: obj * [<ParamArray>] sources: obj[] -> obj
    abstract getOwnPropertySymbols: o: obj -> ResizeArray<Symbol>
    abstract is: value1: obj * value2: obj -> bool
    abstract setPrototypeOf: o: obj * proto: obj -> obj
    abstract getOwnPropertyDescriptor: o: obj * propertyKey: PropertyKey -> PropertyDescriptor
    abstract defineProperty: o: obj * propertyKey: PropertyKey * attributes: PropertyDescriptor -> obj

and Function =
    abstract prototype: obj with get, set
    abstract length: float with get, set
    abstract arguments: obj with get, set
    abstract caller: Function with get, set
    abstract name: string with get, set
    abstract apply: thisArg: obj * ?argArray: obj -> obj
    abstract call: thisArg: obj * [<ParamArray>] argArray: obj[] -> obj
    abstract bind: thisArg: obj * [<ParamArray>] argArray: obj[] -> obj
    [<Emit("$0[Symbol.hasInstance]($1...)")>] abstract ``[Symbol.hasInstance]``: value: obj -> bool

and FunctionConstructor =
    abstract prototype: Function with get, set
    [<Emit("new $0($1...)")>] abstract createNew: [<ParamArray>] args: string[] -> Function
    [<Emit("$0($1...)")>] abstract callSelf: [<ParamArray>] args: string[] -> Function

and IArguments =
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> obj with get, set
    abstract length: float with get, set
    abstract callee: Function with get, set
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<obj>

and String =
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> string with get, set
    abstract toString: unit -> string
    abstract charAt: pos: float -> string
    abstract charCodeAt: index: float -> float
    abstract concat: [<ParamArray>] strings: string[] -> string
    abstract indexOf: searchString: string * ?position: float -> float
    abstract lastIndexOf: searchString: string * ?position: float -> float
    abstract localeCompare: that: string -> float
    abstract ``match``: regexp: string -> RegExpMatchArray
    abstract ``match``: regexp: RegExp -> RegExpMatchArray
    abstract replace: searchValue: string * replaceValue: string -> string
    abstract replace: searchValue: string * replacer: Func<string, obj, string> -> string
    abstract replace: searchValue: RegExp * replaceValue: string -> string
    abstract replace: searchValue: RegExp * replacer: Func<string, obj, string> -> string
    abstract search: regexp: string -> float
    abstract search: regexp: RegExp -> float
    abstract slice: ?start: float * ?``end``: float -> string
    abstract split: separator: string * ?limit: float -> ResizeArray<string>
    abstract split: separator: RegExp * ?limit: float -> ResizeArray<string>
    abstract substring: start: float * ?``end``: float -> string
    abstract toLowerCase: unit -> string
    abstract toLocaleLowerCase: unit -> string
    abstract toUpperCase: unit -> string
    abstract toLocaleUpperCase: unit -> string
    abstract trim: unit -> string
    abstract substr: from: float * ?length: float -> string
    abstract valueOf: unit -> string
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<string>
    abstract codePointAt: pos: float -> float
    abstract includes: searchString: string * ?position: float -> bool
    abstract endsWith: searchString: string * ?endPosition: float -> bool
    abstract normalize: ?form: string -> string
    abstract repeat: count: float -> string
    abstract startsWith: searchString: string * ?position: float -> bool
    abstract ``match``: matcher: obj -> RegExpMatchArray
    abstract replace: searchValue: obj * replaceValue: string -> string
    abstract replace: searchValue: obj * replacer: Func<string, obj, string> -> string
    abstract search: searcher: obj -> float
    abstract split: splitter: obj * ?limit: float -> ResizeArray<string>
    abstract anchor: name: string -> string
    abstract big: unit -> string
    abstract blink: unit -> string
    abstract bold: unit -> string
    abstract ``fixed``: unit -> string
    abstract fontcolor: color: string -> string
    abstract fontsize: size: float -> string
    abstract fontsize: size: string -> string
    abstract italics: unit -> string
    abstract link: url: string -> string
    abstract small: unit -> string
    abstract strike: unit -> string
    abstract sub: unit -> string
    abstract sup: unit -> string

and StringConstructor =
    abstract prototype: String with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?value: obj -> String
    [<Emit("$0($1...)")>] abstract callSelf: ?value: obj -> string
    abstract fromCharCode: [<ParamArray>] codes: float[] -> string
    abstract fromCodePoint: [<ParamArray>] codePoints: float[] -> string
    abstract raw: template: TemplateStringsArray * [<ParamArray>] substitutions: obj[] -> string

and Boolean =
    abstract valueOf: unit -> bool

and BooleanConstructor =
    abstract prototype: Boolean with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?value: obj -> Boolean
    [<Emit("$0($1...)")>] abstract callSelf: ?value: obj -> bool

and Number =
    abstract toString: ?radix: float -> string
    abstract toFixed: ?fractionDigits: float -> string
    abstract toExponential: ?fractionDigits: float -> string
    abstract toPrecision: ?precision: float -> string
    abstract valueOf: unit -> float

and NumberConstructor =
    abstract prototype: Number with get, set
    abstract MAX_VALUE: float with get, set
    abstract MIN_VALUE: float with get, set
    abstract NaN: float with get, set
    abstract NEGATIVE_INFINITY: float with get, set
    abstract POSITIVE_INFINITY: float with get, set
    abstract EPSILON: float with get, set
    abstract MAX_SAFE_INTEGER: float with get, set
    abstract MIN_SAFE_INTEGER: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?value: obj -> Number
    [<Emit("$0($1...)")>] abstract callSelf: ?value: obj -> float
    abstract isFinite: number: float -> bool
    abstract isInteger: number: float -> bool
    abstract isNaN: number: float -> bool
    abstract isSafeInteger: number: float -> bool
    abstract parseFloat: string: string -> float
    abstract parseInt: string: string * ?radix: float -> float

and TemplateStringsArray =
    inherit Array<string>
    abstract raw: ResizeArray<string> with get, set

and Math =
    abstract E: float with get, set
    abstract LN10: float with get, set
    abstract LN2: float with get, set
    abstract LOG2E: float with get, set
    abstract LOG10E: float with get, set
    abstract PI: float with get, set
    abstract SQRT1_2: float with get, set
    abstract SQRT2: float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract abs: x: float -> float
    abstract acos: x: float -> float
    abstract asin: x: float -> float
    abstract atan: x: float -> float
    abstract atan2: y: float * x: float -> float
    abstract ceil: x: float -> float
    abstract cos: x: float -> float
    abstract exp: x: float -> float
    abstract floor: x: float -> float
    abstract log: x: float -> float
    abstract max: [<ParamArray>] values: float[] -> float
    abstract min: [<ParamArray>] values: float[] -> float
    abstract pow: x: float * y: float -> float
    abstract random: unit -> float
    abstract round: x: float -> float
    abstract sin: x: float -> float
    abstract sqrt: x: float -> float
    abstract tan: x: float -> float
    abstract clz32: x: float -> float
    abstract imul: x: float * y: float -> float
    abstract sign: x: float -> float
    abstract log10: x: float -> float
    abstract log2: x: float -> float
    abstract log1p: x: float -> float
    abstract expm1: x: float -> float
    abstract cosh: x: float -> float
    abstract sinh: x: float -> float
    abstract tanh: x: float -> float
    abstract acosh: x: float -> float
    abstract asinh: x: float -> float
    abstract atanh: x: float -> float
    abstract hypot: [<ParamArray>] values: float[] -> float
    abstract trunc: x: float -> float
    abstract fround: x: float -> float
    abstract cbrt: x: float -> float

and Date =
    abstract toString: unit -> string
    abstract toDateString: unit -> string
    abstract toTimeString: unit -> string
    abstract toLocaleString: unit -> string
    abstract toLocaleDateString: unit -> string
    abstract toLocaleTimeString: unit -> string
    abstract valueOf: unit -> float
    abstract getTime: unit -> float
    abstract getFullYear: unit -> float
    abstract getUTCFullYear: unit -> float
    abstract getMonth: unit -> float
    abstract getUTCMonth: unit -> float
    abstract getDate: unit -> float
    abstract getUTCDate: unit -> float
    abstract getDay: unit -> float
    abstract getUTCDay: unit -> float
    abstract getHours: unit -> float
    abstract getUTCHours: unit -> float
    abstract getMinutes: unit -> float
    abstract getUTCMinutes: unit -> float
    abstract getSeconds: unit -> float
    abstract getUTCSeconds: unit -> float
    abstract getMilliseconds: unit -> float
    abstract getUTCMilliseconds: unit -> float
    abstract getTimezoneOffset: unit -> float
    abstract setTime: time: float -> float
    abstract setMilliseconds: ms: float -> float
    abstract setUTCMilliseconds: ms: float -> float
    abstract setSeconds: sec: float * ?ms: float -> float
    abstract setUTCSeconds: sec: float * ?ms: float -> float
    abstract setMinutes: min: float * ?sec: float * ?ms: float -> float
    abstract setUTCMinutes: min: float * ?sec: float * ?ms: float -> float
    abstract setHours: hours: float * ?min: float * ?sec: float * ?ms: float -> float
    abstract setUTCHours: hours: float * ?min: float * ?sec: float * ?ms: float -> float
    abstract setDate: date: float -> float
    abstract setUTCDate: date: float -> float
    abstract setMonth: month: float * ?date: float -> float
    abstract setUTCMonth: month: float * ?date: float -> float
    abstract setFullYear: year: float * ?month: float * ?date: float -> float
    abstract setUTCFullYear: year: float * ?month: float * ?date: float -> float
    abstract toUTCString: unit -> string
    abstract toISOString: unit -> string
    abstract toJSON: ?key: obj -> string
    [<Emit("$0[Symbol.toPrimitive]($1...)")>] abstract ``[Symbol.toPrimitive]_default``: unit -> string
    [<Emit("$0[Symbol.toPrimitive]($1...)")>] abstract ``[Symbol.toPrimitive]_string``: unit -> string
    [<Emit("$0[Symbol.toPrimitive]($1...)")>] abstract ``[Symbol.toPrimitive]_number``: unit -> float
    [<Emit("$0[Symbol.toPrimitive]($1...)")>] abstract ``[Symbol.toPrimitive]``: hint: string -> U2<string, float>

and DateConstructor =
    abstract prototype: DateTime with get, set
    [<Emit("new $0($1...)")>] abstract createNew: unit -> DateTime
    [<Emit("new $0($1...)")>] abstract createNew: value: float -> DateTime
    [<Emit("new $0($1...)")>] abstract createNew: value: string -> DateTime
    [<Emit("new $0($1...)")>] abstract createNew: year: float * month: float * ?date: float * ?hours: float * ?minutes: float * ?seconds: float * ?ms: float -> DateTime
    [<Emit("$0($1...)")>] abstract callSelf: unit -> string
    abstract parse: s: string -> float
    abstract UTC: year: float * month: float * ?date: float * ?hours: float * ?minutes: float * ?seconds: float * ?ms: float -> float
    abstract now: unit -> float

and RegExpMatchArray =
    inherit Array<string>
    abstract index: float option with get, set
    abstract input: string option with get, set

and RegExpExecArray =
    inherit Array<string>
    abstract index: float with get, set
    abstract input: string with get, set

and RegExp =
    abstract source: string with get, set
    abstract ``global``: bool with get, set
    abstract ignoreCase: bool with get, set
    abstract multiline: bool with get, set
    abstract lastIndex: float with get, set
    abstract flags: string with get, set
    abstract sticky: bool with get, set
    abstract unicode: bool with get, set
    abstract exec: string: string -> RegExpExecArray
    abstract test: string: string -> bool
    abstract compile: unit -> RegExp
    [<Emit("$0[Symbol.match]($1...)")>] abstract ``[Symbol.match]``: string: string -> RegExpMatchArray
    [<Emit("$0[Symbol.replace]($1...)")>] abstract ``[Symbol.replace]``: string: string * replaceValue: string -> string
    [<Emit("$0[Symbol.replace]($1...)")>] abstract ``[Symbol.replace]``: string: string * replacer: Func<string, obj, string> -> string
    [<Emit("$0[Symbol.search]($1...)")>] abstract ``[Symbol.search]``: string: string -> float
    [<Emit("$0[Symbol.split]($1...)")>] abstract ``[Symbol.split]``: string: string * ?limit: float -> ResizeArray<string>

and RegExpConstructor =
    abstract prototype: RegExp with get, set
    abstract ``$1``: string with get, set
    abstract ``$2``: string with get, set
    abstract ``$3``: string with get, set
    abstract ``$4``: string with get, set
    abstract ``$5``: string with get, set
    abstract ``$6``: string with get, set
    abstract ``$7``: string with get, set
    abstract ``$8``: string with get, set
    abstract ``$9``: string with get, set
    abstract lastMatch: string with get, set
    [<Emit("new $0($1...)")>] abstract createNew: pattern: string * ?flags: string -> RegExp
    [<Emit("$0($1...)")>] abstract callSelf: pattern: string * ?flags: string -> RegExp
    [<Emit("$0[Symbol.species]($1...)")>] abstract ``[Symbol.species]``: unit -> RegExpConstructor

and Error =
    abstract name: string with get, set
    abstract message: string with get, set

and ErrorConstructor =
    abstract prototype: Error with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> Error
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> Error

and EvalError =
    inherit Error


and EvalErrorConstructor =
    abstract prototype: EvalError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> EvalError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> EvalError

and RangeError =
    inherit Error


and RangeErrorConstructor =
    abstract prototype: RangeError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> RangeError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> RangeError

and ReferenceError =
    inherit Error


and ReferenceErrorConstructor =
    abstract prototype: ReferenceError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> ReferenceError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> ReferenceError

and SyntaxError =
    inherit Error


and SyntaxErrorConstructor =
    abstract prototype: SyntaxError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> SyntaxError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> SyntaxError

and TypeError =
    inherit Error


and TypeErrorConstructor =
    abstract prototype: TypeError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> TypeError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> TypeError

and URIError =
    inherit Error


and URIErrorConstructor =
    abstract prototype: URIError with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?message: string -> URIError
    [<Emit("$0($1...)")>] abstract callSelf: ?message: string -> URIError

and JSON =
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract parse: text: string * ?reviver: Func<obj, obj, obj> -> obj
    abstract stringify: value: obj -> string
    abstract stringify: value: obj * replacer: Func<string, obj, obj> -> string
    abstract stringify: value: obj * replacer: ResizeArray<obj> -> string
    abstract stringify: value: obj * replacer: Func<string, obj, obj> * space: U2<string, float> -> string
    abstract stringify: value: obj * replacer: ResizeArray<obj> * space: U2<string, float> -> string

and ReadonlyArray<'T> =
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: n: float -> 'T with get, set
    abstract toString: unit -> string
    abstract toLocaleString: unit -> string
    abstract concat: [<ParamArray>] items: 'U[] -> ResizeArray<'T>
    abstract concat: [<ParamArray>] items: 'T[] -> ResizeArray<'T>
    abstract join: ?separator: string -> string
    abstract slice: ?start: float * ?``end``: float -> ResizeArray<'T>
    abstract indexOf: searchElement: 'T * ?fromIndex: float -> float
    abstract lastIndexOf: searchElement: 'T * ?fromIndex: float -> float
    abstract every: callbackfn: Func<'T, float, ReadonlyArray<'T>, bool> * ?thisArg: obj -> bool
    abstract some: callbackfn: Func<'T, float, ReadonlyArray<'T>, bool> * ?thisArg: obj -> bool
    abstract forEach: callbackfn: Func<'T, float, ReadonlyArray<'T>, unit> * ?thisArg: obj -> unit
    abstract map: callbackfn: Func<'T, float, ReadonlyArray<'T>, 'U> * ?thisArg: obj -> ResizeArray<'U>
    abstract filter: callbackfn: Func<'T, float, ReadonlyArray<'T>, bool> * ?thisArg: obj -> ResizeArray<'T>
    abstract reduce: callbackfn: Func<'T, 'T, float, ReadonlyArray<'T>, 'T> * ?initialValue: 'T -> 'T
    abstract reduce: callbackfn: Func<'U, 'T, float, ReadonlyArray<'T>, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<'T, 'T, float, ReadonlyArray<'T>, 'T> * ?initialValue: 'T -> 'T
    abstract reduceRight: callbackfn: Func<'U, 'T, float, ReadonlyArray<'T>, 'U> * initialValue: 'U -> 'U

and Array<'T> =
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: n: float -> 'T with get, set
    abstract toString: unit -> string
    abstract toLocaleString: unit -> string
    abstract push: [<ParamArray>] items: 'T[] -> float
    abstract pop: unit -> 'T
    abstract concat: [<ParamArray>] items: U2<'T, ResizeArray<'T>>[] -> ResizeArray<'T>
    abstract join: ?separator: string -> string
    abstract reverse: unit -> ResizeArray<'T>
    abstract shift: unit -> 'T
    abstract slice: ?start: float * ?``end``: float -> ResizeArray<'T>
    abstract sort: ?compareFn: Func<'T, 'T, float> -> ResizeArray<'T>
    abstract splice: start: float -> ResizeArray<'T>
    abstract splice: start: float * deleteCount: float * [<ParamArray>] items: 'T[] -> ResizeArray<'T>
    abstract unshift: [<ParamArray>] items: 'T[] -> float
    abstract indexOf: searchElement: 'T * ?fromIndex: float -> float
    abstract lastIndexOf: searchElement: 'T * ?fromIndex: float -> float
    abstract every: callbackfn: Func<'T, float, ResizeArray<'T>, bool> * ?thisArg: obj -> bool
    abstract some: callbackfn: Func<'T, float, ResizeArray<'T>, bool> * ?thisArg: obj -> bool
    abstract forEach: callbackfn: Func<'T, float, ResizeArray<'T>, unit> * ?thisArg: obj -> unit
    abstract map: callbackfn: Func<'T, float, ResizeArray<'T>, 'U> * ?thisArg: obj -> ResizeArray<'U>
    abstract filter: callbackfn: Func<'T, float, ResizeArray<'T>, bool> * ?thisArg: obj -> ResizeArray<'T>
    abstract reduce: callbackfn: Func<'T, 'T, float, ResizeArray<'T>, 'T> * ?initialValue: 'T -> 'T
    abstract reduce: callbackfn: Func<'U, 'T, float, ResizeArray<'T>, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<'T, 'T, float, ResizeArray<'T>, 'T> * ?initialValue: 'T -> 'T
    abstract reduceRight: callbackfn: Func<'U, 'T, float, ResizeArray<'T>, 'U> * initialValue: 'U -> 'U
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<'T>
    [<Emit("$0[Symbol.unscopables]($1...)")>] abstract ``[Symbol.unscopables]``: unit -> obj
    abstract entries: unit -> IterableIterator<float * 'T>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<'T>
    abstract find: predicate: Func<'T, float, ResizeArray<'T>, bool> * ?thisArg: obj -> 'T
    abstract findIndex: predicate: Func<'T, bool> * ?thisArg: obj -> float
    abstract fill: value: 'T * ?start: float * ?``end``: float -> ResizeArray<'T>
    abstract copyWithin: target: float * start: float * ?``end``: float -> ResizeArray<'T>

and ArrayConstructor =
    abstract prototype: ResizeArray<obj> with get, set
    [<Emit("new $0($1...)")>] abstract createNew: ?arrayLength: float -> ResizeArray<obj>
    [<Emit("new $0($1...)")>] abstract createNew: arrayLength: float -> ResizeArray<'T>
    [<Emit("new $0($1...)")>] abstract createNew: [<ParamArray>] items: 'T[] -> ResizeArray<'T>
    [<Emit("$0($1...)")>] abstract callSelf: ?arrayLength: float -> ResizeArray<obj>
    [<Emit("$0($1...)")>] abstract callSelf: arrayLength: float -> ResizeArray<'T>
    [<Emit("$0($1...)")>] abstract callSelf: [<ParamArray>] items: 'T[] -> ResizeArray<'T>
    abstract isArray: arg: obj -> obj
    abstract from: arrayLike: ArrayLike<'T> * mapfn: Func<'T, float, 'U> * ?thisArg: obj -> ResizeArray<'U>
    abstract from: iterable: Iterable<'T> * mapfn: Func<'T, float, 'U> * ?thisArg: obj -> ResizeArray<'U>
    abstract from: arrayLike: ArrayLike<'T> -> ResizeArray<'T>
    abstract from: iterable: Iterable<'T> -> ResizeArray<'T>
    abstract ``of``: [<ParamArray>] items: 'T[] -> ResizeArray<'T>

and TypedPropertyDescriptor<'T> =
    abstract enumerable: bool option with get, set
    abstract configurable: bool option with get, set
    abstract writable: bool option with get, set
    abstract value: 'T option with get, set
    abstract get: Func<'T> option with get, set
    abstract set: Func<'T, unit> option with get, set

and ClassDecorator =
    Func<obj, U2<obj, unit>>

and PropertyDecorator =
    Func<obj, U2<string, Symbol>, unit>

and MethodDecorator =
    Func<obj, U2<string, Symbol>, TypedPropertyDescriptor<obj>, U2<TypedPropertyDescriptor<obj>, unit>>

and ParameterDecorator =
    Func<obj, U2<string, Symbol>, float, unit>

and PromiseConstructorLike =
    obj

and PromiseLike<'T> =
    abstract ``then``: ?onfulfilled: Func<'T, U2<'TResult, PromiseLike<'TResult>>> * ?onrejected: Func<obj, U2<'TResult, PromiseLike<'TResult>>> -> PromiseLike<'TResult>
    abstract ``then``: ?onfulfilled: Func<'T, U2<'TResult, PromiseLike<'TResult>>> * ?onrejected: Func<obj, unit> -> PromiseLike<'TResult>

and ArrayLike<'T> =
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: n: float -> 'T with get, set

and ArrayBuffer =
    abstract byteLength: float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract slice: ``begin``: float * ?``end``: float -> ArrayBuffer

and ArrayBufferConstructor =
    abstract prototype: ArrayBuffer with get, set
    [<Emit("new $0($1...)")>] abstract createNew: byteLength: float -> ArrayBuffer
    abstract isView: arg: obj -> obj

and ArrayBufferView =
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set

and DataView =
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract getFloat32: byteOffset: float * ?littleEndian: bool -> float
    abstract getFloat64: byteOffset: float * ?littleEndian: bool -> float
    abstract getInt8: byteOffset: float -> float
    abstract getInt16: byteOffset: float * ?littleEndian: bool -> float
    abstract getInt32: byteOffset: float * ?littleEndian: bool -> float
    abstract getUint8: byteOffset: float -> float
    abstract getUint16: byteOffset: float * ?littleEndian: bool -> float
    abstract getUint32: byteOffset: float * ?littleEndian: bool -> float
    abstract setFloat32: byteOffset: float * value: float * ?littleEndian: bool -> unit
    abstract setFloat64: byteOffset: float * value: float * ?littleEndian: bool -> unit
    abstract setInt8: byteOffset: float * value: float -> unit
    abstract setInt16: byteOffset: float * value: float * ?littleEndian: bool -> unit
    abstract setInt32: byteOffset: float * value: float * ?littleEndian: bool -> unit
    abstract setUint8: byteOffset: float * value: float -> unit
    abstract setUint16: byteOffset: float * value: float * ?littleEndian: bool -> unit
    abstract setUint32: byteOffset: float * value: float * ?littleEndian: bool -> unit

and DataViewConstructor =
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?byteLength: float -> DataView

and Int8Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Int8Array
    abstract every: callbackfn: Func<float, float, Int8Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Int8Array
    abstract filter: callbackfn: Func<float, float, Int8Array, bool> * ?thisArg: obj -> Int8Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Int8Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Int8Array, float> * ?thisArg: obj -> Int8Array
    abstract reduce: callbackfn: Func<float, float, float, Int8Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Int8Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Int8Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Int8Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Int8Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Int8Array
    abstract some: callbackfn: Func<float, float, Int8Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Int8Array
    abstract subarray: ``begin``: float * ?``end``: float -> Int8Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Int8ArrayConstructor =
    abstract prototype: Int8Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Int8Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Int8Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Int8Array
    abstract ``of``: [<ParamArray>] items: float[] -> Int8Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int8Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Int8Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int8Array

and Uint8Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Uint8Array
    abstract every: callbackfn: Func<float, float, Uint8Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Uint8Array
    abstract filter: callbackfn: Func<float, float, Uint8Array, bool> * ?thisArg: obj -> Uint8Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Uint8Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Uint8Array, float> * ?thisArg: obj -> Uint8Array
    abstract reduce: callbackfn: Func<float, float, float, Uint8Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Uint8Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Uint8Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Uint8Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Uint8Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Uint8Array
    abstract some: callbackfn: Func<float, float, Uint8Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Uint8Array
    abstract subarray: ``begin``: float * ?``end``: float -> Uint8Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Uint8ArrayConstructor =
    abstract prototype: Uint8Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Uint8Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Uint8Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Uint8Array
    abstract ``of``: [<ParamArray>] items: float[] -> Uint8Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint8Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Uint8Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint8Array

and Uint8ClampedArray =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Uint8ClampedArray
    abstract every: callbackfn: Func<float, float, Uint8ClampedArray, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Uint8ClampedArray
    abstract filter: callbackfn: Func<float, float, Uint8ClampedArray, bool> * ?thisArg: obj -> Uint8ClampedArray
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Uint8ClampedArray, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Uint8ClampedArray, float> * ?thisArg: obj -> Uint8ClampedArray
    abstract reduce: callbackfn: Func<float, float, float, Uint8ClampedArray, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Uint8ClampedArray, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Uint8ClampedArray, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Uint8ClampedArray, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Uint8ClampedArray
    abstract set: index: float * value: float -> unit
    abstract set: array: Uint8ClampedArray * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Uint8ClampedArray
    abstract some: callbackfn: Func<float, float, Uint8ClampedArray, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Uint8ClampedArray
    abstract subarray: ``begin``: float * ?``end``: float -> Uint8ClampedArray
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Uint8ClampedArrayConstructor =
    abstract prototype: Uint8ClampedArray with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Uint8ClampedArray
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Uint8ClampedArray
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Uint8ClampedArray
    abstract ``of``: [<ParamArray>] items: float[] -> Uint8ClampedArray
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint8ClampedArray
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Uint8ClampedArray
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint8ClampedArray

and Int16Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Int16Array
    abstract every: callbackfn: Func<float, float, Int16Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Int16Array
    abstract filter: callbackfn: Func<float, float, Int16Array, bool> * ?thisArg: obj -> Int16Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Int16Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Int16Array, float> * ?thisArg: obj -> Int16Array
    abstract reduce: callbackfn: Func<float, float, float, Int16Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Int16Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Int16Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Int16Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Int16Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Int16Array
    abstract some: callbackfn: Func<float, float, Int16Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Int16Array
    abstract subarray: ``begin``: float * ?``end``: float -> Int16Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Int16ArrayConstructor =
    abstract prototype: Int16Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Int16Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Int16Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Int16Array
    abstract ``of``: [<ParamArray>] items: float[] -> Int16Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int16Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Int16Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int16Array

and Uint16Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Uint16Array
    abstract every: callbackfn: Func<float, float, Uint16Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Uint16Array
    abstract filter: callbackfn: Func<float, float, Uint16Array, bool> * ?thisArg: obj -> Uint16Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Uint16Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Uint16Array, float> * ?thisArg: obj -> Uint16Array
    abstract reduce: callbackfn: Func<float, float, float, Uint16Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Uint16Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Uint16Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Uint16Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Uint16Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Uint16Array
    abstract some: callbackfn: Func<float, float, Uint16Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Uint16Array
    abstract subarray: ``begin``: float * ?``end``: float -> Uint16Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Uint16ArrayConstructor =
    abstract prototype: Uint16Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Uint16Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Uint16Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Uint16Array
    abstract ``of``: [<ParamArray>] items: float[] -> Uint16Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint16Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Uint16Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint16Array

and Int32Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Int32Array
    abstract every: callbackfn: Func<float, float, Int32Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Int32Array
    abstract filter: callbackfn: Func<float, float, Int32Array, bool> * ?thisArg: obj -> Int32Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Int32Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Int32Array, float> * ?thisArg: obj -> Int32Array
    abstract reduce: callbackfn: Func<float, float, float, Int32Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Int32Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Int32Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Int32Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Int32Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Int32Array
    abstract some: callbackfn: Func<float, float, Int32Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Int32Array
    abstract subarray: ``begin``: float * ?``end``: float -> Int32Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Int32ArrayConstructor =
    abstract prototype: Int32Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Int32Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Int32Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Int32Array
    abstract ``of``: [<ParamArray>] items: float[] -> Int32Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int32Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Int32Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Int32Array

and Uint32Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Uint32Array
    abstract every: callbackfn: Func<float, float, Uint32Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Uint32Array
    abstract filter: callbackfn: Func<float, float, Uint32Array, bool> * ?thisArg: obj -> Uint32Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Uint32Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Uint32Array, float> * ?thisArg: obj -> Uint32Array
    abstract reduce: callbackfn: Func<float, float, float, Uint32Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Uint32Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Uint32Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Uint32Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Uint32Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Uint32Array
    abstract some: callbackfn: Func<float, float, Uint32Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Uint32Array
    abstract subarray: ``begin``: float * ?``end``: float -> Uint32Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Uint32ArrayConstructor =
    abstract prototype: Uint32Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Uint32Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Uint32Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Uint32Array
    abstract ``of``: [<ParamArray>] items: float[] -> Uint32Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint32Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Uint32Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Uint32Array

and Float32Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Float32Array
    abstract every: callbackfn: Func<float, float, Float32Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Float32Array
    abstract filter: callbackfn: Func<float, float, Float32Array, bool> * ?thisArg: obj -> Float32Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Float32Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Float32Array, float> * ?thisArg: obj -> Float32Array
    abstract reduce: callbackfn: Func<float, float, float, Float32Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Float32Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Float32Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Float32Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Float32Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Float32Array
    abstract some: callbackfn: Func<float, float, Float32Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Float32Array
    abstract subarray: ``begin``: float * ?``end``: float -> Float32Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Float32ArrayConstructor =
    abstract prototype: Float32Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Float32Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Float32Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Float32Array
    abstract ``of``: [<ParamArray>] items: float[] -> Float32Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Float32Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Float32Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Float32Array

and Float64Array =
    abstract BYTES_PER_ELEMENT: float with get, set
    abstract buffer: ArrayBuffer with get, set
    abstract byteLength: float with get, set
    abstract byteOffset: float with get, set
    abstract length: float with get, set
    [<Emit("$0[$1]{{=$2}}")>] abstract Item: index: float -> float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract copyWithin: target: float * start: float * ?``end``: float -> Float64Array
    abstract every: callbackfn: Func<float, float, Float64Array, bool> * ?thisArg: obj -> bool
    abstract fill: value: float * ?start: float * ?``end``: float -> Float64Array
    abstract filter: callbackfn: Func<float, float, Float64Array, bool> * ?thisArg: obj -> Float64Array
    abstract find: predicate: Func<float, float, ResizeArray<float>, bool> * ?thisArg: obj -> float
    abstract findIndex: predicate: Func<float, bool> * ?thisArg: obj -> float
    abstract forEach: callbackfn: Func<float, float, Float64Array, unit> * ?thisArg: obj -> unit
    abstract indexOf: searchElement: float * ?fromIndex: float -> float
    abstract join: ?separator: string -> string
    abstract lastIndexOf: searchElement: float * ?fromIndex: float -> float
    abstract map: callbackfn: Func<float, float, Float64Array, float> * ?thisArg: obj -> Float64Array
    abstract reduce: callbackfn: Func<float, float, float, Float64Array, float> * ?initialValue: float -> float
    abstract reduce: callbackfn: Func<'U, float, float, Float64Array, 'U> * initialValue: 'U -> 'U
    abstract reduceRight: callbackfn: Func<float, float, float, Float64Array, float> * ?initialValue: float -> float
    abstract reduceRight: callbackfn: Func<'U, float, float, Float64Array, 'U> * initialValue: 'U -> 'U
    abstract reverse: unit -> Float64Array
    abstract set: index: float * value: float -> unit
    abstract set: array: ArrayLike<float> * ?offset: float -> unit
    abstract slice: ?start: float * ?``end``: float -> Float64Array
    abstract some: callbackfn: Func<float, float, Float64Array, bool> * ?thisArg: obj -> bool
    abstract sort: ?compareFn: Func<float, float, float> -> Float64Array
    abstract subarray: ``begin``: float * ?``end``: float -> Float64Array
    abstract toLocaleString: unit -> string
    abstract toString: unit -> string
    abstract entries: unit -> IterableIterator<float * float>
    abstract keys: unit -> IterableIterator<float>
    abstract values: unit -> IterableIterator<float>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<float>

and Float64ArrayConstructor =
    abstract prototype: Float64Array with get, set
    abstract BYTES_PER_ELEMENT: float with get, set
    [<Emit("new $0($1...)")>] abstract createNew: length: float -> Float64Array
    [<Emit("new $0($1...)")>] abstract createNew: array: ArrayLike<float> -> Float64Array
    [<Emit("new $0($1...)")>] abstract createNew: buffer: ArrayBuffer * ?byteOffset: float * ?length: float -> Float64Array
    abstract ``of``: [<ParamArray>] items: float[] -> Float64Array
    abstract from: arrayLike: ArrayLike<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Float64Array
    [<Emit("new $0($1...)")>] abstract createNew: elements: Iterable<float> -> Float64Array
    abstract from: arrayLike: Iterable<float> * ?mapfn: Func<float, float, float> * ?thisArg: obj -> Float64Array

and PropertyKey =
    U3<string, float, Symbol>

and Symbol =
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract toString: unit -> string
    abstract valueOf: unit -> obj

and SymbolConstructor =
    abstract prototype: Symbol with get, set
    abstract hasInstance: Symbol with get, set
    abstract isConcatSpreadable: Symbol with get, set
    abstract iterator: Symbol with get, set
    abstract ``match``: Symbol with get, set
    abstract replace: Symbol with get, set
    abstract search: Symbol with get, set
    abstract species: Symbol with get, set
    abstract split: Symbol with get, set
    abstract toPrimitive: Symbol with get, set
    abstract toStringTag: Symbol with get, set
    abstract unscopables: Symbol with get, set
    [<Emit("$0($1...)")>] abstract callSelf: ?description: U2<string, float> -> Symbol
    abstract ``for``: key: string -> Symbol
    abstract keyFor: sym: Symbol -> string

and IteratorResult<'T> =
    abstract ``done``: bool with get, set
    abstract value: 'T option with get, set

and Iterator<'T> =
    abstract next: ?value: obj -> IteratorResult<'T>
    abstract ``return``: ?value: obj -> IteratorResult<'T>
    abstract throw: ?e: obj -> IteratorResult<'T>

and Iterable<'T> =
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> Iterator<'T>

and IterableIterator<'T> =
    inherit Iterator<'T>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<'T>

and GeneratorFunction =
    inherit Function
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set

and GeneratorFunctionConstructor =
    abstract prototype: GeneratorFunction with get, set
    [<Emit("new $0($1...)")>] abstract createNew: [<ParamArray>] args: string[] -> GeneratorFunction
    [<Emit("$0($1...)")>] abstract callSelf: [<ParamArray>] args: string[] -> GeneratorFunction

and Map<'K, 'V> =
    abstract size: float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract clear: unit -> unit
    abstract delete: key: 'K -> bool
    abstract entries: unit -> IterableIterator<'K * 'V>
    abstract forEach: callbackfn: Func<'V, 'K, Map<'K, 'V>, unit> * ?thisArg: obj -> unit
    abstract get: key: 'K -> 'V
    abstract has: key: 'K -> bool
    abstract keys: unit -> IterableIterator<'K>
    abstract set: key: 'K * ?value: 'V -> Map<'K, 'V>
    abstract values: unit -> IterableIterator<'V>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<'K * 'V>

and MapConstructor =
    abstract prototype: Map<obj, obj> with get, set
    [<Emit("new $0($1...)")>] abstract createNew: unit -> Map<obj, obj>
    [<Emit("new $0($1...)")>] abstract createNew: unit -> Map<'K, 'V>
    [<Emit("new $0($1...)")>] abstract createNew: iterable: Iterable<'K * 'V> -> Map<'K, 'V>

and WeakMap<'K, 'V> =
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract clear: unit -> unit
    abstract delete: key: 'K -> bool
    abstract get: key: 'K -> 'V
    abstract has: key: 'K -> bool
    abstract set: key: 'K * ?value: 'V -> WeakMap<'K, 'V>

and WeakMapConstructor =
    abstract prototype: WeakMap<obj, obj> with get, set
    [<Emit("new $0($1...)")>] abstract createNew: unit -> WeakMap<obj, obj>
    [<Emit("new $0($1...)")>] abstract createNew: unit -> WeakMap<'K, 'V>
    [<Emit("new $0($1...)")>] abstract createNew: iterable: Iterable<'K * 'V> -> WeakMap<'K, 'V>

and Set<'T> =
    abstract size: float with get, set
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract add: value: 'T -> Set<'T>
    abstract clear: unit -> unit
    abstract delete: value: 'T -> bool
    abstract entries: unit -> IterableIterator<'T * 'T>
    abstract forEach: callbackfn: Func<'T, 'T, Set<'T>, unit> * ?thisArg: obj -> unit
    abstract has: value: 'T -> bool
    abstract keys: unit -> IterableIterator<'T>
    abstract values: unit -> IterableIterator<'T>
    [<Emit("$0[Symbol.iterator]($1...)")>] abstract ``[Symbol.iterator]``: unit -> IterableIterator<'T>

and SetConstructor =
    abstract prototype: Set<obj> with get, set
    [<Emit("new $0($1...)")>] abstract createNew: unit -> Set<obj>
    [<Emit("new $0($1...)")>] abstract createNew: unit -> Set<'T>
    [<Emit("new $0($1...)")>] abstract createNew: iterable: Iterable<'T> -> Set<'T>

and WeakSet<'T> =
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract add: value: 'T -> WeakSet<'T>
    abstract clear: unit -> unit
    abstract delete: value: 'T -> bool
    abstract has: value: 'T -> bool

and WeakSetConstructor =
    abstract prototype: WeakSet<obj> with get, set
    [<Emit("new $0($1...)")>] abstract createNew: unit -> WeakSet<obj>
    [<Emit("new $0($1...)")>] abstract createNew: unit -> WeakSet<'T>
    [<Emit("new $0($1...)")>] abstract createNew: iterable: Iterable<'T> -> WeakSet<'T>

and ProxyHandler<'T> =
    abstract getPrototypeOf: target: 'T -> obj
    abstract setPrototypeOf: target: 'T * v: obj -> bool
    abstract isExtensible: target: 'T -> bool
    abstract preventExtensions: target: 'T -> bool
    abstract getOwnPropertyDescriptor: target: 'T * p: PropertyKey -> PropertyDescriptor
    abstract has: target: 'T * p: PropertyKey -> bool
    abstract get: target: 'T * p: PropertyKey * receiver: obj -> obj
    abstract set: target: 'T * p: PropertyKey * value: obj * receiver: obj -> bool
    abstract deleteProperty: target: 'T * p: PropertyKey -> bool
    abstract defineProperty: target: 'T * p: PropertyKey * attributes: PropertyDescriptor -> bool
    abstract enumerate: target: 'T -> ResizeArray<PropertyKey>
    abstract ownKeys: target: 'T -> ResizeArray<PropertyKey>
    abstract apply: target: 'T * thisArg: obj * ?argArray: obj -> obj
    abstract construct: target: 'T * thisArg: obj * ?argArray: obj -> obj

and ProxyConstructor =
    abstract revocable: target: 'T * handler: ProxyHandler<'T> -> obj
    [<Emit("new $0($1...)")>] abstract createNew: target: 'T * handler: ProxyHandler<'T> -> 'T

and Promise<'T> =
    [<Emit("$0[Symbol.toStringTag]{{=$1}}")>] abstract ``[Symbol.toStringTag]``: obj with get, set
    abstract ``then``: ?onfulfilled: Func<'T, U2<'TResult, PromiseLike<'TResult>>> * ?onrejected: Func<obj, U2<'TResult, PromiseLike<'TResult>>> -> Promise<'TResult>
    abstract ``then``: ?onfulfilled: Func<'T, U2<'TResult, PromiseLike<'TResult>>> * ?onrejected: Func<obj, unit> -> Promise<'TResult>
    abstract catch: ?onrejected: Func<obj, U2<'T, PromiseLike<'T>>> -> Promise<'T>
    abstract catch: ?onrejected: Func<obj, unit> -> Promise<'T>

and PromiseConstructor =
    abstract prototype: Promise<obj> with get, set
    [<Emit("$0[Symbol.species]{{=$1}}")>] abstract ``[Symbol.species]``: Function with get, set
    [<Emit("new $0($1...)")>] abstract createNew: executor: Func<Func<U2<'T, PromiseLike<'T>>, unit>, Func<obj, unit>, unit> -> Promise<'T>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> * U2<'T6, PromiseLike<'T6>> * U2<'T7, PromiseLike<'T7>> * U2<'T8, PromiseLike<'T8>> * U2<'T9, PromiseLike<'T9>> * U2<'T10, PromiseLike<'T10>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7 * 'T8 * 'T9 * 'T10>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> * U2<'T6, PromiseLike<'T6>> * U2<'T7, PromiseLike<'T7>> * U2<'T8, PromiseLike<'T8>> * U2<'T9, PromiseLike<'T9>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7 * 'T8 * 'T9>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> * U2<'T6, PromiseLike<'T6>> * U2<'T7, PromiseLike<'T7>> * U2<'T8, PromiseLike<'T8>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7 * 'T8>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> * U2<'T6, PromiseLike<'T6>> * U2<'T7, PromiseLike<'T7>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> * U2<'T6, PromiseLike<'T6>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> * U2<'T5, PromiseLike<'T5>> -> Promise<'T1 * 'T2 * 'T3 * 'T4 * 'T5>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> * U2<'T4, PromiseLike<'T4>> -> Promise<'T1 * 'T2 * 'T3 * 'T4>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> * U2<'T3, PromiseLike<'T3>> -> Promise<'T1 * 'T2 * 'T3>
    abstract all: values: U2<'T1, PromiseLike<'T1>> * U2<'T2, PromiseLike<'T2>> -> Promise<'T1 * 'T2>
    abstract all: values: Iterable<U2<'TAll, PromiseLike<'TAll>>> -> Promise<ResizeArray<'TAll>>
    abstract race: values: Iterable<U2<'T, PromiseLike<'T>>> -> Promise<'T>
    abstract reject: reason: obj -> Promise<unit>
    abstract resolve: value: U2<'T, PromiseLike<'T>> -> Promise<'T>
    abstract resolve: unit -> Promise<unit>

type Globals =
    [<Global>] static member NaN with get(): float = failwith "JS only" and set(v: float): unit = failwith "JS only"
    [<Global>] static member Infinity with get(): float = failwith "JS only" and set(v: float): unit = failwith "JS only"
    [<Global>] static member Object with get(): ObjectConstructor = failwith "JS only" and set(v: ObjectConstructor): unit = failwith "JS only"
    [<Global>] static member Function with get(): FunctionConstructor = failwith "JS only" and set(v: FunctionConstructor): unit = failwith "JS only"
    [<Global>] static member String with get(): StringConstructor = failwith "JS only" and set(v: StringConstructor): unit = failwith "JS only"
    [<Global>] static member Boolean with get(): BooleanConstructor = failwith "JS only" and set(v: BooleanConstructor): unit = failwith "JS only"
    [<Global>] static member Number with get(): NumberConstructor = failwith "JS only" and set(v: NumberConstructor): unit = failwith "JS only"
    [<Global>] static member Math with get(): Math = failwith "JS only" and set(v: Math): unit = failwith "JS only"
    [<Global>] static member Date with get(): DateConstructor = failwith "JS only" and set(v: DateConstructor): unit = failwith "JS only"
    [<Global>] static member RegExp with get(): RegExpConstructor = failwith "JS only" and set(v: RegExpConstructor): unit = failwith "JS only"
    [<Global>] static member Error with get(): ErrorConstructor = failwith "JS only" and set(v: ErrorConstructor): unit = failwith "JS only"
    [<Global>] static member EvalError with get(): EvalErrorConstructor = failwith "JS only" and set(v: EvalErrorConstructor): unit = failwith "JS only"
    [<Global>] static member RangeError with get(): RangeErrorConstructor = failwith "JS only" and set(v: RangeErrorConstructor): unit = failwith "JS only"
    [<Global>] static member ReferenceError with get(): ReferenceErrorConstructor = failwith "JS only" and set(v: ReferenceErrorConstructor): unit = failwith "JS only"
    [<Global>] static member SyntaxError with get(): SyntaxErrorConstructor = failwith "JS only" and set(v: SyntaxErrorConstructor): unit = failwith "JS only"
    [<Global>] static member TypeError with get(): TypeErrorConstructor = failwith "JS only" and set(v: TypeErrorConstructor): unit = failwith "JS only"
    [<Global>] static member URIError with get(): URIErrorConstructor = failwith "JS only" and set(v: URIErrorConstructor): unit = failwith "JS only"
    [<Global>] static member JSON with get(): JSON = failwith "JS only" and set(v: JSON): unit = failwith "JS only"
    [<Global>] static member Array with get(): ArrayConstructor = failwith "JS only" and set(v: ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member ArrayBuffer with get(): ArrayBufferConstructor = failwith "JS only" and set(v: ArrayBufferConstructor): unit = failwith "JS only"
    [<Global>] static member DataView with get(): DataViewConstructor = failwith "JS only" and set(v: DataViewConstructor): unit = failwith "JS only"
    [<Global>] static member Int8Array with get(): Int8ArrayConstructor = failwith "JS only" and set(v: Int8ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Uint8Array with get(): Uint8ArrayConstructor = failwith "JS only" and set(v: Uint8ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Uint8ClampedArray with get(): Uint8ClampedArrayConstructor = failwith "JS only" and set(v: Uint8ClampedArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Int16Array with get(): Int16ArrayConstructor = failwith "JS only" and set(v: Int16ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Uint16Array with get(): Uint16ArrayConstructor = failwith "JS only" and set(v: Uint16ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Int32Array with get(): Int32ArrayConstructor = failwith "JS only" and set(v: Int32ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Uint32Array with get(): Uint32ArrayConstructor = failwith "JS only" and set(v: Uint32ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Float32Array with get(): Float32ArrayConstructor = failwith "JS only" and set(v: Float32ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Float64Array with get(): Float64ArrayConstructor = failwith "JS only" and set(v: Float64ArrayConstructor): unit = failwith "JS only"
    [<Global>] static member Symbol with get(): SymbolConstructor = failwith "JS only" and set(v: SymbolConstructor): unit = failwith "JS only"
    [<Global>] static member GeneratorFunction with get(): GeneratorFunctionConstructor = failwith "JS only" and set(v: GeneratorFunctionConstructor): unit = failwith "JS only"
    [<Global>] static member Map with get(): MapConstructor = failwith "JS only" and set(v: MapConstructor): unit = failwith "JS only"
    [<Global>] static member WeakMap with get(): WeakMapConstructor = failwith "JS only" and set(v: WeakMapConstructor): unit = failwith "JS only"
    [<Global>] static member Set with get(): SetConstructor = failwith "JS only" and set(v: SetConstructor): unit = failwith "JS only"
    [<Global>] static member WeakSet with get(): WeakSetConstructor = failwith "JS only" and set(v: WeakSetConstructor): unit = failwith "JS only"
    [<Global>] static member Proxy with get(): ProxyConstructor = failwith "JS only" and set(v: ProxyConstructor): unit = failwith "JS only"
    [<Global>] static member Promise with get(): PromiseConstructor = failwith "JS only" and set(v: PromiseConstructor): unit = failwith "JS only"

type [<Global>] Reflect =
    static member apply(target: Function, thisArgument: obj, argumentsList: ArrayLike<obj>): obj = failwith "JS only"
    static member construct(target: Function, argumentsList: ArrayLike<obj>, ?newTarget: obj): obj = failwith "JS only"
    static member defineProperty(target: obj, propertyKey: PropertyKey, attributes: PropertyDescriptor): bool = failwith "JS only"
    static member deleteProperty(target: obj, propertyKey: PropertyKey): bool = failwith "JS only"
    static member enumerate(target: obj): IterableIterator<obj> = failwith "JS only"
    static member get(target: obj, propertyKey: PropertyKey, ?receiver: obj): obj = failwith "JS only"
    static member getOwnPropertyDescriptor(target: obj, propertyKey: PropertyKey): PropertyDescriptor = failwith "JS only"
    static member getPrototypeOf(target: obj): obj = failwith "JS only"
    static member has(target: obj, propertyKey: string): bool = failwith "JS only"
    static member has(target: obj, propertyKey: Symbol): bool = failwith "JS only"
    static member isExtensible(target: obj): bool = failwith "JS only"
    static member ownKeys(target: obj): ResizeArray<PropertyKey> = failwith "JS only"
    static member preventExtensions(target: obj): bool = failwith "JS only"
    static member set(target: obj, propertyKey: PropertyKey, value: obj, ?receiver: obj): bool = failwith "JS only"
    static member setPrototypeOf(target: obj, proto: obj): bool = failwith "JS only"


