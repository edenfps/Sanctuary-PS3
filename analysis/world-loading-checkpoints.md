# PS3 world-loading checkpoints

Static source: NPUA30048 `game.elf`, IDA function `sub_EC3DC` (`WaitForWorldReady`), decompile in `ida-decompile/0xEC3DC_0xEC3DC.c`.

After zone readiness, proxied character/actor, drawable actor, sky readiness, and sky definition checks, the client logs `Waiting for confirmation packet` and waits for all three conditions:

- byte at client object offset `0x2A0` (`InitialZoneDataComplete`), set to 1 at `0x1070CC` next to the `RECEIVED=ZoneDoneSendingInitialData` log string. This is the handler for server opcode 14.
- byte at offset `0x2A1` (`ReceivedPreloadDonePacket`), set to 1 at `0xFE6E4` in `sub_F9214` switch case 26. This matches the server's `ClientUpdatePacketDoneSendingPreloadCharacters` subopcode 26.
- `sub_1BDA50(a1[176])` (`RequiredCharactersLoaded`)

It logs those values about every five seconds while waiting. The server currently sends both matching opcodes in `StartingZone.OnClientIsReady`. At `0xED7DC`, after `WaitForWorldReady: waiting complete`, the PS3 client constructs and sends opcode 10 (`PacketClientFinishedLoading`). The earlier gateway trace received opcode 10; therefore that run passed zone, sky, proxy, and completion checks. The persistent loading screen on that run is later than this wait loop, or a new loading screen triggered after it. Trace the post-completion UI transition and server responses rather than changing these completion packets.

Authoritative log evidence: `src/bin/Debug/Logs/Sanctuary.Gateway-Info-2026-10-02.log` records opcode 10 at 22:09:44 and 22:22:46, followed by two opcode 109 messages and a `ListQueuesResponsePacket`. The later 22:27 and 22:30 tests disconnected with `CorruptPacket` before opcode 10. No new connection has yet tested the 22:55 item-definition batch build, so do not infer its behavior from the older runs.

The initial-run caller `sub_F0BF8` calls `WaitForWorldReady` with argument 1 at `0xF12CC`. On success it logs `Initialized - Client.` and enters its main update loop. This strengthens the conclusion that opcode 10 indicates the client passed the world-ready gate, but does not prove that the loading overlay disappeared or that subsequent server responses were valid. The post-completion UI transition is the next target.

The loading overlay is started by `sub_C72F8` via `sub_14420C`. `WaitForWorldReady` sends opcode 10 at `0xED7DC`, then reaches its common exit at `0xECD68`, which calls `sub_C5CA8`. That function calls `sub_1441B0` on the loading-screen object, which calls `sub_182930`/`sub_182834` to stop it. Thus opcode 10 is *before* overlay dismissal. A screen persisting after opcode 10 could mean a client stall in the short post-send path, failure to stop the overlay, or another overlay started later. The gateway trace alone cannot distinguish these.

`sub_182834` stops the overlay by signaling the loading-screen thread, then calls `sub_7EA0F8(thread, -1)` to join it with no finite timeout before clearing the thread pointer and marking the overlay stopped. This is a concrete place where the client could remain on the loading image even after opcode 10 if the rendering thread does not exit. It is a candidate, not a verified cause. A client thread stack or the game's logging would distinguish it from a later overlay.

The same function checks zone readiness through `sub_897B74` and logs a reason from `sub_897C54` while that is false. The relevant diagnostic string starts at ELF file offset `0xDDC638`, virtual address `0xDEC638`.

`sub_1BDA50` walks a linked list of required character proxies. It returns true when the list is empty or every required proxy has passed `sub_834EEC` (its ready predicate). Thus `RequiredCharactersLoaded` is an actual asset/proxy readiness check, not another server packet flag. Its decompile is in `ida-decompile/0x1BDA50_0x1BDA50.c`.

`sub_834EEC` requires five nonzero bytes on each proxy, at offsets `0x114`, `0x115`, `0x119`, `0x11A`, and `0x11B`. This check is independent of the two completion packet flags. `sub_19EB10` skips proxies already marked as complete or out of scope; `sub_19EA8C` resolves the actor for an outstanding proxy. Which server data or local resources set those five readiness bytes still needs tracing.

The 2026-10-03 07:06 client file log reaches `World Ready new loc=...` but
never `Initialized - Client.` The gateway receives opcode 10 and the client
marketing log says the loading screen was viewed for 2.72 seconds. In
`sub_F0BF8`, `Initialized - Client.` follows the `WaitForWorldReady` return.
The likely stall is therefore in its exit call to `sub_C5CA8`, especially
the `sub_182834` worker-thread join at `0x182870`. This is still a hypothesis.
For one reversible test, the active NPUA30048 ELF was backed up as
`game.elf.pre-loading-join-test-20261003.bak`; the four-byte call at ELF
offset `0x172870` was replaced by a PPC `nop` (`60 00 00 00`). The worker
stop flag is still set before this call. The next boot must establish whether
the client reaches `Initialized - Client.` or the world, and whether skipping
the join causes a later error. Restore the backup if it does not help.

Candidate actor initialization `sub_83918C` sets `0x115`, `0x11A`, and `0x11B` to 1, sets `0x114` to 0, and sets `0x119` from an initialization argument. Another setter, `sub_89B1AC`, sets `0x114` based on a resource manager request `sub_65351C`. These routines suggest actor readiness can wait on local resource loading; class identity and linkage to the required character proxies need confirmation before changing server packets.
