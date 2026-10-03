"""Guard a zero material divisor in the 2009 client's geometry builder.

The unmodified client faults at FreeRealms.exe+0x314624 when a Shrouded Glade
material has zero atlas columns. Preserve the original executable separately.
"""

from pathlib import Path
import struct


client = Path(__file__).resolve().parents[2] / 'client-2009-test'
original = client / 'FreeRealms.exe.original'
target = client / 'FreeRealms.exe'
image_base = 0x400000
hook_va = 0x7145B3
resume_va = 0x7145BF
cave_va = 0xC64600
hook_offset = hook_va - image_base
cave_offset = cave_va - image_base
expected = bytes.fromhex('89 54 24 18 33 D2 89 93 D4 00 00 00')

assert original.read_bytes()[hook_offset:hook_offset + len(expected)] == expected, 'Unexpected original client build'
data = bytearray(target.read_bytes())

def jump(source: int, destination: int) -> bytes:
    return b'\xE9' + struct.pack('<i', destination - (source + 5))

# When the atlas column count is zero, use one column before the modulo.
cave = bytes.fromhex('85 D2 75 05 BA 01 00 00 00') + expected
cave += jump(cave_va + len(cave), resume_va)
patched_hook = jump(hook_va, cave_va) + bytes([0x90]) * (len(expected) - 5)
if data[hook_offset:hook_offset + len(expected)] == patched_hook and data[cave_offset:cave_offset + len(cave)] == cave:
    print('Geometry guard already applied')
    raise SystemExit(0)
assert data[hook_offset:hook_offset + len(expected)] == expected, 'Unexpected client bytes at hook'
assert data[cave_offset:cave_offset + 32] == bytes(32), 'Code cave is occupied'
data[cave_offset:cave_offset + len(cave)] = cave
data[hook_offset:hook_offset + len(expected)] = patched_hook
target.write_bytes(data)
print(f'Patched {target} ({len(data)} bytes)')
