#!/usr/bin/env python3
"""Local dev server for the WebGL build.

Serves Builds/WebGL as static files and forwards /api/* requests to the
Ningo backend, so the game can use same-origin API URLs and avoid CORS.

Usage:  python3 Tools/serve-webgl.py [port]     (default port 8734)
"""
import http.server
import os
import sys
import urllib.request
import urllib.error

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Builds", "WebGL"))
API_BASE = "https://ningoafrica.app"
PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8734

HOP_BY_HOP = {"transfer-encoding", "connection", "keep-alive", "content-encoding",
              "content-length", "host", "accept-encoding"}


class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=ROOT, **kwargs)

    def _proxy(self):
        body = None
        length = int(self.headers.get("Content-Length") or 0)
        if length:
            body = self.rfile.read(length)
        headers = {k: v for k, v in self.headers.items()
                   if k.lower() not in HOP_BY_HOP}
        req = urllib.request.Request(API_BASE + self.path, data=body,
                                     headers=headers, method=self.command)
        try:
            res = urllib.request.urlopen(req)
        except urllib.error.HTTPError as e:
            res = e
        self.send_response(res.status)
        for k, v in res.headers.items():
            if k.lower() not in HOP_BY_HOP:
                self.send_header(k, v)
        self.end_headers()
        if self.command != "HEAD":
            self.wfile.write(res.read())

    def end_headers(self):
        # Always revalidate — build filenames aren't hashed, so stale
        # cached wasm would silently run old code after a rebuild.
        self.send_header("Cache-Control", "no-cache")
        super().end_headers()

    def _handle(self):
        if self.path.startswith("/api/"):
            self._proxy()
        else:
            super().do_GET()

    do_GET = do_POST = do_PUT = do_DELETE = do_PATCH = do_OPTIONS = _handle


if __name__ == "__main__":
    if not os.path.isdir(ROOT):
        sys.exit(f"Build not found at {ROOT} — run the WebGL build first.")
    print(f"Serving {ROOT} on http://localhost:{PORT}")
    print(f"Proxying /api/* -> {API_BASE}")
    http.server.ThreadingHTTPServer(("127.0.0.1", PORT), Handler).serve_forever()
