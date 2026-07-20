# -*- coding: utf-8 -*-
"""
Official Grasshopper craft proposals (does NOT overwrite plugin icons).

Key lesson from docs/gh_icon_samples:
  - Transparent background (NO full-bleed colored tile)
  - Soft-plastic 3D OBJECTS with top-left light
  - Clear real metaphors; high-contrast silhouette
  - Same-hue darker edges; soft BR shadow
  - 24x24, ~2px margin
"""

from __future__ import annotations

import math
from pathlib import Path
from typing import Callable, Dict, Tuple

from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parents[1] / "docs" / "icon_official_craft"
SIZE = 24


def lerp(a, b, t):
    n = min(len(a), len(b))
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(n)) + a[n:]


def shadow(fg: Image.Image) -> Image.Image:
    a = fg.split()[-1]
    tmp = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    tmp.putalpha(a.point(lambda p: int(p * 0.30) if p else 0))
    sh = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    sh.paste(tmp, (1, 1), tmp)
    sh = sh.filter(ImageFilter.GaussianBlur(1.15))
    return Image.alpha_composite(sh, fg)


def ball(draw, box, hi, mid, lo, edge=None):
    x0, y0, x1, y1 = box
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    rx, ry = (x1 - x0) / 2, (y1 - y0) / 2
    for i in range(6):
        t = i / 5
        c = lerp(hi, lo, t)
        inset = t * min(rx, ry) * 0.55
        draw.ellipse([x0 + inset, y0 + inset, x1 - inset, y1 - inset], fill=c + (255,))
    # specular
    sx0, sy0 = cx - rx * 0.45, cy - ry * 0.55
    draw.ellipse([sx0, sy0, sx0 + rx * 0.45, sy0 + ry * 0.35], fill=(255, 255, 255, 90))
    if edge:
        draw.ellipse(box, outline=edge + (220,), width=1)


def cube(draw, ox, oy, s, top, left, right, edge):
    # isometric-ish soft cube
    # top diamond
    t = [
        (ox + s // 2, oy),
        (ox + s, oy + s // 4),
        (ox + s // 2, oy + s // 2),
        (ox, oy + s // 4),
    ]
    l = [
        (ox, oy + s // 4),
        (ox + s // 2, oy + s // 2),
        (ox + s // 2, oy + s),
        (ox, oy + s * 3 // 4),
    ]
    r = [
        (ox + s // 2, oy + s // 2),
        (ox + s, oy + s // 4),
        (ox + s, oy + s * 3 // 4),
        (ox + s // 2, oy + s),
    ]
    draw.polygon(t, fill=top + (255,), outline=edge + (255,))
    draw.polygon(l, fill=left + (255,), outline=edge + (255,))
    draw.polygon(r, fill=right + (255,), outline=edge + (255,))


def plate_doc(draw, x0, y0, x1, y1, hi, mid, lo, edge):
    # soft document with folded corner
    draw.rounded_rectangle([x0, y0, x1, y1], radius=2, fill=mid + (255,), outline=edge + (255,))
    draw.polygon([(x1 - 6, y0), (x1, y0), (x1, y0 + 6)], fill=lo + (255,))
    draw.line([(x0 + 3, y0 + 8), (x1 - 4, y0 + 8)], fill=hi + (255,), width=1)
    draw.line([(x0 + 3, y0 + 11), (x1 - 4, y0 + 11)], fill=hi + (255,), width=1)
    draw.line([(x0 + 3, y0 + 14), (x1 - 7, y0 + 14)], fill=hi + (255,), width=1)


# ---- Metaphors (object-based, like official GH) ----

def m_dataset(d, C):
    # gray data slab + green accent bar (like GH table feel)
    hi, mid, lo, edge, acc, ac2 = C
    draw = d
    draw.rounded_rectangle([4, 5, 20, 19], radius=2, fill=mid + (255,), outline=edge + (255,))
    draw.rectangle([4, 5, 20, 9], fill=acc + (255,))
    draw.line([(4, 12), (20, 12)], fill=edge + (180,), width=1)
    draw.line([(4, 15), (20, 15)], fill=edge + (180,), width=1)
    draw.line([(12, 9), (12, 19)], fill=edge + (180,), width=1)


def m_train(d, C):
    # soft gear = center ball + tooth nubs
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [5, 5, 19, 19], hi, mid, lo, edge)
    for ang in range(0, 360, 60):
        rad = math.radians(ang)
        cx, cy = 12 + math.cos(rad) * 8, 12 + math.sin(rad) * 8
        d.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=mid + (255,), outline=edge + (200,))
    ball(d, [9, 9, 15, 15], lo, lo, edge, edge)


def m_predict(d, C):
    # 3D wedge / play object (not flat on tile)
    hi, mid, lo, edge, acc, ac2 = C
    d.polygon([(6, 5), (19, 12), (6, 19)], fill=mid + (255,), outline=edge + (255,))
    d.polygon([(6, 5), (19, 12), (8, 12)], fill=hi + (255,))
    d.polygon([(6, 19), (19, 12), (8, 12)], fill=lo + (255,))


def m_evaluate(d, C):
    # official-like: soft circle badge + check (cf. green ? help)
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [3, 3, 21, 21], hi, mid, lo, edge)
    d.line([(7, 12), (11, 16), (17, 8)], fill=(20, 20, 20, 255), width=2)


def m_cluster(d, C):
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [3, 3, 13, 13], hi, mid, lo, edge)
    ball(d, [11, 3, 21, 13], acc, lerp(acc, lo, 0.4), lerp(acc, (40, 40, 40), 0.5), edge)
    ball(d, [7, 11, 17, 21], ac2, lerp(ac2, lo, 0.4), lerp(ac2, (40, 40, 40), 0.5), edge)


def m_classify(d, C):
    # tagged spheres (class labels)
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [4, 8, 14, 18], hi, mid, lo, edge)
    ball(d, [12, 4, 21, 13], acc, lerp(acc, (255, 255, 200), 0.2), lerp(acc, lo, 0.5), edge)
    # small badge
    d.rounded_rectangle([3, 3, 9, 9], radius=1, fill=(250, 250, 250, 240), outline=edge + (255,))
    d.line([(4, 6), (8, 6)], fill=edge + (255,), width=1)


def m_regress(d, C):
    # rising bars as soft blocks
    hi, mid, lo, edge, acc, ac2 = C
    for i, h in enumerate((7, 11, 15)):
        x0 = 5 + i * 5
        y0 = 19 - h
        d.rounded_rectangle([x0, y0, x0 + 4, 19], radius=1, fill=(mid if i < 2 else acc) + (255,), outline=edge + (255,))
        d.line([x0 + 1, y0 + 1, x0 + 3, y0 + 1], fill=hi + (200,), width=1)


def m_smart(d, C):
    # glossy sphere + spark (like GH iridescent ball, simplified)
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [4, 4, 20, 20], hi, mid, lo, edge)
    d.line([(17, 4), (21, 8)], fill=acc + (255,), width=2)
    d.line([(19, 3), (19, 9)], fill=acc + (255,), width=2)


def m_help(d, C):
    # match official help: saturated circle + dark mark
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [3, 3, 21, 21], hi, mid, lo, edge)
    d.ellipse([10, 6, 14, 10], fill=(20, 20, 20, 255))
    d.rectangle([10, 11, 14, 17], fill=(20, 20, 20, 255))


METAPHORS = [
    ("dataset", m_dataset),
    ("train", m_train),
    ("predict", m_predict),
    ("evaluate", m_evaluate),
    ("cluster", m_cluster),
    ("classify", m_classify),
    ("regress", m_regress),
    ("smart", m_smart),
]

# Colors: hi, mid, lo, edge, accent, accent2
DIRECTIONS: Dict[str, Dict] = {
    "O1": {
        "title": "Official Vivid (recommended)",
        "blurb": "对照官方样本：透明底 + 立体物件 + 鲜明绿/橙/蓝。剪影清晰，不靠色砖。",
        "colors": {
            "dataset": ((220, 220, 220), (180, 180, 180), (120, 120, 120), (70, 70, 70), (80, 170, 60), (60, 130, 40)),
            "train":   ((140, 200, 90), (70, 150, 40), (40, 90, 25), (30, 60, 15), (255, 220, 80), (200, 160, 40)),
            "predict": ((120, 190, 255), (50, 130, 210), (25, 80, 140), (15, 50, 90), (255, 255, 255), (200, 200, 200)),
            "evaluate":((160, 220, 80), (90, 180, 40), (50, 110, 25), (30, 70, 15), (20, 20, 20), (20, 20, 20)),
            "cluster": ((140, 210, 100), (70, 160, 50), (40, 100, 30), (25, 65, 18), (255, 170, 60), (90, 160, 230)),
            "classify":((255, 210, 80), (220, 170, 40), (150, 110, 25), (90, 65, 15), (80, 160, 230), (220, 100, 70)),
            "regress": ((255, 170, 90), (220, 120, 50), (150, 70, 30), (90, 40, 15), (255, 210, 100), (180, 90, 40)),
            "smart":   ((255, 230, 120), (100, 180, 220), (50, 100, 160), (30, 60, 100), (255, 255, 200), (80, 200, 120)),
        },
    },
    "O2": {
        "title": "Official Soft",
        "blurb": "同一套立体物件语言，饱和度略降，更耐看，仍比色砖清晰。",
        "colors": {
            "dataset": ((210, 210, 210), (165, 165, 165), (110, 110, 110), (70, 70, 70), (100, 160, 90), (70, 120, 60)),
            "train":   ((160, 190, 130), (100, 145, 75), (60, 95, 45), (40, 65, 30), (230, 200, 100), (180, 150, 60)),
            "predict": ((150, 185, 220), (90, 140, 185), (50, 95, 135), (30, 60, 90), (240, 240, 240), (190, 190, 190)),
            "evaluate":((170, 200, 120), (110, 160, 70), (70, 110, 45), (45, 75, 30), (30, 30, 30), (30, 30, 30)),
            "cluster": ((150, 185, 130), (95, 145, 80), (55, 95, 50), (35, 65, 30), (230, 160, 90), (100, 150, 200)),
            "classify":((230, 200, 110), (190, 155, 70), (130, 100, 40), (80, 60, 25), (90, 145, 200), (200, 110, 80)),
            "regress": ((230, 165, 110), (190, 120, 70), (130, 75, 40), (80, 45, 25), (235, 195, 120), (160, 95, 55)),
            "smart":   ((230, 210, 140), (120, 165, 195), (70, 110, 145), (45, 70, 95), (245, 245, 210), (100, 175, 130)),
        },
    },
    "O3": {
        "title": "Task Object Colors",
        "blurb": "仍是立体物件，但分类偏黄、回归偏橙、聚类偏绿——任务色在物体上，不是色砖底板。",
        "colors": {
            "dataset": ((215, 215, 215), (170, 170, 170), (115, 115, 115), (70, 70, 70), (120, 120, 120), (90, 90, 90)),
            "train":   ((230, 200, 90), (190, 155, 45), (120, 95, 25), (75, 55, 15), (255, 240, 160), (200, 160, 40)),
            "predict": ((230, 200, 90), (190, 155, 45), (120, 95, 25), (75, 55, 15), (255, 255, 255), (200, 200, 200)),
            "evaluate":((230, 200, 90), (190, 155, 45), (120, 95, 25), (75, 55, 15), (20, 20, 20), (20, 20, 20)),
            "cluster": ((130, 190, 90), (80, 150, 50), (45, 95, 30), (28, 60, 18), (255, 180, 70), (90, 150, 220)),
            "classify":((240, 205, 80), (210, 165, 40), (140, 105, 20), (85, 60, 12), (70, 150, 220), (220, 90, 60)),
            "regress": ((240, 155, 80), (210, 110, 45), (140, 65, 25), (85, 40, 12), (255, 210, 120), (180, 90, 40)),
            "smart":   ((230, 200, 110), (180, 150, 60), (110, 90, 35), (70, 55, 20), (255, 255, 200), (120, 180, 90)),
        },
    },
    "O4": {
        "title": "Gray Object + Accent",
        "blurb": "主体统一灰塑料（像官方灰控件），功能靠彩色小件/角标表达——家族感强。",
        "colors": {
            "dataset": ((220, 220, 220), (175, 175, 175), (120, 120, 120), (70, 70, 70), (70, 160, 70), (50, 120, 50)),
            "train":   ((210, 210, 210), (160, 160, 160), (110, 110, 110), (65, 65, 65), (80, 170, 60), (255, 200, 60)),
            "predict": ((210, 210, 210), (160, 160, 160), (110, 110, 110), (65, 65, 65), (50, 140, 220), (50, 140, 220)),
            "evaluate":((200, 210, 200), (150, 170, 145), (95, 120, 90), (55, 75, 50), (70, 160, 50), (20, 20, 20)),
            "cluster": ((210, 210, 210), (160, 160, 160), (110, 110, 110), (65, 65, 65), (230, 140, 50), (70, 140, 210)),
            "classify":((215, 215, 215), (170, 170, 170), (115, 115, 115), (70, 70, 70), (230, 180, 50), (60, 140, 210)),
            "regress": ((215, 215, 215), (170, 170, 170), (115, 115, 115), (70, 70, 70), (230, 120, 50), (230, 180, 80)),
            "smart":   ((215, 215, 215), (165, 165, 165), (110, 110, 110), (65, 65, 65), (90, 170, 220), (230, 190, 70)),
        },
    },
}


def render(dir_id: str, key: str, drawer: Callable) -> Image.Image:
    C = DIRECTIONS[dir_id]["colors"][key]
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    drawer(ImageDraw.Draw(im, "RGBA"), C)
    return shadow(im)


def preview_row(dir_id: str, bg=(43, 43, 43, 255), cell=44) -> Image.Image:
    n = len(METAPHORS)
    row = Image.new("RGBA", (n * cell, cell + 22), bg)
    d = ImageDraw.Draw(row)
    d.text((6, 3), f"{dir_id}  {DIRECTIONS[dir_id]['title']}", fill=(230, 230, 230, 255))
    for i, (key, drawer) in enumerate(METAPHORS):
        ico = render(dir_id, key, drawer)
        big = ico.resize((36, 36), Image.Resampling.NEAREST)
        row.paste(big, (i * cell + 4, 20), big)
    return row


def compare_board() -> Image.Image:
    # official samples strip + our O1-O4
    official = Path(__file__).resolve().parents[1] / "docs" / "gh_official_samples_3x.png"
    rows = [preview_row(did) for did in DIRECTIONS]
    w = max(r.width for r in rows)
    if official.exists():
        off = Image.open(official).convert("RGBA")
        # scale to width
        scale = w / off.width
        off = off.resize((w, int(off.height * scale)), Image.Resampling.NEAREST)
    else:
        off = Image.new("RGBA", (w, 80), (40, 40, 40, 255))

    gap = 12
    h = 70 + off.height + gap + sum(r.height + gap for r in rows)
    board = Image.new("RGBA", (w + 24, h), (236, 234, 228, 255))
    d = ImageDraw.Draw(board)
    d.text((12, 10), "WRONG before: colored tile + glyph.  RIGHT: official GH = transparent + 3D object.", fill=(50, 48, 42, 255))
    d.text((12, 32), "Top strip = official Grasshopper samples (3x). Below = new directions O1-O4.", fill=(90, 86, 78, 255))
    y = 55
    board.paste(off, (12, y), off)
    y += off.height + gap
    for r in rows:
        board.paste(r, (12, y), r)
        y += r.height + gap
    return board


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for did in DIRECTIONS:
        for key, drawer in METAPHORS:
            render(did, key, drawer).save(OUT / f"{did}_{key}.png")
        preview_row(did).save(OUT / f"{did}_row.png")
        print("wrote", did)
    board = compare_board()
    bp = OUT.parent / "icon_official_craft_board.png"
    board.save(bp)
    print("board", bp)


if __name__ == "__main__":
    main()
