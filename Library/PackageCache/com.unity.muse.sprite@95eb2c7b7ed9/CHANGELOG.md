# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2024-08-07

## [1.1.0] - 2024-08-06

## [1.1.0-pre.6] - 2024-08-02

## [1.1.0-pre.5] - 2024-07-31

### Added

- Add multi-selection of Training Images for the Style Trainer.

### Changed

- Rename "Muse Sprite Tool" to "Muse Sprite".
- Change Style Trainer UI.

### Fixed

- Fix search bar focus being lost when typing.

### Added
- New internal generation APIs
 
## [1.0.0] - 2024-04-15

### Changed

- Update version to "1.0.0".

## [1.0.0-pre.21] - 2024-04-05

### Changed

- Update com.unity.muse.common dependency version.

## [1.0.0-pre.20] - 2024-04-02

## [1.0.0-pre.19] - 2024-03-14

## [1.0.0-pre.18] - 2024-03-11

## [1.0.0-pre.6] - 2024-02-16

### Changed

- Update com.unity.muse.common dependency version.

## [1.0.0-pre.5] - 2024-02-16

### Fixed
- Fixed description field in Style Trainer makes other UI components misaligned when expanded.
- Fixed duplicated style some times doesn't show training images.

## [1.0.0-pre.4] - 2023-12-15

### Fixed

- Fix error when trying to build a Unity Project.

## [1.0.0-pre.3] - 2023-11-17

### Fixed

- Opening Style Trainers in multiple editors results in errors.
- Sprite's Style Selection dropdown shows an out of range warning when toggling style visibility.
- Styles not available in the Style Selection dropdown after editor restart.

### Changed

- Update built-in style version number.

## [1.0.0-pre.2] - 2023-11-16

### Fixed

- Fix ellipsis not visible in refine mode.
- Upgrade from previous version without errors.
- Fix performance issue with accessing large number of Artifacts.

### Changed

- Update default, built-in style list.

## [1.0.0-pre.1] - 2023-11-16

### Changed
- Change style trainer flow. Each style now trains a predetermined number of versions.

### Fixed

- Fixed custom seed toggle visual consistency.
- Clearing printing triggers console warnings.
- Operator styles.
- Fix style trainer cache file access error.
- Swap transparent pixels with black in the reference image.
- Add missing brush radius slider.

## [0.4.1] - 2023-10-20

### Added

- Add option for Default Path of Muse assets in Preferences.

### Fixed

- Fixed Style Trainer split view handle can be dragged beyond the intended size.
- Fix UltraLiteDB could not be found when doing builds.

## [0.3.0] - 2023-09-28

### Added

- Feedback mechanism.

### Changed

- Style Trainer cache DB moved to project's library folder in Unity Editor.
- Changed star icon in version drop down to better reflect the selected version.

### Fixed

- Remove special characters from suggested export name.
- Doodle pad works after entering-exiting playmode.
- Reduce image preloading in Style Trainer.
- Fix unable to load default style in when default style project goes into a weird state.
- Remove incorrect icons in the style dropdown.
- Fix index out of range exception in style version drop down.

## [0.2.0] - 2023-09-20

### Changed

- Reduce error logging.
- Style selection is now part of the Sprite Generation operator. Old Sprite Muse asset will enounter exception and might not be usable. Please start with a new project.

### Fixed

- Dragging and dropping elements that are outside of the GridView's ScrollView to the Project, or Scene view.
- Fix style selection using incorrect settings.
- Fix grid view alignment in style trainer.
- Fix empty name and description allowed in style trainer.


## [0.1.2] - 2023-09-12

## [0.1.1] - 2023-08-28

### Added

- Added style training capabilities.
- Allow users to select custom trained style for generation.

## [0.1.0] - 2023-06-10

### Added

- Initial release of the Unity Muse AI Tools package.
