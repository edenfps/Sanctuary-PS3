"""Serve extracted 2009 assets, accepting numeric bucket paths."""

from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit

JULY = Path(r"E:\Free Realms 2009 client files\2009 frs dat unpack")
SEPTEMBER = Path(r"C:\assets2009\september-zip")
FALLBACK = Path(r"C:\Users\bobya\Documents\Free Realms Unpacker\editz fr assets\FR Assets 2025-07-07")
MANIFEST = Path(r"C:\assets2009")
SOUND_OVERRIDE = Path(__file__).parent / "asset-overrides" / "ActorSoundEmitterDefinitions.xml"
BLACKSPORE_OVERRIDES = {
    name.lower(): Path(r"E:\Free Realms 2009 client files\2009 dat unpack") / name
    for name in ("FabledRealms_16_-32.gcnk", "FabledRealms_16_-36.gcnk")
}


class Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"

    def do_GET(self):
        name = Path(unquote(urlsplit(self.path).path)).name
        if name in {"manifest.crc", "manifest.txt.z"}:
            path = MANIFEST / name
        elif name.lower() == SOUND_OVERRIDE.name.lower():
            path = SOUND_OVERRIDE
        elif name.lower() in BLACKSPORE_OVERRIDES:
            path = BLACKSPORE_OVERRIDES[name.lower()]
        else:
            path = JULY / name
            if not path.is_file():
                path = SEPTEMBER / name
            if not path.is_file():
                path = FALLBACK / name
        if not path.is_file():
            self.send_error(404, "Asset missing")
            return
        data = path.read_bytes()
        self.send_response(200)
        self.send_header("Content-Type", "application/octet-stream")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def log_message(self, format, *args):
        if args and str(args[1]) != "200":
            super().log_message(format, *args)


class AssetServer(ThreadingHTTPServer):
    request_queue_size = 128


if __name__ == "__main__":
    print("Serving assets at http://127.0.0.1:20043/assets", flush=True)
    AssetServer(("127.0.0.1", 20043), Handler).serve_forever()
