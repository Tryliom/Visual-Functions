# Visual Functions
A runtime script interpreter to code small code logic or prototyping in Unity from the editor.
Designed to be more used by developers since most of the functions are using a code syntax to run the logic.

Use these tools to make your life easier:
- Variable: A scriptable object that contains a type
- Reference: A field that can accept a Variable or a local type

## Example 
A system to move the player based on inputs WASD.

Here the inputs are stored in a variable called `MoveValue`,
that is a reference to a Vector3 variable, that can be used everywhere.

![SimpleInputManager.png](documentation/images/SimpleInputManager.png)

And here the player uses the `MoveValue` to move the player very simply.

![SimplePlayerMove.png](documentation/images/SimplePlayerMove.png)

More examples can be found [here](./documentation/MoreExamples.md).

## Pros
- Easy to use for a programmer
- Code-like syntax
- No need to compile
- Powerful, flexible logic
- Editable from the editor and during runtime
- Built-in components
- Can be extended with custom functions (code)

## Cons
This is slow, event Unity Visual Scripting that is known to be the slowest among the visual scripting tools, is faster than this in some cases.

This table shows the time in milliseconds it takes to run some tests and the memory used compared to Unity Visual Scripting and code.

| Tests           | Unity Visual Scripting  | Visual Functions            | Code                |
|-----------------|-------------------------|-----------------------------|---------------------|
| Float Increment | 9'301 ms - 0.95 mb      | 5'394 ms - 22.9 mb          | 14 ms - 0 mb        |
| Random Vec2     | 36’489 ms - 11.44 mb    | 52’002 ms - 151.15 mb       | 36 ms - 0 mb        |
| List Loop       | 10’849 ms - 0.95 mb     | 21’762 ms - 51.49 mb        | 10 ms - 9.5 E-07 mb |
| List Multiply   | 634’047 ms - 240.43 mb  | 2’021’720 ms - 5’343.02 mb  | 629 ms - 0 mb       |

The different tests were made with the same logic, but using different tools, they consist of repeating the same logic 1'000'000 times, and measuring the time it takes to run them:
- Increment a float variable by 1
- Generate a random Vector2 variable
- Add an int to a list
- Loop through a list of 50 elements, take elements by pairs and multiply them, then insert the result in a new list

The project where these tests were made can be found [here](https://github.com/Tryliom/Comparison-UnityVS-Csharp-Visual-Functions), they were made the 08/07/2025.

## Features
Contains basic functions used to run code logic:
- Evaluate: Interpret a string as a code and run it
- Timer: Run a function after a delay
- Counter: Run a function after a number of times
- Call Game Event: Trigger a game event
- For: Run a function for a number of times
- Loop: Run a function while a condition is met
- If: Run a function if a condition is met, else run another function
- Log: Log a message to the console, you can use variables in the message
- Reset: Reset a variable to its default value

It doesn't contain a lot of built-in functions, but with the evaluate function, you can run any code logic you want.

### Evaluate
Currently, the evaluate function supports all mathematical, logical and object methods operations.
For example, we have Vec that is a variable of type Vector3, we can do:
`Vec.x = Random.Range(Vec.y, Vec.z)` and it will set the x value of Vec to a random value between y and z.

Loop and For functions can also use the evaluate function to run a code logic.

#### Missing and planned features
- Rename variable asset name from the editor
- Better error handling

#### Known Issues
- Editing a field will not save the asset, so you need to save it manually (control + S)
- Undo is supported, but on some operations, it will not update the view

## Install
In Unity, go to `Window` menu -> `Package Manager` -> `+` -> `install package from git url` and put this url:
```
https://github.com/Tryliom/Visual-Functions.git
```

In Unity, you need to set the API compatibility level to `.NET Framework` in `Edit` -> `Project Settings` -> `Player` -> `Other Settings` -> `Configuration` -> `Api Compatibility Level`.

## Notes
Folder needed:
- `Resources/ScriptableObjects/Variables`: Where the game objects variables are stored
- `Resources/ScriptableObjects/GlobalVariables`: Where you need to store the global variables to be accessible from anywhere
- `Resources/ScriptableObjects/GlobalObjects`: Where you need to store other objects to be accessible from anywhere (like ExportableFields)

You can change these folder paths by creating a scriptable object with the menu `VisualFunctions/Settings` and set the paths in the inspector.
**It needs to be in a Resources folder to be loaded at runtime.**

## Usage
todo

## Coding
### Add a IValue
todo

### Add a reference
todo

### Add a function
todo