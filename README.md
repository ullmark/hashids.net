# Hashids
A small .NET package to generate YouTube-like hashes from one or many numbers. 
Use hashids when you do not want to expose your database ids to the user.

[http://www.hashids.org/net/](http://www.hashids.org/net/)

## What is it?

hashids (Hash ID's) creates short, unique, decryptable hashes from unsigned integers.

_(NOTE: This is **NOT** a true cryptographic hash, since it is reversible)_

It was designed for websites to use in URL shortening, tracking stuff, or 
making pages private (or at least unguessable).

This algorithm tries to satisfy the following requirements:

1. Hashes must be unique and decryptable.
2. They should be able to contain more than one integer (so you can use them in complex or clustered systems).
3. You should be able to specify minimum hash length.
4. Hashes should not contain basic English curse words (since they are meant to appear in public places - like the URL).

Instead of showing items as `1`, `2`, or `3`, you could show them as `U6dc`, `u87U`, and `HMou`.
You don't have to store these hashes in the database, but can encrypt + decrypt on the fly.

All integers need to be greater than or equal to zero.

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

**1.2.2**
- Accepted PR [#19](https://github.com/ullmark/hashids.net/pull/19) - We now only instantiate the HEX-connected Regexes if we use any of the HEX functions. This will speed up creation of "Hashids"-instances. It 
is likely that most users doesn't use the HEX-functions.
- Version tag added: `1.2.2`

**1.2.1**
- Accepted PR [#11](https://github.com/ullmark/hashids.net/pull/11)
- Fixed issue [#15](https://github.com/ullmark/hashids.net/issues/15) Decoding strings that contain characters not in the alphabet will now return empty array. (To conform to behaviour in the js-library).
- Fixed issue [#18](https://github.com/ullmark/hashids.net/issues/18) Encoding with a negative number will now return empty string. (To conform to behaviour in the js-library).
- Version tag added: `1.2.1`
- `README.md` updated

**1.2.0**
- .NET Core support. Sorry for the wait and thanks [haroldma](https://github.com/haroldma), 
[mlafleur](https://github.com/mlafleur) and [lstyles](https://github.com/lstyles) for submitting pull requests.
- Version tag added: `1.2.0`
- `README.md` updated

**1.1.2**
- Fixed issue [#14](https://github.com/ullmark/hashids.net/issues/14) that caused HEX values to be encoded/decoded incorrectly.
- Version tag added `1.1.2`

**1.1.1**
- Accepted PR [#12](https://github.com/ullmark/hashids.net/pull/12) that fixed an issue when encoding very many longs at the same time
- `README.md` updated
- Version tag added: `1.1.1`

**1.1.0**

- Added support for `long` via *new* functions to not introduce breaking changes.
    - `EncodeLong` for encodes.
	- `DecodeLong` for decodes.
- Added interface `IHashids` for people who want an interface to work with.
- Version tag added: `1.1.0`
- `README.md` updated

**1.0.1**

- The .NET 4.0 version of the package used .NET 4.5 as build target. This was fixed and a new version was pushed to nuget. 

**1.0.0**

- Several public functions marked obsolete and renamed versions added, to be more appropriate:
	- Function `Encrypt()` changed to `Encode()`
	- Function `Decrypt()` changed to `Decode()`
	- Function `EncryptHex()` changed to `EncodeHex()`
	- Function `DecryptHex()` changed to `DecodeHex()`
	
	Hashids was designed to encode integers, primary ids at most. We've had several requests to encrypt sensitive data with Hashids and this is the wrong algorithm for that. So to encourage more appropriate use, `encrypt/decrypt` is being "downgraded" to `encode/decode`.

- Version tag added: `1.0`
- `README.md` updated

**0.3.4**

  - The public functions are now virtual and therefor can be mocked with a mocking library.

**0.3.3**

  - Rewrote the code to support the new hashing algorithm.
  - Support for `EncryptHex` and `DecryptHex`

**0.1.4**

  - Initial version of the port.

[Nuget]: http://nuget.org/