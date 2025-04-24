# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.0-exp.3] - 2025-01-06

### Fixed

- Duplicated toolbar icons when switching from a Loop key to a Pose key.
- Edits made to a keyframe with "Loop to first pose" enabled are neither saved nor applied to the baked animation.
- Opening a Muse Animate asset from the Project window does not work.

### Added

 - Ability to rename a take in the Library by double-clicking on the name or selecting "Rename" from the "..." menu.

## [1.2.0-exp.2] - 2024-10-15

### Fixed

- Fixed errors related to AssetDatabase that prevented the tool from working in Unity 6.

## [1.2.0-exp.1] - 2024-10-07

### Added

- Brand new Take Library view.
- See an animated preview of a take in the Library when you hover over the item in the Library.
- Custom icons for Muse Animate assets.
- Editor Analytics.

### Changed

- Animations are now saved as individual asset files, called Takes. Previously, multiple takes were saved as part of a single "Session" asset. This is a breaking change, meaning that previous Session assets are no longer supported.
- Interface for creating editable takes has been moved to the left side panel.
- Support only Sentis 2.x

### Fixed

- Loading time has been vastly improved, especially when there are numerous generations in the Library.
- Fix InputSystem error after uninstalling the InputSystem package.
- Improve time to export to humanoid clip.
- Playback controls when converting to an editable take now function as expected.
- Fixed issue where it was not possible to orbit the camera by holding down the Option key on macOS.

## [1.1.0-exp.5] - 2024-09-18

### Fixed

- Fix compilation errors with Input System.

## [1.1.0-exp.3] - 2024-09-16

### Fixed

- Support only Sentis 1.x
- Support Muse Common 2.0.4 or greater.

## [1.1.0-exp.2] - 2024-08-02

## [1.1.0-exp.1] - 2024-07-17

### Changed

- Rename "Muse Animate Tool" to "Muse Animate".
- Fix input to use new InputSystem if enabled.
- Undo/redo history now covers a variety of actions, is less buggy.

### Added

- Muse Chat integration to Muse Animate.
- Option to change the default asset path in the settings.
- Support for Unity 6 preview.
- Support for Drag & Drop animation clips from Muse Animate to Unity Editor

### Fixed

- Lightprobes in scene affecting in-tool lighting.
- Scene gets progressively brighter when new sessions are opened without closing the Muse Animate window. 
- Minor styling tweaks.
- Library thumbnails not updating when editing an animation.
- Entering Play Mode causes tool to stop working.
- A removed animation from the library will not reappear anymore next time the window is opened.

## [1.0.0-exp.10] - 2024-04-22

### Removed

- All non-public APIs have been made `internal`.

### Added

- Getting started instructions.

## [1.0.0-exp.9] - 2024-04-15

### Changed

- Motion completion is on a small delay to prevent excess requests while posing.
- Upgraded Sentis dependency to 1.4.0-pre.3

### Fixed

- UI not updating when window does not have focus.
- Character selection outline not working in HDRP and URP.
- Viewport becomes unresponsive after docking the window.
- Closing and re-opening Unity causes current session to be lost.

## [1.0.0-exp.8] - 2024-04-08

### Added

 - Initial experimental release of Muse Animate package.
