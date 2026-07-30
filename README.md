# Texture Batch Compressor

A Unity Editor tool for batch texture compression, built for VRChat world optimization.

Texture Batch Compressor helps creators scan one or more folders inside `Assets`, preview texture import changes, and apply compression settings in bulk for PC and mobile targets. It is designed for workflows where a world contains many map or model textures and manual importer edits would be slow, repetitive, and easy to miss.

## Features

- Batch scan multiple target folders inside `Assets`
- Presets for `Map > PC`, `Map > Mobile`, `Model > PC`, and `Model > Mobile`
- Standalone and Android platform override support
- Dry Run preview before modifying assets
- Filters for normal maps, Sprite/UI textures, file extensions, path keywords, and textures already under the target size
- Progress bar with cancel support
- Multilingual UI: English, Chinese, Japanese, and Korean
- Built-in guide window that follows the selected language
- Processing reports written to the Unity Console

## Usage

Open the tool from Unity:

```text
Tools > Texture Batch Compressor
```

Recommended workflow:

1. Add one or more target folders.
2. Click `Scan All`.
3. Choose a map or model preset for PC or mobile.
4. Adjust filters and import settings.
5. Run a Dry Run preview.
6. Disable Dry Run and apply only after confirming the preview.

For the full tutorial and risk notes, see:

[TextureBatchCompressor_UserGuide.md](TextureBatchCompressor_UserGuide.md)

## Important Notes

This tool changes Unity texture importer settings and `.meta` files. Use version control or back up your project before applying bulk changes. Very low texture sizes or wrong settings on normal maps, UI textures, or text-heavy images can noticeably reduce visual quality.

## Compatibility

Tested against Unity `2022.3.22f1` editor references in this project.
