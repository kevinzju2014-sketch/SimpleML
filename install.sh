#!/usr/bin/env bash
# SimpleML installer for macOS / Linux (Rhino 7+)
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
echo "=== SimpleML Installer ==="
echo "Repo: $ROOT"

PYTHON_BIN="${PYTHON_PATH:-}"
if [[ -z "${PYTHON_BIN}" ]]; then
  if command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="$(command -v python3)"
  elif command -v python >/dev/null 2>&1; then
    PYTHON_BIN="$(command -v python)"
  else
    echo "ERROR: 未找到 python3。请先安装 Python 3.9+（macOS 可用 brew install python）"
    exit 1
  fi
fi

echo "Python: $PYTHON_BIN"
"$PYTHON_BIN" -m pip install --upgrade pip
"$PYTHON_BIN" -m pip install -r "$ROOT/requirements.txt"

# Detect Rhino Libraries on macOS
HOME_DIR="${HOME}"
CANDIDATES=(
  "$HOME_DIR/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries"
  "$HOME_DIR/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries"
  "$HOME_DIR/Library/Application Support/Grasshopper/Libraries"
)

TARGET=""
for c in "${CANDIDATES[@]}"; do
  if [[ -d "$c" ]]; then TARGET="$c"; break; fi
done

if [[ -z "$TARGET" ]]; then
  TARGET="${CANDIDATES[1]}"
  mkdir -p "$TARGET"
fi

DEST="$TARGET/SimpleML"
mkdir -p "$DEST"
# Copy Python package as myML
rsync -a --delete \
  --exclude '.git' --exclude 'GHA_Project/bin' --exclude 'GHA_Project/obj' \
  --exclude '__pycache__' \
  "$ROOT/" "$DEST/myML/" 2>/dev/null || {
  mkdir -p "$DEST/myML"
  cp -R "$ROOT/components" "$ROOT/core" "$ROOT/requirements.txt" "$DEST/myML/"
}

# Copy gha if built
GHA="$ROOT/GHA_Project/bin/Release/SimpleML.gha"
if [[ ! -f "$GHA" ]]; then
  GHA="$ROOT/GHA_Project/bin/Debug/SimpleML.gha"
fi
if [[ -f "$GHA" ]]; then
  cp "$GHA" "$DEST/SimpleML.gha"
  cp "$GHA" "$TARGET/SimpleML.gha"
  echo "Installed GHA -> $TARGET/SimpleML.gha"
else
  echo "WARN: 未找到已编译的 SimpleML.gha"
  echo "请在装有 Rhino 的机器上执行:"
  echo "  dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7"
  echo "然后将 .gha 复制到: $TARGET"
fi

export SIMPLEML_PATH="$DEST/myML"
echo "SIMPLEML_PATH=$SIMPLEML_PATH"
echo "请将以下行写入 ~/.zshrc 或 ~/.bash_profile（可选）:"
echo "  export SIMPLEML_PATH=\"$DEST/myML\""
echo "  export PYTHON_PATH=\"$PYTHON_BIN\""
echo ""
echo "完成。请重启 Rhino，运行「环境体检」组件。"
