# PS3 display resolution path

Analyzed `game.elf.i64` from NPUA30048. The video initialization function is
`sub_759310` at `0x759310`.

- It calls `cellVideoOutGetResolutionAvailability` for resolution IDs 1, 2, 5,
  and 4, then `cellVideoOutGetResolution` for the selected mode.
- The returned width and height feed the render-buffer allocations at
  `sub_758E88`, `cellGcmSetDisplayBuffer`, and `cellVideoOutConfigure`.
- The function also assigns logical UI dimensions `1280x720` for the wide
  modes, or `1120x840` for the alternate aspect ratio. These are separate from
  the render-buffer dimensions.
- It recognizes output heights 480, 576, 720, and 1080. There is no 2160
  branch or PS3 video mode for native 4K output.

The RPCS3 NPUA30048 profile previously selected a 1280x720 PS3 output with
300% render scaling. To exercise the game's 1080p path, use a 1920x1080 PS3
output with 200% render scaling. The latter still produces a 3840x2160 scaled
3D image, while the game itself allocates 1080p render buffers. UI art and
logical coordinates remain based on the game's own dimensions.

The profile is external to this repository at
`C:\Users\bobya\Downloads\rpcs3\config\custom_configs\config_NPUA30048.yml`.
The original profile was saved beside it as `config_NPUA30048.before-1080p.yml`.
