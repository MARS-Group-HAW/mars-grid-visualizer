# Mars Grid Visualizer

This project currently implements a visualization for [LaserTag](https://github.com/MARS-Group-HAW/model-mars-laser-tag-game) with an aim to provide a more generic solution for grid-based simulations in the future.

# What does MGV do?

There are three parts to MGV.

1. It communicates with a MARS-Simulation through a [WebSocket-Connection](https://de.wikipedia.org/wiki/WebSocket),
2. loads a map through a CSV file
3. and renders them in a nice fashion, by mapping both simulation-data and map to sprites.

# Usage

To use this project in tandem with a MARS-Simulation, go to [GitHub Releases](https://github.com/MARS-Group-HAW/mars-grid-visualizer/releases) and download a zip file for your respective OS.
After unzipping you should find an executable inside and run it.

> [!note]
> On macOS there are some additional permissions required to.
> You can run the following command in a terminal:
> ```bash
> TODO
> ```

## Using Nix

If you have [nix](https://nixos.org/) installed:
```bash
nix run github:MARS-Group-HAW/mars-grid-visualizer
```

## From Source

You'll need git and the dotnet toolchain.

Clone the git repo:
```bash
git clone https://github.com/MARS-Group-HAW/mars-grid-visualizer
```
or if you have ssh setup
```bash
git clone git@github.com:MARS-Group-HAW/mars-grid-visualizer.git
```

Go into the new director:
```bash
cd mars-grid-visualizer
```

and run MGV with:
```bash
dotnet build && godot-mono .
```

# Future Outlook

Currently there are some known limitations that could be tackled in the future.

- following the course of the simulation isn't easy as there are no animations and many actions aren't visible
- right now the only working simulation is [LaserTag](https://github.com/MARS-Group-HAW/model-mars-laser-tag-game) and even there, not all map sizes are supported

# Development

## Project Structure

- `src` contains all other source files.
  - `Program.cs` is the main entry point of the program.
  - `Agent.cs` models LaserTag agents
  - `Map.cs` handles reading and modelling the maps used by LaserTag that are specified.

> [!NOTE]
> When changing settings in the Godot editor they are written to text files like `main.tscn` for example. Please don't forget to commit those changes as well.

## Naming Conventions

- private variables and methods are lower case and use no underscore at the beginning to
  set Godot built-ins more apart
- public properties/methods are upper case
- files use PascalCase

## Required Programs

To run and develop the project locally you'll need to install the Godot version that supports C#. It can be downloaded through [godotengine.org/download/](https://godotengine.org/download/) or your respective package manager.

After installing Godot open the project in it and click the run button in the upper right corner.

For editing the `C#` scripts an external editor like [rider](https://www.jetbrains.com/rider/) is recommended since the code editor built into Godot, does not provide code completion for `C#`.
