# -*- coding: utf-8 -*-
"""
Generate color-scheme OPTIONS only (does NOT overwrite plugin icons).
Follows docs/icon_style_proposals_gh.html Rhino 7–9 GH native craft:
  24x24, 2px margin, soft volume, same-hue edge, soft BR shadow, no letter badges.
"""

from __future__ import annotations

from pathlib import Path
from typing import Dict, List, Tuple

from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parents[1] / "docs" / "icon_palette_options"
SIZE = 24
MARGIN = 2

# Family keys used across schemes
FAMS = ("clf", "reg", "clus", "util", "help", "smart")

# ---- Palette options (light, mid, dark/edge) per family ----
# Soft GH plastic: muted, mid-value, not neon.

SCHEMES: Dict[str, Dict] = {
    "P1": {
        "title": "Classic GH Plastic",
        "blurb": "最接近原生 Params：灰蓝 / 暖灰 / 橄榄。任务靠图形，不靠高饱和。",
        "pal": {
            "clf":  ((168, 178, 188), (120, 132, 145), (72, 82, 92)),
            "reg":  ((186, 176, 160), (140, 128, 112), (86, 76, 62)),
            "clus": ((168, 180, 158), (118, 132, 108), (70, 82, 62)),
            "util": ((176, 176, 176), (128, 128, 128), (78, 78, 78)),
            "help": ((158, 172, 182), (110, 126, 138), (64, 76, 88)),
            "smart":((186, 180, 164), (140, 132, 114), (86, 80, 64)),
        },
    },
    "P2": {
        "title": "Soft Task Hue",
        "blurb": "低饱和黄/橙/绿区分分类·回归·聚类；工具灰蓝。语义清晰但仍像 GH。",
        "pal": {
            "clf":  ((214, 196, 120), (168, 148, 72), (100, 86, 36)),
            "reg":  ((212, 164, 122), (168, 112, 72), (100, 62, 36)),
            "clus": ((156, 186, 136), (108, 142, 90), (58, 82, 46)),
            "util": ((176, 178, 182), (126, 128, 134), (74, 76, 82)),
            "help": ((156, 170, 186), (108, 124, 144), (58, 72, 90)),
            "smart":((206, 186, 136), (158, 136, 86), (94, 78, 42)),
        },
    },
    "P3": {
        "title": "McNeel Type Colors",
        "blurb": "对齐 GH 类型色：黄=数据树、橙=曲面感、绿=数学、蓝=曲线/向量、灰=参数。",
        "pal": {
            "clf":  ((220, 196, 96), (176, 148, 48), (108, 88, 24)),   # data yellow
            "reg":  ((216, 148, 96), (176, 100, 52), (108, 56, 28)),   # surface orange
            "clus": ((128, 176, 88), (80, 132, 48), (40, 76, 24)),     # math green
            "util": ((192, 196, 200), (140, 144, 150), (84, 88, 94)),  # param gray
            "help": ((128, 176, 216), (72, 128, 176), (36, 72, 112)),  # curve blue
            "smart":((200, 184, 120), (152, 132, 72), (90, 76, 36)),
        },
    },
    "P4": {
        "title": "Ink Wash Sage",
        "blurb": "近单色灰绿水墨。最克制、色弱友好；任务几乎全靠剪影。",
        "pal": {
            "clf":  ((186, 188, 178), (136, 140, 128), (82, 86, 74)),
            "reg":  ((186, 188, 178), (136, 140, 128), (82, 86, 74)),
            "clus": ((176, 186, 168), (124, 138, 114), (72, 84, 64)),
            "util": ((180, 182, 176), (130, 132, 126), (78, 80, 74)),
            "help": ((170, 180, 178), (118, 130, 128), (68, 78, 76)),
            "smart":((186, 188, 178), (136, 140, 128), (82, 86, 74)),
        },
    },
    "P5": {
        "title": "Slate + Accent Chip",
        "blurb": "统一冷灰软底座，右上角小色标区分任务。家族感最强。",
        "pal": {
            # base is same; accent used as chip
            "clf":  ((168, 172, 178), (118, 122, 130), (70, 74, 82)),
            "reg":  ((168, 172, 178), (118, 122, 130), (70, 74, 82)),
            "clus": ((168, 172, 178), (118, 122, 130), (70, 74, 82)),
            "util": ((168, 172, 178), (118, 122, 130), (70, 74, 82)),
            "help": ((168, 172, 178), (118, 122, 130), (70, 74, 82)),
            "smart":((168, 172, 178), (118, 122, 130), (70, 74, 82)),
        },
        "chip": {
            "clf": (214, 186, 72),
            "reg": (204, 128, 72),
            "clus": (120, 164, 88),
            "util": (150, 154, 160),
            "help": (120, 148, 180),
            "smart": (196, 168, 88),
        },
    },
}


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


def soft_plate(draw, box, light, mid, edge, r=4):
    x0, y0, x1, y1 = box
    for i in range(5):
        t = i / 4
        c = lerp(light, mid, t * 0.85)
        inset = i * 0.35
        draw.rounded_rectangle(
            [x0 + inset, y0 + inset, x1 - inset, y1 - inset],
            radius=max(1, r - i * 0.25),
            fill=c + (255,),
        )
    draw.rounded_rectangle(box, radius=r, outline=edge + (255,), width=1)
    # top highlight
    draw.line([x0 + 3, y0 + 2, x1 - 3, y0 + 2], fill=lerp(light, (255, 255, 255), 0.35) + (180,), width=1)


def soft_blob(draw, box, light, mid, edge):
    x0, y0, x1, y1 = box
    for i in range(4):
        t = i / 3
        c = lerp(light, mid, t * 0.8)
        inset = min(i * 0.35, (x1 - x0) / 2 - 0.5)
        draw.ellipse([x0 + inset, y0 + inset, x1 - inset, y1 - inset], fill=c + (255,))
    draw.ellipse(box, outline=edge + (255,), width=1)


def add_shadow(fg: Image.Image) -> Image.Image:
    alpha = fg.split()[-1]
    sh = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    tmp = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    tmp.putalpha(alpha.point(lambda p: int(p * 0.28) if p else 0))
    sh.paste(tmp, (1, 1), tmp)
    sh = sh.filter(ImageFilter.GaussianBlur(1.2))
    return Image.alpha_composite(sh, fg)


def glyph_play(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    ink = lerp(light, (255, 255, 255), 0.55)
    d.polygon([(8, 7), (17, 12), (8, 17)], fill=ink + (255,), outline=edge + (255,))


def glyph_check(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    ink = lerp(light, (255, 255, 255), 0.55)
    d.line([(6, 12), (10, 16), (17, 7)], fill=ink + (255,), width=2)


def glyph_table(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    ink = lerp(light, (255, 255, 255), 0.5)
    d.rectangle([5, 5, 18, 9], fill=ink + (255,))
    d.line([(5, 12), (18, 12)], fill=edge + (200,), width=1)
    d.line([(5, 16), (18, 16)], fill=edge + (200,), width=1)
    d.line([(11, 9), (11, 18)], fill=edge + (200,), width=1)


def glyph_cluster(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    soft_blob(d, [5, 5, 12, 12], light, mid, edge)
    soft_blob(d, [13, 5, 20, 12], light, mid, edge)
    soft_blob(d, [9, 12, 16, 19], light, mid, edge)


def glyph_line(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    ink = lerp(light, (255, 255, 255), 0.55)
    d.line([(5, 16), (10, 11), (14, 13), (18, 7)], fill=ink + (255,), width=2)


def glyph_gear(d, light, mid, edge):
    soft_plate(d, [MARGIN, MARGIN, SIZE - MARGIN - 1, SIZE - MARGIN - 1], light, mid, edge)
    soft_blob(d, [7, 7, 16, 16], light, mid, edge)
    soft_blob(d, [10, 10, 13, 13], mid, edge, edge)


GLYPHS = [
    ("clf", glyph_play, "Predict Clf"),
    ("reg", glyph_line, "Eval Reg"),
    ("clus", glyph_cluster, "Cluster"),
    ("util", glyph_table, "Dataset"),
    ("help", glyph_check, "Health"),
    ("smart", glyph_gear, "Smart"),
]


def render_icon(scheme_id: str, fam: str, drawer) -> Image.Image:
    meta = SCHEMES[scheme_id]
    light, mid, edge = meta["pal"][fam]
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    drawer(d, light, mid, edge)
    if scheme_id == "P5" and "chip" in meta:
        chip = meta["chip"][fam]
        d.rounded_rectangle([16, 3, 21, 8], radius=1, fill=chip + (255,), outline=edge + (255,))
    return add_shadow(im)


def swatch_row(scheme_id: str, cell=36) -> Image.Image:
    meta = SCHEMES[scheme_id]
    n = len(FAMS)
    row = Image.new("RGBA", (n * cell, cell), (0, 0, 0, 0))
    for i, fam in enumerate(FAMS):
        light, mid, edge = meta["pal"][fam]
        tile = Image.new("RGBA", (cell, cell), (45, 45, 45, 255))
        td = ImageDraw.Draw(tile)
        td.rounded_rectangle([4, 4, cell - 5, cell - 5], radius=6, fill=mid + (255,), outline=edge + (255,))
        if scheme_id == "P5":
            chip = meta["chip"][fam]
            td.rounded_rectangle([cell - 14, 6, cell - 7, 13], radius=1, fill=chip + (255,))
        row.paste(tile, (i * cell, 0))
    return row


def contact_sheet(scheme_id: str) -> Image.Image:
    cols = len(GLYPHS)
    cell = 40
    header = 28
    sheet = Image.new("RGBA", (cols * cell, cell + header), (42, 42, 42, 255))
    d = ImageDraw.Draw(sheet)
    d.text((8, 6), f"{scheme_id}  {SCHEMES[scheme_id]['title']}", fill=(230, 230, 230, 255))
    for i, (fam, drawer, _label) in enumerate(GLYPHS):
        ico = render_icon(scheme_id, fam, drawer)
        big = ico.resize((32, 32), Image.Resampling.NEAREST)
        sheet.paste(big, (i * cell + 4, header + 4), big)
    return sheet


def board_all() -> Image.Image:
    sheets = [contact_sheet(sid) for sid in SCHEMES]
    swatches = [swatch_row(sid) for sid in SCHEMES]
    gap = 10
    w = max(s.width for s in sheets) + 24
    h = sum(s.height + 36 + gap for s in sheets) + 40
    board = Image.new("RGBA", (w, h), (236, 234, 228, 255))
    d = ImageDraw.Draw(board)
    d.text((12, 10), "SimpleML palette options  (Rhino 7-9 GH native craft)  — choose P1-P5", fill=(60, 58, 52, 255))
    y = 36
    for sid, sheet, sw in zip(SCHEMES, sheets, swatches):
        d.text((12, y), SCHEMES[sid]["blurb"][:80], fill=(100, 96, 88, 255))
        y += 18
        board.paste(sw, (12, y), sw)
        y += sw.height + 4
        board.paste(sheet, (12, y), sheet)
        y += sheet.height + gap
    return board


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for sid in SCHEMES:
        contact_sheet(sid).save(OUT / f"{sid}_preview.png")
        swatch_row(sid).save(OUT / f"{sid}_swatches.png")
        # also dump the 6 sample icons
        for fam, drawer, label in GLYPHS:
            render_icon(sid, fam, drawer).save(OUT / f"{sid}_{fam}.png")
        print("wrote", sid)
    board = board_all()
    board_path = OUT.parent / "icon_palette_options_board.png"
    board.save(board_path)
    print("board", board_path)


if __name__ == "__main__":
    main()
