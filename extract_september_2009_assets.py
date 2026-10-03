"""Extract the asset cache from the client in Free Realms(1).zip."""

from pathlib import Path
import struct
import zlib

CLIENT = Path(r'C:\Users\bobya\Documents\ChatGPT\free realms\client-2009-archive-test')
OUT = Path(r'C:\assets2009\september-zip')
RECORD_SIZE = 148


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    manifest = (CLIENT / 'Assets_manifest.dat').read_bytes()
    if len(manifest) % RECORD_SIZE:
        raise ValueError('Unexpected cache index length')
    packs = [(CLIENT / f'Assets_{i:03}.dat').open('rb') for i in range(4)]
    extracted = 0
    try:
        for pos in range(0, len(manifest), RECORD_SIZE):
            record = manifest[pos:pos + RECORD_SIZE]
            name_length = struct.unpack_from('<I', record)[0]
            if not 0 < name_length <= RECORD_SIZE - 20:
                raise ValueError(f'Invalid filename length at record {pos // RECORD_SIZE}')
            name = record[4:4 + name_length].decode('utf-8')
            if Path(name).name != name:
                raise ValueError(f'Invalid filename: {name}')
            offset, reserved, size, checksum = struct.unpack_from('<IIII', record, 4 + name_length)
            if reserved != 0:
                raise ValueError(f'Unexpected cache metadata for {name}: {reserved}')
            target = OUT / name
            if (target.exists() and target.stat().st_size == size
                    and zlib.crc32(target.read_bytes()) & 0xffffffff == checksum):
                continue
            remaining = size
            chunks = []
            while remaining:
                pack_number = offset // 209715200
                pack_offset = offset % 209715200
                if pack_number >= len(packs):
                    raise ValueError(f'Cache ends inside {name}')
                pack = packs[pack_number]
                pack.seek(pack_offset)
                chunk = pack.read(min(remaining, 209715200 - pack_offset))
                if not chunk:
                    raise ValueError(f'Cache ends inside {name}')
                chunks.append(chunk)
                remaining -= len(chunk)
                offset += len(chunk)
            data = b''.join(chunks)
            if len(data) != size or zlib.crc32(data) & 0xffffffff != checksum:
                raise ValueError(f'Cache CRC mismatch for {name}')
            target.write_bytes(data)
            extracted += 1
    finally:
        for pack in packs:
            pack.close()
    print(f'Extracted {extracted} assets from September 2009 cache')


if __name__ == '__main__':
    main()
