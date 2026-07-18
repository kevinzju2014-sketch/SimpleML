#!/usr/bin/env bash
# One-click install SimpleML release onto THIS macOS/Linux Rhino machine
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"

if [[ -f "$HERE/SimpleML.gha" && -d "$HERE/myML" ]]; then
  SRC="$HERE"
elif [[ -f "$HERE/SimpleML-1.2.0/SimpleML.gha" ]]; then
  SRC="$HERE/SimpleML-1.2.0"
elif [[ -f "$HERE/SimpleML-1.2.0.zip" ]]; then
  echo "Unzipping SimpleML-1.2.0.zip ..."
  unzip -qo "$HERE/SimpleML-1.2.0.zip" -d "$HERE"
  SRC="$HERE/SimpleML-1.2.0"
else
  echo "ERROR: cannot find SimpleML.gha / SimpleML-1.2.0"
  exit 1
fi

if [[ ! -f "$SRC/SimpleML.gha" ]]; then
  echo "ERROR: $SRC/SimpleML.gha not found"
  exit 1
fi

if command -v python3 >/dev/null 2>&1; then
  PYTHON_BIN="$(command -v python3)"
elif command -v python >/dev/null 2>&1; then
  PYTHON_BIN="$(command -v python)"
else
  echo "ERROR: need python3"
  exit 1
fi
if [[ -n "${PYTHON_PATH:-}" ]]; then
  PYTHON_BIN="$PYTHON_PATH"
fi

CANDIDATES=(
  "$HOME/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries"
  "$HOME/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries"
  "$HOME/Library/Application Support/Grasshopper/Libraries"
  "$HOME/.config/Grasshopper/Libraries"
)
TARGET=""
for c in "${CANDIDATES[@]}"; do
  if [[ -d "$c" ]]; then TARGET="$c"; break; fi
done
if [[ -z "$TARGET" ]]; then
  TARGET="$HOME/.config/Grasshopper/Libraries"
  mkdir -p "$TARGET"
fi

DEST="$TARGET/SimpleML"
echo "Installing from: $SRC"
echo "Installing to:   $DEST"
mkdir -p "$DEST"
find "$DEST" -mindepth 1 -maxdepth 1 -exec rm -rf {} + 2>/dev/null || true
cp -R "$SRC"/. "$DEST/"
cp "$SRC/SimpleML.gha" "$TARGET/SimpleML.gha"
"$PYTHON_BIN" -m pip install -q -r "$DEST/myML/requirements.txt"

echo "SIMPLEML_PATH=$DEST/myML"
echo "PYTHON_PATH=$PYTHON_BIN"
echo "DONE. Fully quit Rhino, reopen Grasshopper, run Health Check."
