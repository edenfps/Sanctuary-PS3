"""Guard the PS3 client's null resource-list branch seen in RPCS3's crash log.

At 0x78BD10, the nonempty branch goes to 0x78C5F0. The empty branch used to
set r31=0 and then read 0x3C(r31). Return zero through the function's existing
epilogue instead. This only changes the empty branch.
"""

from pathlib import Path
import hashlib
import shutil


game = Path(r"D:\RPCS3\dev_hdd0\game\NPEA00299\USRDIR\game.elf")
backup = Path(__file__).resolve().parent / "game.elf.before-empty-resource-guard"
file_offset = 0x77BD14  # ELF virtual address 0x78BD14; load segment offset 0x10000
original = bytes.fromhex("3CA000EC3CC0010B")
replacement = bytes.fromhex("3AA0000048000874")  # li r21,0; b 0x78C58C

if not backup.exists():
    shutil.copy2(game, backup)

blob = bytearray(game.read_bytes())
current = bytes(blob[file_offset:file_offset + len(original)])
if current == original:
    blob[file_offset:file_offset + len(original)] = replacement
    game.write_bytes(blob)
elif current != replacement:
    raise SystemExit(f"Unexpected bytes at offset {file_offset:#x}: {current.hex()}")

assert bytes(game.read_bytes()[file_offset:file_offset + len(replacement)]) == replacement
assert backup.read_bytes()[file_offset:file_offset + len(original)] == original
print("game.elf sha256:", hashlib.sha256(game.read_bytes()).hexdigest())
print("backup sha256:", hashlib.sha256(backup.read_bytes()).hexdigest())
