#!/usr/bin/env bash
# Build SimpleML.gha (NuGet Rhino refs if needed), stage dist/, install to Grasshopper Libraries.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DIST="${ROOT}/dist/SimpleML"
CONFIG="${CONFIG:-Release}"

# Prefer user-local dotnet if present
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$HOME/.dotnet/tools:$PATH"

echo "=== SimpleML build & install ==="
echo "ROOT=$ROOT"
echo "DIST=$DIST"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Installing .NET SDK 8 to $HOME/.dotnet ..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
fi

echo "dotnet: $(command -v dotnet) ($(dotnet --version))"

PYTHON_BIN="${PYTHON_PATH:-}"
if [[ -z "${PYTHON_BIN}" ]]; then
  if command -v python3 >/dev/null 2>&1; then PYTHON_BIN="$(command -v python3)"
  elif command -v python >/dev/null 2>&1; then PYTHON_BIN="$(command -v python)"
  else echo "ERROR: Python 3.9+ required"; exit 1; fi
fi
echo "Python: $PYTHON_BIN"
"$PYTHON_BIN" -m pip install -q --upgrade pip
"$PYTHON_BIN" -m pip install -q -r "$ROOT/requirements.txt"

echo "--- Compile GHA ---"
cd "$ROOT/GHA_Project"
dotnet restore SimpleML.csproj
dotnet build SimpleML.csproj -c "$CONFIG" -p:UseNuGetRhino=true -p:RhinoMajorVersion=7 --no-restore

GHA="$ROOT/GHA_Project/bin/${CONFIG}/SimpleML.gha"
DLL="$ROOT/GHA_Project/bin/${CONFIG}/SimpleML.dll"
if [[ ! -f "$GHA" && -f "$DLL" ]]; then
  cp "$DLL" "$GHA"
fi
if [[ ! -f "$GHA" ]]; then
  echo "ERROR: SimpleML.gha not found after build"
  ls -la "$ROOT/GHA_Project/bin/${CONFIG}/" || true
  exit 1
fi
echo "Built: $GHA ($(wc -c < "$GHA") bytes)"

echo "--- Stage dist/SimpleML ---"
rm -rf "$DIST"
mkdir -p "$DIST/myML"
cp "$GHA" "$DIST/SimpleML.gha"
cp -R "$ROOT/components" "$DIST/myML/"
cp -R "$ROOT/core" "$DIST/myML/"
cp "$ROOT/requirements.txt" "$DIST/myML/"
cp -R "$ROOT/docs" "$DIST/docs" 2>/dev/null || true
cp -R "$ROOT/examples" "$DIST/examples" 2>/dev/null || true
cat > "$DIST/README_INSTALL.txt" <<EOF
SimpleML 发布包（已编译）

内容:
  SimpleML.gha     — Grasshopper 插件
  myML/            — Python 包（components + core）
  docs/            — 安装与用户文档
  examples/        — 示例

本机安装（任选其一）:
  1) 在仓库根目录再跑:  bash scripts/build_and_install.sh
  2) 手动复制本目录到 Grasshopper Libraries/SimpleML/

Windows Libraries:
  %APPDATA%\\Grasshopper\\Libraries\\SimpleML\\

macOS Libraries:
  ~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
  ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/

安装后请完全退出并重启 Rhino，运行「环境体检」。
EOF

echo "Dist staged: $DIST"
find "$DIST" -maxdepth 2 -type f | head -40

echo "--- Install to Grasshopper Libraries ---"
# Prefer real macOS / Windows-style paths; on Linux create a standard staging Libraries path
CANDIDATES=(
  "$HOME/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries"
  "$HOME/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries"
  "$HOME/Library/Application Support/Grasshopper/Libraries"
  "${APPDATA:-}/Grasshopper/Libraries"
  "$HOME/.config/Grasshopper/Libraries"
)

TARGET=""
for c in "${CANDIDATES[@]}"; do
  [[ -z "$c" || "$c" == "/Grasshopper/Libraries" ]] && continue
  if [[ -d "$c" ]]; then TARGET="$c"; break; fi
done
if [[ -z "$TARGET" ]]; then
  # Linux / 无 Rhino：写入可复制的 Libraries 目录，并同时写一份到仓库旁
  TARGET="$HOME/.config/Grasshopper/Libraries"
  mkdir -p "$TARGET"
fi

DEST="$TARGET/SimpleML"
mkdir -p "$DEST/myML"
cp "$DIST/SimpleML.gha" "$DEST/SimpleML.gha"
cp "$DIST/SimpleML.gha" "$TARGET/SimpleML.gha"
rm -rf "$DEST/myML"
cp -R "$DIST/myML" "$DEST/myML"
cp -R "$DIST/docs" "$DEST/docs" 2>/dev/null || true
cp -R "$DIST/examples" "$DEST/examples" 2>/dev/null || true
cp "$DIST/README_INSTALL.txt" "$DEST/" 2>/dev/null || true

# Clean pycache then zip + sync to releases/
find "$DIST" -type d -name '__pycache__' -exec rm -rf {} + 2>/dev/null || true
find "$DIST" -name '*.pyc' -delete 2>/dev/null || true

ZIP="$ROOT/dist/SimpleML_Install.zip"
(cd "$ROOT/dist" && rm -f SimpleML_Install.zip && zip -qr SimpleML_Install.zip SimpleML)
echo "ZIP: $ZIP ($(wc -c < "$ZIP") bytes)"

REL_DIR="$ROOT/releases/SimpleML-1.2.0"
rm -rf "$REL_DIR"
mkdir -p "$ROOT/releases"
cp -a "$DIST/." "$REL_DIR/"
(cd "$ROOT/releases" && rm -f SimpleML-1.2.0.zip && zip -qr SimpleML-1.2.0.zip SimpleML-1.2.0)
echo "Release synced: $REL_DIR and releases/SimpleML-1.2.0.zip"

export SIMPLEML_PATH="$DEST/myML"
echo ""
echo "INSTALLED -> $DEST"
echo "SIMPLEML_PATH=$SIMPLEML_PATH"
echo "PYTHON_PATH=$PYTHON_BIN"
echo ""
echo "完成。若在本机有 Rhino：请重启 Rhino，运行「环境体检」。"
echo "若要把包拷到你的 Rhino 电脑：复制 $ZIP 或整个 $DIST"
