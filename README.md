# Mars Grid Visualizer

This project currently implements a visualization for [LaserTag](https://github.com/MARS-Group-HAW/model-mars-laser-tag-game) with an aim to provide a more generic solution for grid-based simulations in the future.

# Development

## Required Programs

To run and develop the project locally you'll need to install the Godot version that supports C#. It can be downloaded through [godotengine.org/download/](https://godotengine.org/download/) or your respective package manager.

After installing Godot open the project in it and click the run button in the upper right corner.

For editing the C# scripts an external editor like [rider](https://www.jetbrains.com/rider/) is recommended since the built-in one does not provide code completion.

## Project Structure

- `Program.cs` is the main entry point of the program.
- `src` contains all other source files.
  - `Agent.cs` models lasertag agents
  - `Map.cs` handles reading and modelling the maps used by lasertag that are specified.

> [!NOTE]
> When changing settings in the Godot editor they are written to text files like `main.tscn` for example. Please don't forget to commit those changes as well.
