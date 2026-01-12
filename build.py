#!/usr/bin/env python3
"""
Build script for JustTray release.
Usage: python build.py <output_path>
"""

import argparse
import shutil
import subprocess
import sys
from pathlib import Path


def clean_directory(path: Path) -> None:
    """Remove directory contents if exists."""
    if path.exists():
        print(f"Cleaning output directory: {path}")
        shutil.rmtree(path)
    path.mkdir(parents=True, exist_ok=True)


def build_release(project_dir: Path, output_path: Path) -> bool:
    """Build the release version of the project."""
    print(f"Building release to: {output_path}")
    
    cmd = [
        "dotnet", "publish",
        str(project_dir / "JustTray.csproj"),
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-o", str(output_path)
    ]
    
    print(f"Running: {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=project_dir)
    
    return result.returncode == 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Build JustTray release")
    parser.add_argument("output_path", type=str, help="Output directory for the release build")
    args = parser.parse_args()
    
    output_path = Path(args.output_path).resolve()
    project_dir = Path(__file__).parent.resolve()
    
    print(f"Project directory: {project_dir}")
    print(f"Output path: {output_path}")
    
    # Clean output directory
    clean_directory(output_path)
    
    # Build release
    if not build_release(project_dir, output_path):
        print("Build failed!", file=sys.stderr)
        return 1
    
    print(f"\nBuild successful! Output: {output_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
