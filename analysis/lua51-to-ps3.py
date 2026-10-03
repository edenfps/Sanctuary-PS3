"""Convert 32-bit little-endian Lua 5.1 bytecode to PS3 big-endian bytecode."""
import struct
import sys
from pathlib import Path

source = Path(sys.argv[1]).read_bytes()
assert source[:12] == b"\x1bLua\x51\x00\x01\x04\x04\x04\x08\x00", source[:12].hex()
cursor = 12
out = bytearray(source[:12])
out[6] = 0


def convert(size):
    global cursor
    raw = source[cursor:cursor + size]
    assert len(raw) == size
    cursor += size
    out.extend(raw[::-1])
    return int.from_bytes(raw, "little")


def copy(size):
    global cursor
    out.extend(source[cursor:cursor + size])
    cursor += size


def string():
    n = convert(4)
    copy(n)


def function():
    string()
    convert(4)  # first source line
    convert(4)  # last source line
    copy(4)     # upvalues, arguments, vararg, stack size
    count = convert(4)
    for _ in range(count):
        convert(4)  # instruction
    count = convert(4)
    for _ in range(count):
        tag = source[cursor]
        copy(1)
        if tag == 1:
            copy(1)
        elif tag == 3:
            convert(8)  # floating-point number
        elif tag == 4:
            string()
        else:
            assert tag == 0, (cursor, tag)
    count = convert(4)
    for _ in range(count):
        function()
    count = convert(4)
    for _ in range(count):
        convert(4)  # source line map
    count = convert(4)
    for _ in range(count):
        string()
        convert(4)  # local start pc
        convert(4)  # local end pc
    count = convert(4)
    for _ in range(count):
        string()  # upvalue names


function()
assert cursor == len(source), (cursor, len(source))
Path(sys.argv[2]).write_bytes(out)
