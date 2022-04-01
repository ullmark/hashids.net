# Hashids
A small .NET package to generate YouTube-like IDs from numbers.

It converts numbers like `347` into strings like `yr8`, or array of numbers like `[27, 986]` into `3kTMd`. You can also decode those ids back. This is useful in bundling several parameters into one, hiding actual IDs, or simply using them as short string IDs.

[http://www.hashids.org/net/](http://www.hashids.org/net/)

## Features

- Creates short unique ids from integers. *(only positive numbers & zero)*
- Generates non-sequential IDs for incremental input to stay unguessable.
- Supports single number or array of numbers. *(supports `int` and `long`)*
- Allows custom alphabet as well as salt — so ids are unique only to you. *(salt must be smaller than alphabet)*
- Allows specifying minimum hash length.
- Tries to avoid basic English curse words.

*NOTE: This is **NOT** a true cryptographic hash, since it is reversible*

## Installation
Install the package with [NuGet][]

    Install-Package hashids.net

## Usage

### Import namespace

```C#
using HashidsNet;
```

### Encoding one number

You can pass a unique salt value so your hashes differ from everyone else's. I use "**this is my salt**" as an example.

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encode(12345);
```

`hash` is now going to be:

    NkK9

If your id is stored as a `Int64` you need to use "EncodeLong".

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.EncodeLong(666555444333222L);
```

`hash` is now going to be:

    KVO9yy1oO5j

### Decoding

Notice during decoding, same salt value is used:

```C#
var hashids = new Hashids("this is my salt");
numbers = hashids.Decode("NkK9");
```

`numbers` is now going to be:

    [ 12345 ]

```C#
var hashids = new Hashids("this is my salt");
numbers = hashids.DecodeLong("KVO9yy1oO5j");
```

`numbers` is now going to be:

    [ 666555444333222L ]
    
### Decoding a single id

By default, Decode and DecodeLong will return an array. If you need to decode just one id you can use
the following helper functions:

```C#
var hashids = new Hashids("this is my pepper");
number = hashids.DecodeSingle("NkK9");
```

`number` is now going to be:

    12345

```C#
var hashids = new Hashids("this is my pepper");

if (hashids.TryDecodeSingle("NkK9", out int number)) { // Decoding hash successfull. }
```

`number` is now going to be:

    12345

You can handle the exception to see what went wrong with the decoding:

```C#
var hashids = new Hashids("this is my pepper");
try
{    
    number = hashids.DecodeSingle("NkK9");
}
catch (NoResultException) { // Decoding the provided hash has not yielded any result. }
catch (MultipleResultsException) { // The decoding process yielded more than one result when just one was expected. }
```

`number` is now going to be:

    12345

```C#
var hashids = new Hashids("this is my pepper");
number = hashids.DecodeSingleLong("KVO9yy1oO5j");
```

`number` is now going to be:

    666555444333222L

```C#
var hashids = new Hashids("this is my pepper");

if (hashids.TryDecodeSingleLong("NkK9", out long number)) { // Decoding hash successfull. }
```

`number` is now going to be:

    666555444333222L

```C#
var hashids = new Hashids("this is my pepper");
try
{
    number = hashids.DecodeSingleLong("KVO9yy1oO5j");
}
catch (NoResultException) { // Decoding the provided hash has not yielded any result. }
catch (MultipleResultsException) { // The decoding process yielded more than one result when just one was expected. }
```

`number` is now going to be:

    666555444333222L

### Decoding with different salt

Decoding will not work if salt is changed:

```C#
var hashids = new Hashids("this is my pepper");
numbers = hashids.Decode("NkK9");
```

`numbers` is now going to be:

    []

### Encoding several numbers

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encode(683, 94108, 123, 5);
```

`hash` is now going to be:

    aBMswoO2UB3Sj

### Decoding is done the same way

```C#
var hashids = new Hashids("this is my salt");
var numbers = hashids.Decode("aBMswoO2UB3Sj")
```

`numbers` is now going to be:

    [ 683, 94108, 123, 5 ]

### Encoding and specifying minimum hash length

Here we encode integer 1, and set the minimum hash length to **8** (by default it's **0** -- meaning hashes will be the shortest possible length).

```C#
var hashids = new Hashids("this is my salt", 8);
var hash = hashids.Encode(1);
```

`hash` is now going to be:

    gB0NV05e

### Decoding 

```C#
var hashids = new Hashids("this is my salt", 8);
var numbers = hashids.Decode("gB0NV05e");
```

`numbers` is now going to be:

    [ 1 ]

### Specifying custom hash alphabet

Here we set the alphabet to consist of: "abcdefghijkABCDEFGHIJK12345"

```C#
var hashids = new Hashids("this is my salt", 0, "abcdefghijkABCDEFGHIJK12345")
var hash = hashids.Encode(1, 2, 3, 4, 5)
```

`hash` is now going to be:

    Ec4iEHeF3

## Randomness

The primary purpose of hashids is to obfuscate ids. It's not meant or tested to be used for security purposes or compression.
Having said that, this algorithm does try to make these hashes unguessable and unpredictable:

### Repeating numbers

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encode(5, 5, 5, 5);
```

You don't see any repeating patterns that might show there's 4 identical numbers in the hash:

    1Wc8cwcE

Same with incremented numbers:

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encode(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
```

`hash` will be :

    kRHnurhptKcjIDTWC3sx

### Incrementing number hashes:

```C#
var hashids = new Hashids("this is my salt");

hashids.Encode(1); // => NV
hashids.Encode(2); // => 6m
hashids.Encode(3); // => yD
hashids.Encode(4); // => 2l
hashids.Encode(5); // => rD
```

### Encoding using a HEX string

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.EncodeHex("DEADBEEF");
```

`hash` is now going to be: 

    kRNrpKlJ

### Decoding to a HEX string

```C#
var hashids = new Hashids("this is my salt");
var hex = hashids.DecodeHex("kRNrpKlJ");
```

`hex` is now going to be:

    DEADBEEF

## Changelog

**1.4.1**
- Accepted PR [#45](https://github.com/ullmark/hashids.net/pull/45) - Cleanup unused nuget references and replace `Microsoft.Extensions.ObjectPool` with internal implementation.

**1.4.0**
- Modernized project with updated build targets now set to `netnet461`, `net5.0`, `netstandard2.0`
- Accepted PR [#30](https://github.com/ullmark/hashids.net/pull/30) - Fix floating-point math to handle large ratio of alphabet to separators.
- Accepted PR [#37](https://github.com/ullmark/hashids.net/pull/37) - Performance and memory optimizations.
- Accepted PR [#42](https://github.com/ullmark/hashids.net/pull/42) - Performance updates and added BenchmarkDotnet for profiling.
- Accepted PR [#43](https://github.com/ullmark/hashids.net/pull/43) - Improved performance and reduced allocations.  
- Fixed issues [#23](https://github.com/ullmark/hashids.net/issues/23), [#32](https://github.com/ullmark/hashids.net/issues/32), [#35](https://github.com/ullmark/hashids.net/issues/35) - Fix floating-point math, now replaced by Horner's method. 
- Fixed issue [#27](https://github.com/ullmark/hashids.net/issues/27) - Allow dashes in alphabet (dashes caused issues with Regex which is not used anymore).
- Fixed issue [#21](https://github.com/ullmark/hashids.net/issues/21) - Fix encoding exception when decoding a character used as guard. 
- Fixed issue [#29](https://github.com/ullmark/hashids.net/issues/29) - Added tests to confirm thread-safety.

**1.3.0**
- Accepted PR [#26](https://github.com/ullmark/hashids.net/pull/26) - We now support .netstandard2.0.

**1.2.2**
- Accepted PR [#19](https://github.com/ullmark/hashids.net/pull/19) - We now only instantiate the HEX-connected Regexes if we use any of the HEX functions. This will speed up creation of "Hashids"-instances. It 
is likely that most users doesn't use the HEX-functions.

**1.2.1**
- Accepted PR [#11](https://github.com/ullmark/hashids.net/pull/11)
- Fixed issue [#15](https://github.com/ullmark/hashids.net/issues/15) Decoding strings that contain characters not in the alphabet will now return empty array. (To conform to behaviour in the js-library).
- Fixed issue [#18](https://github.com/ullmark/hashids.net/issues/18) Encoding with a negative number will now return empty string. (To conform to behaviour in the js-library).

**1.2.0**
- .NET Core support.

**1.1.2**
- Fixed issue [#14](https://github.com/ullmark/hashids.net/issues/14) that caused HEX values to be encoded/decoded incorrectly.

**1.1.1**
- Accepted PR [#12](https://github.com/ullmark/hashids.net/pull/12) that fixed an issue when encoding very many longs at the same time

**1.1.0**

- Added support for `long` via *new* functions to not introduce breaking changes.
    - `EncodeLong` for encodes.
	- `DecodeLong` for decodes.
- Added interface `IHashids` for people who want an interface to work with.

**1.0.1**

- The .NET 4.0 version of the package used .NET 4.5 as build target. This was fixed and a new version was pushed to nuget. 

**1.0.0**

- Several public functions marked obsolete and renamed versions added, to be more appropriate:
	- Function `Encrypt()` changed to `Encode()`
	- Function `Decrypt()` changed to `Decode()`
	- Function `EncryptHex()` changed to `EncodeHex()`
	- Function `DecryptHex()` changed to `DecodeHex()`
	
	Hashids was designed to encode integers, primary ids at most. We've had several requests to encrypt sensitive data with Hashids and this is the wrong algorithm for that. So to encourage more appropriate use, `encrypt/decrypt` is being "downgraded" to `encode/decode`.

**0.3.4**

  - The public functions are now virtual and therefor can be mocked with a mocking library.

**0.3.3**

  - Rewrote the code to support the new hashing algorithm.
  - Support for `EncryptHex` and `DecryptHex`

**0.1.4**

  - Initial version of the port.

[Nuget]: http://nuget.org/
