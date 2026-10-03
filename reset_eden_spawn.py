"""Reset the local 2009 test character to Snowhill's valid spawn point."""

from pathlib import Path
import sqlite3

DB = Path(__file__).parent / "src" / "bin" / "Debug" / "sanctuary.db"
SNOWHILL = (51.391, 30.987, 363.156)

with sqlite3.connect(DB) as connection:
    row = connection.execute(
        "SELECT FirstName FROM Characters WHERE Id = 1"
    ).fetchone()
    if row != ("Eden",):
        raise SystemExit("Character 1 is not Eden; no position changed.")
    connection.execute(
        "UPDATE Characters SET PositionX = ?, PositionY = ?, PositionZ = ? WHERE Id = 1",
        SNOWHILL,
    )
    connection.commit()

print(f"Eden spawn set to Snowhill {SNOWHILL}")
