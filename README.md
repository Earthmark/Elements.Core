## What is this?
This is a library for various core datatypes for FrooxEngine (which powers Resonite - a social VR sandbox platform) - vector, matrix, color and other types, parsing, serialization, string operations, math operations and more.

Resonite is a free social VR sandbox platform, which allows for socialization and collaborative in-game building. While game content can be fully built in-game (either in desktop or VR modes), not every user prefers this workflow. Unity SDK opens a new way to build content for Resonite, by using the Unity Editor and more traditional workflow or existing tooling. 

You can get Resonite free on Steam: https://store.steampowered.com/app/2519830/Resonite/

## Can I use this in my project?
If you find this useful, yes, please do! A lot of these datatypes were implemented because we found the existing ones lacking in functionality, so if they're useful to us, they should be useful to you too!

## Why some some stuff implemented weird?
FrooxEngine used to run within Unity game engine - at original under .NET 2.0 at that. Because of this, some initial design choices had to be made in this environment, where a lot of modern C# & .NET features were not available and where we had to work around limitations of Unity's JIT & GC. Our hope is to modernize and replace those parts gradually over time.

## Why open source this?
There are three major reasons:

1) Serve as a reference for anyone using Resonite, making our systems less of a black box
2) Allow for community contributions to improve Resonite more directly
3) Provide useful library for other projects and games written in C#

# Contributing

### Add tests before you contribute
Add tests to Element.Core.Tests if they do not exist yet for whatever code area you're changing

- This should be **separate PR** before your actual changes
- We need to make sure your changes do not break existing behavior (unless it's a bug that you're fixing)

### Add tests for new code
If you're adding new code, please include tests for this as well. This helps us ensure that it keeps working with future changes and there are no surprises.

### Keep PR's simple and to the point
- **DO NOT** mix multiple changes in a PR - keep them separate and easy to review
- The smaller the PR the better - it's easier for us to review
- This is a good resource on good PR's: https://mtlynch.io/code-review-love/

### Benchmark changes
Especially if you're working on optimizations, write Benchmark.NET tests to profile the code in various scenarios!

It's difficult to reason about performance and sometimes changes that might seem like they would be faster actually end up making the code slower as a result - that's why it's important to test.

We will **NOT accept** any optimization PR's without some data showing that they are faster.

### Test with Resonite & community content
If you're modifying existing behavior, compile a new version of this library and put it in Resonite to test with. Run through community content to make sure there's no regressions & breakages in behavior.

If you're a community member interested in getting particular changes into Resonite - help test too!

#### If content breakage is unavoidable
If there's existing Resonite content that depends on a behavior of a bug:
- Document it - what content breaks & why
- You can help by making a "Legacy" method that preserves old behavior if possible
- We'll take care of upgrade paths that will make old content use the old behavior

## What types of contributions are we looking for?
- Optimizations
    - Writing optimized paths for math operations
    - Adding SIMD paths
    - Using modern .NET types where possible (e.g. System.Numerics)
- Fixes
    - Is the math wrong in some places? Are there some edge cases? Help us fix those!
    - Be careful about changing behavior (test the changes in Resonite) and add tests first
- Tests
    - More tests are good!
    - For most contributions, we ask you add tests firts
    - They avoid content breakage when changes happen
    - Add Benchmark.NET tests as well to ensure we don't get regressions
- New Features
    - Want Resonite to have new math/geometry operations? Implement those in this library and we might expose them!
    - Want operations for datatypes that we do not support (e.g. complex number types, new matrix operations and so on?)
    - Any new additions should be in spirit of this library and relatively simple - do not hijack this library to add complex features that don't have much to do with this
- Cleanup
    - A lot of the code is pretty old and tangled, 
