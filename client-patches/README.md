# September 2009 UI test client

`ScriptsBase.bin` is the tested 2009 UI script bundle copied from
`client-2009-test/UI/ScriptsBase.bin` (SHA256
`27c0f4f4d26fca628cee577b43e800e8edd11639470fc0ca45000e7520445c2e`).

Relative to `ScriptsBase.bin.original`, it has two Lua 5.1 bytecode changes:

1. `welcome.lua` skips `PopulateMarketplaceItems`, whose 2014 server data does not match this client.
2. `group.lua` `GetNextJob` returns `-1` after scanning all profile rows without a match, instead of looping forever. Normal matching rows still use the original code.

The scripts `../patch_test_welcome_marketplace.py` and
`../patch_test_group_next_job.py` reproduce these changes in the test client.
The running game uses the copy in `client-2009-test/UI`, not this Git copy.
