# 2009 game dock investigation

## Confirmed

- The September 2009 client uses `UI/UiModules/Main/wndGameDock2.xml` and `gamedock2.gfx`. The movie is in `C:/assets2009/raw/gamedock2.gfx`; its external texture `gamedock2_I4.dds` was added to the local asset source and manifest.
- `UI/ScriptsBase.bin` contains the compiled 2009 `gamedock.lua`. `GameDock.Init` calls the movie's `addDockItem` and `createDock` methods at runtime. The dock is therefore the original GFX movie plus runtime Lua data, rather than a later `gamedock.gfx` asset.
- `welcome.lua` calls `GameDock.Init` and `HUD.showMain` from `WelcomeHandler.Hide`.
- The 2009 welcome packet is opcode 94 with a different payload from modern Sanctuary. Commit `a15cd15` sends the 2009 layout; the client renders its original welcome screen.
- The original dock still has **not** appeared. Clicking the welcome X and subsequent client activity can leave the client busy in Lua/UI code. Dumps are in `E:/FreeRealms_2009_Codex_Backups/`.

## Diagnostic experiments (isolated client copy only)

- `client-2009-test/UI/ScriptsBase.bin.original` is the original compiled script. `ScriptsBase.bin.marketplace-patched` disables `WelcomeHandler.PopulateMarketplaceItems` to avoid a repeated store data read. The current `ScriptsBase.bin` matches that marketplace-patched test.
- `ScriptsBase.bin.context-skip-hide2-test` additionally skips `Context.Pop` in `WelcomeHandler.Hide` and calls the existing `hide2` closure directly. The client remained responsive in this run, but the welcome panel stayed visible; this is **not** a working fix or proof that `Context.Pop` is the sole cause.
- Disabling `GameDock.Init` alone did not remove the observed busy state.
- The asset log also reports missing generated `img*.dds` entries. Their source has not been found, and their relation to the dock is unproven.

## Next diagnostic

Trace whether the welcome X actually invokes `WelcomeHandler.Hide` in each run, and inspect the Lua/context stack when it does. Once the welcome window closes reliably, verify `GameDock.Init` creates the original bottom icons before porting them to the newer client.
