# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- smooth agent movement via Tween interpolation
- simple play/pause UI button
- Auto-reconnect after lost WebSocket connection
- disconnect indicator label
- dynamic map path loading from server
- Chickensoft GameTools for UI scale handling
- GitHub workflow to test project builds
- This CHANGELOG file to better track changes

### Changed
- upgraded to Godot 4.6
- started refactor to multitier architecture (Presentation/Domain/Infrastructure)
  - extracted WebSocket functionality into separate class
  - moved JSON serialization to Adapter layer
- unified project naming and normalized namespaces following Chickensoft conventions

### Fixed
- Map not rendering initially
- Barrel animation
- Test setup

## [0.0.4] - 2025-07-16

Initial tracked release.

[Unreleased]: https://github.com/MARS-Group-HAW/mars-grid-visualizer/compare/v0.0.4...HEAD
[0.0.4]: https://github.com/MARS-Group-HAW/mars-grid-visualizer/releases/v0.0.4
