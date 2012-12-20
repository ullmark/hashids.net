# Hashids
A small .NET package to generate YouTube-like hashes from one or many numbers. 
Use hashids when you do not want to expose your database ids to the user.

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

### Encrypting one number

You can pass a unique salt value so your hashes differ from everyone else's. I use "**this is my salt**" as an example.

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encrypt(12345);
```

`hash` is now going to be:

    ryBo

### Decrypting

Notice during decryption, same salt value is used:

```C#
var hashids = new Hashids("this is my salt");
numbers = hashids.Decrypt("ryBo");
```

`numbers` is now going to be:

    [ 12345 ]

### Decrypting with different salt

Decryption will not work if salt is changed:

```C#
var hashids = new Hashids("this is my pepper");
numbers = hashids.decrypt("ryBo");
```

`numbers` is now going to be:

    []

### Encrypting several numbers

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encrypt(683, 94108, 123, 5);
```

`hash` is now going to be:

    zBphL54nuMyu5

### Decrypting is done the same way

```C#
var hashids = new Hashids("this is my salt");
var numbers = hashids.Decrypt("zBphL54nuMyu5")
```

`numbers` is now going to be:

    [ 683, 94108, 123, 5 ]

### Encrypting and specifying minimum hash length

Here we encrypt integer 1, and set the minimum hash length to **8** (by default it's **0** -- meaning hashes will be the shortest possible length).

```C#
var hashids = new Hashids("this is my salt", 8);
var hash = hashids.Encrypt(1);
```

`hash` is now going to be:

    b9iLXiAa

### Decrypting

```C#
var hashids = new Hashids("this is my salt", 8);
var numbers = hashids.Decrypt("b9iLXiAa");
```

`numbers` is now going to be:

    [ 1 ]

### Specifying custom hash alphabet

Here we set the alphabet to consist of only four letters: "abcd"

```C#
var hashids = new Hashids("this is my salt", 0, "abcd")
var hash = hashids.encrypt(1, 2, 3, 4, 5)
```

`hash` is now going to be:

    adcdacddcdaacdad

## Randomness

The primary purpose of hashids is to obfuscate ids. It's not meant or tested to be used for security purposes or compression.
Having said that, this algorithm does try to make these hashes unguessable and unpredictable:

### Repeating numbers

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encrypt(5, 5, 5, 5);
```

You don't see any repeating patterns that might show there's 4 identical numbers in the hash:

    GLh5SMs9

Same with incremented numbers:

```C#
var hashids = new Hashids("this is my salt");
var hash = hashids.Encrypt(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
```

`hash` will be :

    zEUzfySGIpuyhpF6HaC7

### Incrementing number hashes:

```C#
var hashids = new Hashids("this is my salt");

hashids.Encrypt(1); // => LX
hashids.Encrypt(2); // => ed
hashids.Encrypt(3); // => o9
hashids.Encrypt(4); // => 4n
hashids.Encrypt(5); // => a5
```