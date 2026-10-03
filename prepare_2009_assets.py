"""Build a manifest with July 2009, September 2009, then 2025 assets."""

from pathlib import Path
import zlib

JULY = Path(r'E:\Free Realms 2009 client files\2009 frs dat unpack')
SEPTEMBER = Path(r'C:\assets2009\september-zip')
FALLBACK = Path(r'C:\Users\bobya\Documents\Free Realms Unpacker\editz fr assets\FR Assets 2025-07-07')
SOUND_OVERRIDE = Path(__file__).parent / 'asset-overrides' / 'ActorSoundEmitterDefinitions.xml'
OUT = Path(r'C:\assets2009')
BLACKSPORE_OVERRIDES = {
    name: Path(r'E:\Free Realms 2009 client files\2009 dat unpack') / name
    for name in ('FabledRealms_16_-32.gcnk', 'FabledRealms_16_-36.gcnk')
}


def main():
    for directory in (JULY, SEPTEMBER, FALLBACK):
        if not directory.is_dir():
            raise FileNotFoundError(directory)
    files = {}
    for directory in (FALLBACK, SEPTEMBER, JULY):
        files.update({path.name.lower(): path for path in directory.iterdir() if path.is_file()})
    # These cached/fallback chunks leave Blackspore tiles pending.
    files.update({name.lower(): path for name, path in BLACKSPORE_OVERRIDES.items()})
    # The original sound definition leaves this client silent.
    files[SOUND_OVERRIDE.name.lower()] = SOUND_OVERRIDE
    for path in files.values():
        if not path.is_file():
            raise FileNotFoundError(path)
    entries = []
    for path in sorted(files.values(), key=lambda p: p.name.lower()):
        data = path.read_bytes()
        entries.append(f'{path.name},{zlib.crc32(data) & 0xffffffff},{len(data)}\r\n')
    manifest = ''.join(entries).encode('utf-8')
    OUT.mkdir(exist_ok=True)
    (OUT / 'manifest.txt.z').write_bytes(
        b'\xa1\xb2\xc3\xd4' + len(manifest).to_bytes(4, 'big') + zlib.compress(manifest)
    )
    (OUT / 'manifest.crc').write_text(
        f'{zlib.crc32(manifest) & 0xffffffff},{len(manifest)}', encoding='ascii'
    )
    print(f'Built {len(entries)} assets; manifest {len(manifest)} bytes')


if __name__ == '__main__':
    main()
