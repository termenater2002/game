# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.10] - 2024-12-16

### Changed

- Changed trial expiration message to let users chose organisation.

## [2.0.9] - 2024-11-28

### Fixed

- Fixed issue where the Unity Editor would crash using a Mac M1.
- Fixed issue in Muse settings where path separation character could be different in the same path.

## [2.0.8] - 2024-11-20

### Changed

- Updated App UI version.

### Fixed

- Fix gap between operators and Artifacts list.

## [2.0.7] - 2024-10-24

### Changed

- Changed default path where Muse assets are saved.
- Changed default texture name to readable save file name.

### Fixed

- Fix Muse dropdown flickering.

## [2.0.6] - 2024-10-07

### Added

- Added function to set dropzone message.

### Changed

- Improve Generation data design.

### Fixed

- Fix Muse dropdown layout.
- Fix App UI localization.
- Fix Muse dropdown sometimes not visible.
- Fix Onboarding loop workflow issue.

## [2.0.5] - 2024-09-06

### Changed

- Minor change to trial, beta and experimental program.

### Fixed

- Fix app ui muse plugin conflict.

### Removed

- Remove recently added (2.x) .children in favor of .history.

## [2.0.4] - 2024-08-15

### Fixed

- Fix "Unity Muse AI" tag not being added on all exported assets.
- Fix "Open in Muse" option.
- Fix toolbar exception in batch mode.

## [2.0.3] - 2024-08-07

### Added

- Added Support of MacOS 10.14

## [2.0.2] - 2024-08-06

### Fixed

- Fix pattern visuals.
- Fix invalid characters in file names.

## [2.0.1] - 2024-08-02

### Fixed

- Fix Muse dropdown.

## [2.0.0] - 2024-07-30

### Added

- Muse Chat context retrieval and plugin integration.

### Changed

- Change NodesList UI definition so that it can be reuse in other EditorWindow.
- Change Muse Account dropdown style.

### Fixed

- Fix selection changing when adding/removing items in the Generations list.

## [1.0.0] - 2024-04-15

### Changed

- Update version to "1.0.0".

## [1.0.0-pre.21] - 2024-04-05

## [1.0.0-pre.20] - 2024-04-02

### Fixed

- Fix organizations not showing in project settings.

### Removed

- Remove App UI as a dependency.
- Remove Settings Manager as a dependency.
- Remove Burst as a dependency.

## [1.0.0-pre.19] - 2024-03-14

## [1.0.0-pre.18] - 2024-03-11

## [1.0.0-pre.10] - 2024-03-06

### Changed

- Update com.unity.dt.app-ui to "1.0.3".

## [1.0.0-pre.4] - 2024-02-16

### Added

- Add characters limit for prompts.

### Changed

- Muse tools now use a Unity Editor theme.

### Fixed

- Generate button not updated when prompts is set from Generation Settings -> Use.
- Fix bug where items could be unselected in refinement.
- Fix shortcuts in Refine mode not always working.
- Fix Star and Unstar not working on multiple elements in the generations panel.
- Fix Save shortcut not working on a new Muse window.
- Fix option icons overlapping at specific window sizes.
- Fix new Muse window getting dirty without any changes.
- Fix Asset list view performance when there are a large number of items.
- Fix Muse points label overflowing.
- Fix "ExecuteMenuItem failed" error.

## [1.0.0-pre.3] - 2023-12-15

### Fixed

- Fix error when trying to build a Unity Project.

### Changed

- Brush tool order in the Refinement mode.
- Doodle pad's cursor color in light mode.

## [1.0.0-pre.2] - 2023-11-16

## [1.0.0-pre.1] - 2023-11-16

### Added

- Add deselect support from mouse click in the Generations grid.
- Add card stack for refined artifacts.
- Add Ledger system.

### Changed

- Use PointerEvent instead of MouseEvent during inpainting.
- Change styling on multiple components.
- Merge Prompt and Negative Prompt.

### Fixed

- Fixed drop shadows styling.

## [0.4.1] - 2023-10-20

### Changed

- Change context menu text for the Generations.

### Fixed

- Delete shortcuts are no longer applied to all elements.
- Correct operators are set when leaving refine mode after changing the thumbnail artifact.

## [0.3.1] - 2023-10-12

### Fixed

- Fix NullReferenceException when saving multiple generations.
- Fix the casing check of dropped texture files.

## [0.3.0] - 2023-09-28

### Added

- Add Muse Preferences window

### Changed

- Improve asset creation workflow - selecting Menu -> Muse -> Muse Sprite / Texture creates a temporary asset that can be saved later.


### Fixed

- Fix generate button is enabled with whitespace only prompt.
- Fix duplicated generation settings when using "Use".
- Control toolbar settings persists even if the tool was deactivated.
- Export button not visible in the Assets list.

## [0.2.0] - 2023-09-20

### Changed

- Improve in-painting experience.
- Artifacts cannot be unselected in refinement.

### Fixed

- Fix delete not appearing in context menu when there was an error with the generation.
- Fix title of Muse window reseting when reloading the window.
- Fix error when working with a large number of selected assets.
- Fix artifact favorite icon not showing after changing the thumbnail.

## [0.1.2] - 2023-09-12

## [0.1.1] - 2023-08-28

## [0.1.0] - 2023-06-10

### Added

- Initial release of the Unity Muse AI Tools package.
