#!/usr/bin/env python3
"""
Lossless image compression for the Unity project.

Like TinyPNG's lossless mode: re-encodes PNGs at max compression and strips
metadata, optimizes JPEG Huffman tables + strips metadata. Pixels are never
touched — output is bit-for-bit identical in quality, just smaller files.

NOTE: this shrinks the *source* files (smaller repo, faster imports). The
deployed bundle size is set by Unity's GPU texture format, not the PNG bytes —
use WebGLBuild.ApplyTextureCompression / resolution caps for download size.

Usage:
    python3 Tools/compress-images.py [--root Assets] [--dry-run] [--jobs 8]

Requires: ImageMagick 7 (`magick`), jpegoptim (brew install jpegoptim)
"""

import argparse
import os
import shutil
import subprocess
import sys
import tempfile
from concurrent.futures import ProcessPoolExecutor, as_completed

PNG_EXT = {".png"}
JPG_EXT = {".jpg", ".jpeg"}

# Never touch these — template/marketing assets are already optimized copies,
# and non-asset dirs don't get imported by Unity anyway.
SKIP_DIRS = {"Library", "Temp", "obj", ".git", "Builds", "ServerData", "UserSettings"}


def compress_png(path: str) -> int:
    """Rewrite PNG losslessly: strip metadata, brute-force best zlib settings."""
    fd, tmp = tempfile.mkstemp(suffix=".png")
    os.close(fd)
    try:
        r = subprocess.run(
            ["magick", path, "-strip",
             "-define", "png:compression-level=9",
             "-define", "png:compression-strategy=5",
             "-define", "png:compression-filter=5",
             tmp],
            capture_output=True, timeout=300)
        if r.returncode != 0:
            return 0
        return _keep_smaller(path, tmp)
    finally:
        if os.path.exists(tmp):
            os.unlink(tmp)


def compress_jpg(path: str) -> int:
    """jpegoptim --strip-all is lossless (optimizes Huffman tables, strips metadata)."""
    before = os.path.getsize(path)
    r = subprocess.run(["jpegoptim", "--strip-all", "--quiet", path],
                       capture_output=True, timeout=300)
    if r.returncode != 0:
        return 0
    return max(0, before - os.path.getsize(path))


def _keep_smaller(orig: str, candidate: str) -> int:
    """Replace orig with candidate only if strictly smaller. Returns bytes saved."""
    o, c = os.path.getsize(orig), os.path.getsize(candidate)
    if c < o:
        shutil.copystat(orig, candidate)
        os.replace(candidate, orig)
        return o - c
    return 0


def work(path: str):
    ext = os.path.splitext(path)[1].lower()
    try:
        if ext in PNG_EXT:
            return path, compress_png(path), None
        if ext in JPG_EXT:
            return path, compress_jpg(path), None
    except Exception as e:  # noqa: BLE001 - never abort the batch on one bad file
        return path, 0, str(e)
    return path, 0, None


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--root", default="Assets", help="directory to scan (default: Assets)")
    ap.add_argument("--dry-run", action="store_true", help="list files without compressing")
    ap.add_argument("--jobs", type=int, default=os.cpu_count() or 4)
    args = ap.parse_args()

    targets = []
    total_before = 0
    for root, dirs, files in os.walk(args.root):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for fn in files:
            if os.path.splitext(fn)[1].lower() in PNG_EXT | JPG_EXT:
                p = os.path.join(root, fn)
                targets.append(p)
                total_before += os.path.getsize(p)

    print(f"Found {len(targets)} images ({total_before / 1048576:.1f} MB) under {args.root}")
    if args.dry_run:
        return 0

    saved = errors = 0
    done = 0
    with ProcessPoolExecutor(max_workers=args.jobs) as ex:
        for path, delta, err in ex.map(work, targets):
            done += 1
            if err:
                errors += 1
                print(f"  !! {path}: {err}", file=sys.stderr)
            elif delta:
                saved += delta
            if done % 200 == 0:
                print(f"  {done}/{len(targets)} — saved {saved / 1048576:.1f} MB so far")

    print(f"\nDone: {len(targets)} files, saved {saved / 1048576:.1f} MB "
          f"({total_before / 1048576:.1f} -> {(total_before - saved) / 1048576:.1f} MB), "
          f"{errors} errors")
    return 0


if __name__ == "__main__":
    sys.exit(main())
