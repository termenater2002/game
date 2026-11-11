MAJOR DAMAGE — authored Style V3 sources

See ../../CHARACTER_STYLE_V3.md for reference analysis, asset paths, reproduction and verification.

Run make_style_textures.py with Python/Pillow, then build_all.py in Blender --background --factory-startup.
Do not run the isolated-session build_all.py inside an existing artist session: it removes other scenes from its own temporary file.
Individual build files require style_core.py definitions and V3_OUTPUT provided by build_all.py.

Original Koharu meshes, atlases and GIFs are not redistributed.
