"""Extract pristine September 2009 cache packs into an isolated client copy."""

from io import BytesIO
from pathlib import Path
import os
import shutil
import zipfile

ARCHIVE = Path(r"C:\Users\bobya\Downloads\Free Realms(1).zip")
DEST = Path(r"C:\Users\bobya\Documents\ChatGPT\free realms\client-2009-archive-test")
WANTED = {
    "Assets_000.dat",
    "Assets_001.dat",
    "Assets_002.dat",
    "Assets_003.dat",
    "Assets_manifest.dat",
    "Assets_manifest.txt",
}

with zipfile.ZipFile(ARCHIVE) as outer:
    nested_name = next(name for name in outer.namelist() if name.endswith(".zip"))
    nested_bytes = outer.read(nested_name)

with zipfile.ZipFile(BytesIO(nested_bytes)) as nested:
    for item in nested.infolist():
        name = Path(item.filename).name
        if name not in WANTED:
            continue
        temp = DEST / f"{name}.extracting"
        with nested.open(item) as source, temp.open("wb") as target:
            shutil.copyfileobj(source, target, length=1024 * 1024)
        os.replace(temp, DEST / name)
        print(name, item.file_size, flush=True)
