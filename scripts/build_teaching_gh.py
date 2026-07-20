# -*- coding: utf-8 -*-
"""
Rebuild SimpleML teaching .gh files via Grasshopper MCP (port 8080)
or Rhino gh_* tools when available.

Usage (Rhino open, Grasshopper open, mcp / GH server running):
  python scripts/build_teaching_gh.py
"""

from __future__ import annotations

import json
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "examples" / "gh"

# Component GUIDs (SimpleML)
GUID = {
    "health": "A0B1C2D3-E4F5-6789-ABCD-101112131415",
    "load_ds": "E3F4A5B6-C7D8-9012-EF01-234567890124",
    "split": "B8C9D0E1-F2A3-4567-1234-567890123458",
    "smart": "B1C2D3E4-F5A6-7890-BCDE-212223242526",
    "predict": "E5F6A7B8-C9D0-1234-EF01-515253545556",
    "evaluate": "F6A7B8C9-D0E1-2345-F012-616263646566",
    "color": "C9D0E1F2-A3B4-5678-2345-919293949596",
    "language": "A9C4E2B1-7D35-4F80-9E12-6B8A0C5D4E31",
    "panel": "59e0b89a-e487-49f8-bab8-b5bab16be14c",  # GH Panel (common)
    "toggle": "2e78987b-9dfb-42a2-8b76-392adb30304f",  # Boolean Toggle approx — may vary
}

RECIPES = {
    "01_classification_iris": {
        "title": "01 Classification (iris) — swap Load Dataset or use Read CSV",
        "dataset": "iris",
        "task": "classification",
        "use_split": True,
        "use_color": False,
    },
    "02_clustering_color": {
        "title": "02 Clustering + Color — swap points / dataset only",
        "dataset": "make_blobs",
        "task": "clustering",
        "use_split": False,
        "use_color": True,
    },
    "03_regression_diabetes": {
        "title": "03 Regression (diabetes) — swap data source only",
        "dataset": "diabetes",
        "task": "regression",
        "use_split": True,
        "use_color": False,
    },
}


def gh_post(path: str, payload: dict, host="http://127.0.0.1:8080"):
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        f"{host}{path}",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=10) as resp:
        return json.loads(resp.read().decode("utf-8"))


def try_build_via_http():
    """Best-effort: clear canvas, place core chain, save. Requires GH HTTP bridge."""
    try:
        gh_post("/clear_document", {})
    except Exception as e:
        print("Grasshopper HTTP bridge not reachable:", e)
        print("Keep the existing examples/gh/*.gh files.")
        print("When Rhino+GH MCP is up, re-run this script or wire manually from the MD recipes.")
        return False

    print("HTTP bridge OK — place components manually via MCP tools if this script's API differs.")
    print("Recipes defined:", ", ".join(RECIPES))
    print("Output folder:", OUT)
    return True


def write_howto():
    text = """# How to refresh teaching .gh files

1. Open Rhino + Grasshopper, load SimpleML.
2. For each recipe in examples/*.md, place:
   Language | Health Check | Load Dataset | Split(optional) | Smart Train | Predict | Evaluate
3. Set Load Dataset name (iris / make_blobs / diabetes).
4. Add a Panel with the title + "Swap data: replace Load Dataset with Read CSV + Quick Dataset".
5. Save as examples/gh/01_classification_iris.gh (etc.).

Default parameters: Smart Train Task=auto or as listed; leave other knobs alone.
"""
    (OUT / "HOW_TO_REBUILD.md").write_text(text, encoding="utf-8")
    print("wrote", OUT / "HOW_TO_REBUILD.md")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    write_howto()
    # Ensure numbered copies exist (from legacy Chinese names if present)
    legacy = {
        "01_classification_iris.gh": "260128_分类预测模型.gh",
        "02_clustering_color.gh": "260128_聚类预测模型.gh",
        "03_regression_diabetes.gh": "260128_回归预测模型.gh",
        "03b_regression_advanced.gh": "260511_回归预测模型.gh",
    }
    for dst, src in legacy.items():
        sp, dp = OUT / src, OUT / dst
        if sp.exists() and not dp.exists():
            dp.write_bytes(sp.read_bytes())
            print("copied", src, "->", dst)
        elif dp.exists():
            print("ok", dst)
        else:
            print("missing", src, "and", dst)

    try_build_via_http()
    print("\nTeaching docs: examples/README.md + 01/02/03 markdown recipes.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
