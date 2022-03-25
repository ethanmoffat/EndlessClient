# Contributing

This is a general guide for contributing to the EndlessClient project. Pull requests that do not meet the minimum of these requirements will not be accepted, and you will be referred to this document. This is an evolving document and is subject to change without notice.

Note that all contributions are welcome! I'd rather work with a contributor to meet these requirements than not have any contributions at all, so please be open to receiving feedback if you open a PR.

### Code Style

Here are the style requirements for this project. This is the bare minimum you must meet when submitting a PR.

- Use spaces for indentation tabs
- Use 4 spaces to represent one tab
- Use var instead of explicit types (except for where required by the compiler)
- Use the latest available C# syntax
  - Example: `public int Property => 5;` instead of `public int Property { get { return 5; } }`
- Use formatting from Visual Studio/ReSharper by default
  - Spaces after keywords
    - Example: `if (condition)`, `for (int i = 0...)`
    - Do not: `if(condition)`
  - Curly braces on the next line
  - Curly braces may be ommitted from trivial lines, but should be left in for readability
    - Example:
    ```c#
    for (int i = 0; i < 5; ++i)
       if (i == 3)
         continue;
    ```
  - Maintain appropriate whitespace
    - Group statements together for logical cohesion
    - Put empty lines around functions, properties, etc.
- Avoid use of explicit `this` or `base` keywords

### Naming

Try to follow standard C# namining conventions

- Name interfaces with an `I` prefix (.Net convention)
- Make private/protected fields `readonly` when possible
- Public access to fields should always be through properties
- Fields should be named with an `_` prefix and then `camelCase`
- Public members (methods/properties/types) should be `PascalCase`

### Architecture

Here are a few notes on the architecture of the project

- EndlessClient uses a unity-based dependency injection framework to automatically map interfaces to implementations. See [AutomaticTypeMapper](https://github.com/ethanmoffat/AutomaticTypeMapper) for more information.
- Types should have an interface that can be mocked for unit testing.
- Follow the repository/provider pattern for storing/getting data
- Collections should be IReadOnly(List|Dictionary|...) if the caller doesn't need write access
- Types should be immutable
- Events should be avoided (see EOLib.Notifiers), except for in UI-related code (XNAControls/dialogs specifically)
  - This requirement may change in the future
- Separate the domain from the client code
- Do not add project references where they don't need to be
  - Example: EOLib should not depend on MonoGame for **any** reason
- If you must, use `async`/`await` where it makes sense instead of `System.Threading.Thread`
  - Try to avoid multiple threads
- Avoid static classes in favor of instances that can be used via the dependency injection system

### Unit tests

While good test coverage is an eventual goal, testing isn't a huge priority right now. However, keeping the code testable *is*; this is the reason for adding interfaces for everything and relying heavily on dependency injection.
