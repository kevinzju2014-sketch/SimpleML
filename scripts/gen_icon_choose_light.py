# -*- coding: utf-8 -*-
"""
Pixel-native 24x24 icons for CHOICE (does not overwrite plugin).

Design logic (user request):
  - Preview on LIGHT grey (Grasshopper UI), not dark grey
  - Draw FOR 24px: chunky solid shapes, integer pixels
  - NOT: high-res soft art then downscale (causes fragile dots)
  - Min feature size ~3px; avoid 1px speckles
"""

from __future__ import annotations

from pathlib import Path
from typing import Callable, Dict, List, Tuple

from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).resolve().parents[1] / "docs" / "icon_choose_light"
SIZE = 24

# Grasshopper-like light greys (lighter chrome, not dark toolbar)
GH_CANVAS = (210, 210, 210, 255)
GH_TOOLBAR = (240, 240, 240, 255)
GH_WELL = (226, 226, 226, 255)


def px(im: Image.Image, x: int, y: int, rgba):
    if 0 <= x < SIZE and 0 <= y < SIZE:
        im.putpixel((x, y), rgba)


def fill_rect(im, x0, y0, x1, y1, rgba):
    """Inclusive pixel rect — no AA."""
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            px[x, y] = rgba


def fill_ellipse(im, x0, y0, x1, y1, rgba):
    """Hard pixel ellipse (no anti-alias fringe)."""
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    cx = (x0 + x1) / 2.0
    cy = (y0 + y1) / 2.0
    rx = max(0.5, (x1 - x0) / 2.0)
    ry = max(0.5, (y1 - y0) / 2.0)
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            dx = (x - cx) / rx
            dy = (y - cy) / ry
            if dx * dx + dy * dy <= 1.02:
                px[x, y] = rgba


def stroke_ellipse(im, x0, y0, x1, y1, rgba, thick=1):
    """Hard ring outline."""
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    cx = (x0 + x1) / 2.0
    cy = (y0 + y1) / 2.0
    rx = max(0.5, (x1 - x0) / 2.0)
    ry = max(0.5, (y1 - y0) / 2.0)
    inner = 1.0 - (thick + 0.35) / max(rx, ry)
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            dx = (x - cx) / rx
            dy = (y - cy) / ry
            r2 = dx * dx + dy * dy
            if inner * inner <= r2 <= 1.05:
                px[x, y] = rgba


def stroke_rect(im, x0, y0, x1, y1, rgba):
    fill_rect(im, x0, y0, x1, y0, rgba)
    fill_rect(im, x0, y1, x1, y1, rgba)
    fill_rect(im, x0, y0, x0, y1, rgba)
    fill_rect(im, x1, y0, x1, y1, rgba)


def hline(im, x0, x1, y, rgba, w=1):
    for i in range(w):
        fill_rect(im, x0, y + i, x1, y + i, rgba)


def line_thick(im, x0, y0, x1, y1, rgba, w=2):
    """Bresenham thick line with solid pixels."""
    x0, y0, x1, y1 = int(x0), int(y0), int(x1), int(y1)
    dx = abs(x1 - x0)
    dy = -abs(y1 - y0)
    sx = 1 if x0 < x1 else -1
    sy = 1 if y0 < y1 else -1
    err = dx + dy
    px = im.load()
    while True:
        for oy in range(-(w // 2), w - w // 2):
            for ox in range(-(w // 2), w - w // 2):
                xx, yy = x0 + ox, y0 + oy
                if 0 <= xx < SIZE and 0 <= yy < SIZE:
                    px[xx, yy] = rgba
        if x0 == x1 and y0 == y1:
            break
        e2 = 2 * err
        if e2 >= dy:
            err += dy
            x0 += sx
        if e2 <= dx:
            err += dx
            y0 += sy


def soft_shadow(im: Image.Image) -> Image.Image:
    """Very light 1px BR shadow; only under fully opaque pixels."""
    out = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    src = im.load()
    dst = out.load()
    for y in range(SIZE):
        for x in range(SIZE):
            r, g, b, a = src[x, y]
            if a < 250:
                continue
            sx, sy = x + 1, y + 1
            if 0 <= sx < SIZE and 0 <= sy < SIZE and dst[sx, sy][3] < 20:
                dst[sx, sy] = (0, 0, 0, 40)
    return Image.alpha_composite(out, im)


# ---- Chunky metaphors (min ~3–4 px features) ----

def m_data(im, pal):
    hi, mid, lo, edge, acc, _ = pal
    fill_rect(im, 4, 5, 19, 18, mid + (255,))
    fill_rect(im, 4, 5, 19, 8, acc + (255,))
    stroke_rect(im, 4, 5, 19, 18, edge + (255,))
    hline(im, 4, 19, 12, edge + (255,))
    hline(im, 4, 19, 15, edge + (255,))
    fill_rect(im, 11, 8, 11, 18, edge + (255,))
    fill_rect(im, 5, 6, 18, 6, hi + (255,))


def m_train(im, pal):
    """Solid plus (like GH math) — no thin spokes / fragile teeth."""
    hi, mid, lo, edge, acc, _ = pal
    fill_rect(im, 4, 9, 19, 14, mid + (255,))
    fill_rect(im, 9, 4, 14, 19, mid + (255,))
    stroke_rect(im, 4, 9, 19, 14, edge + (255,))
    stroke_rect(im, 9, 4, 14, 19, edge + (255,))
    fill_rect(im, 10, 10, 13, 13, lo + (255,))
    fill_rect(im, 5, 10, 8, 11, hi + (255,))
    fill_rect(im, 10, 5, 11, 8, hi + (255,))


def m_predict(im, pal):
    hi, mid, lo, edge, _, _ = pal
    # rasterize triangle by scan fill (hard pixels)
    px = im.load()
    # three faces via polygon scan
    def fill_poly(pts, rgba):
        ys = [p[1] for p in pts]
        for y in range(max(0, min(ys)), min(SIZE, max(ys) + 1)):
            xs = []
            for i in range(len(pts)):
                x0, y0 = pts[i]
                x1, y1 = pts[(i + 1) % len(pts)]
                if y0 == y1:
                    continue
                if min(y0, y1) <= y < max(y0, y1) or y == max(y0, y1) and y == max(ys):
                    t = (y - y0) / (y1 - y0)
                    xs.append(x0 + t * (x1 - x0))
            if len(xs) < 2:
                continue
            xs.sort()
            for x in range(max(0, int(xs[0] + 0.5)), min(SIZE, int(xs[-1] + 0.5) + 1)):
                px[x, y] = rgba
    fill_poly([(5, 4), (19, 12), (5, 19)], mid + (255,))
    fill_poly([(5, 4), (19, 12), (7, 12)], hi + (255,))
    fill_poly([(5, 19), (19, 12), (7, 12)], lo + (255,))
    # outline points
    line_thick(im, 5, 4, 19, 12, edge + (255,), 1)
    line_thick(im, 19, 12, 5, 19, edge + (255,), 1)
    line_thick(im, 5, 19, 5, 4, edge + (255,), 1)


def m_eval(im, pal):
    hi, mid, lo, edge, _, _ = pal
    fill_ellipse(im, 3, 3, 20, 20, mid + (255,))
    stroke_ellipse(im, 3, 3, 20, 20, edge + (255,), thick=1)
    fill_ellipse(im, 5, 5, 10, 9, hi + (255,))
    line_thick(im, 7, 12, 11, 16, (25, 25, 25, 255), 3)
    line_thick(im, 11, 16, 17, 8, (25, 25, 25, 255), 3)


def m_cluster(im, pal):
    hi, mid, lo, edge, acc, ac2 = pal
    fill_ellipse(im, 2, 2, 12, 12, mid + (255,))
    stroke_ellipse(im, 2, 2, 12, 12, edge + (255,), thick=2)
    fill_rect(im, 4, 4, 7, 6, hi + (255,))

    fill_ellipse(im, 12, 2, 22, 12, acc + (255,))
    stroke_ellipse(im, 12, 2, 22, 12, edge + (255,), thick=2)
    fill_rect(im, 14, 4, 17, 6, hi + (255,))

    fill_ellipse(im, 7, 11, 17, 21, ac2 + (255,))
    stroke_ellipse(im, 7, 11, 17, 21, edge + (255,), thick=2)
    fill_rect(im, 9, 13, 12, 15, hi + (255,))


def m_split(im, pal):
    hi, mid, lo, edge, acc, _ = pal
    fill_rect(im, 3, 5, 10, 18, mid + (255,))
    fill_rect(im, 13, 5, 20, 18, acc + (255,))
    stroke_rect(im, 3, 5, 10, 18, edge + (255,))
    stroke_rect(im, 13, 5, 20, 18, edge + (255,))
    fill_rect(im, 4, 6, 9, 7, hi + (255,))
    fill_rect(im, 14, 6, 19, 7, hi + (255,))


def m_line(im, pal):
    hi, mid, lo, edge, acc, _ = pal
    fill_rect(im, 3, 4, 20, 19, mid + (255,))
    stroke_rect(im, 3, 4, 20, 19, edge + (255,))
    for i, h in enumerate((6, 9, 13)):
        x0 = 5 + i * 5
        fill_rect(im, x0, 18 - h, x0 + 3, 18, (acc if i == 2 else lo) + (255,))
    line_thick(im, 5, 15, 10, 11, hi + (255,), 2)
    line_thick(im, 10, 11, 15, 12, hi + (255,), 2)
    line_thick(im, 15, 12, 19, 7, hi + (255,), 2)


def m_smart(im, pal):
    hi, mid, lo, edge, acc, _ = pal
    fill_ellipse(im, 4, 4, 19, 19, mid + (255,))
    stroke_ellipse(im, 4, 4, 19, 19, edge + (255,))
    fill_ellipse(im, 6, 6, 11, 10, hi + (255,))
    fill_rect(im, 18, 3, 20, 8, acc + (255,))
    fill_rect(im, 16, 5, 22, 7, acc + (255,))
    stroke_rect(im, 16, 3, 22, 8, edge + (255,))


KEYS = [
    ("data", m_data, "Data"),
    ("train", m_train, "Train"),
    ("predict", m_predict, "Predict"),
    ("eval", m_eval, "Evaluate"),
    ("cluster", m_cluster, "Cluster"),
    ("split", m_split, "Split"),
    ("line", m_line, "Regress"),
    ("smart", m_smart, "Smart"),
]

# palettes: hi, mid, lo, edge, accent, accent2
VERSIONS: Dict[str, Dict] = {
    "L1": {
        "title": "Official Vivid · Light Grey",
        "note": "官方鲜艳立体块，浅灰底预览。像素原生，无碎点。",
        "C": {
            "data":    ((245, 245, 245), (200, 200, 200), (140, 140, 140), (90, 90, 90), (70, 160, 50), (50, 120, 35)),
            "train":   ((170, 220, 100), (90, 170, 45), (50, 110, 25), (35, 75, 15), (255, 220, 80), (200, 160, 40)),
            "predict": ((160, 210, 255), (60, 140, 220), (30, 90, 160), (20, 55, 110), (255, 255, 255), (200, 200, 200)),
            "eval":    ((180, 230, 90), (100, 185, 45), (55, 120, 25), (35, 80, 15), (30, 30, 30), (30, 30, 30)),
            "cluster": ((150, 210, 95), (80, 165, 50), (45, 110, 30), (30, 75, 18), (240, 160, 50), (70, 150, 220)),
            "split":   ((235, 235, 235), (190, 190, 190), (135, 135, 135), (90, 90, 90), (80, 175, 55), (55, 130, 40)),
            "line":    ((255, 190, 110), (230, 130, 55), (160, 80, 30), (100, 50, 18), (255, 230, 120), (200, 100, 40)),
            "smart":   ((255, 230, 130), (90, 175, 230), (40, 110, 170), (25, 70, 120), (255, 200, 60), (80, 200, 120)),
        },
    },
    "L2": {
        "title": "Official Soft · Light Grey",
        "note": "同块面语言，饱和略降，浅灰底仍清晰。",
        "C": {
            "data":    ((240, 240, 240), (195, 195, 195), (140, 140, 140), (95, 95, 95), (95, 155, 80), (70, 120, 55)),
            "train":   ((175, 200, 140), (115, 155, 85), (75, 110, 55), (50, 75, 35), (230, 200, 110), (175, 150, 60)),
            "predict": ((165, 195, 225), (100, 150, 195), (60, 105, 150), (40, 70, 105), (245, 245, 245), (190, 190, 190)),
            "eval":    ((180, 210, 140), (120, 170, 85), (80, 120, 55), (55, 85, 35), (40, 40, 40), (40, 40, 40)),
            "cluster": ((160, 195, 140), (105, 155, 90), (70, 110, 60), (45, 75, 40), (225, 155, 90), (100, 150, 200)),
            "split":   ((230, 230, 230), (185, 185, 185), (135, 135, 135), (95, 95, 95), (110, 165, 95), (80, 130, 70)),
            "line":    ((235, 175, 130), (200, 130, 80), (145, 85, 50), (95, 55, 30), (235, 205, 140), (170, 100, 55)),
            "smart":   ((235, 215, 155), (125, 170, 200), (80, 120, 155), (55, 85, 110), (240, 200, 90), (110, 175, 140)),
        },
    },
    "L3": {
        "title": "Max Clarity · Light Grey",
        "note": "浅灰底专用：更深描边、更亮高光、块面更大。",
        "C": {
            "data":    ((255, 255, 255), (210, 210, 210), (145, 145, 145), (70, 70, 70), (50, 150, 40), (35, 110, 25)),
            "train":   ((190, 240, 110), (95, 185, 40), (50, 120, 18), (25, 70, 8), (255, 235, 90), (210, 170, 35)),
            "predict": ((180, 225, 255), (45, 145, 235), (20, 95, 170), (10, 55, 115), (255, 255, 255), (220, 220, 220)),
            "eval":    ((195, 245, 95), (105, 200, 40), (55, 130, 18), (30, 80, 8), (20, 20, 20), (20, 20, 20)),
            "cluster": ((165, 230, 100), (75, 180, 40), (40, 120, 20), (22, 75, 10), (250, 165, 35), (55, 150, 240)),
            "split":   ((255, 255, 255), (205, 205, 205), (140, 140, 140), (70, 70, 70), (60, 175, 45), (40, 130, 30)),
            "line":    ((255, 200, 100), (240, 135, 40), (165, 80, 18), (100, 45, 10), (255, 240, 140), (220, 100, 30)),
            "smart":   ((255, 240, 140), (70, 180, 245), (25, 110, 185), (12, 65, 125), (255, 200, 50), (70, 210, 110)),
        },
    },
    "L4": {
        "title": "Task Hue Blocks · Light Grey",
        "note": "黄分类 / 橙回归 / 绿聚类；块面任务色，浅灰底。",
        "C": {
            "data":    ((250, 250, 250), (205, 205, 205), (145, 145, 145), (90, 90, 90), (130, 130, 130), (100, 100, 100)),
            "train":   ((250, 220, 90), (220, 175, 40), (150, 115, 20), (95, 70, 12), (255, 245, 160), (200, 160, 40)),
            "predict": ((250, 220, 90), (220, 175, 40), (150, 115, 20), (95, 70, 12), (255, 255, 255), (200, 200, 200)),
            "eval":    ((250, 220, 90), (220, 175, 40), (150, 115, 20), (95, 70, 12), (30, 30, 30), (30, 30, 30)),
            "cluster": ((130, 210, 85), (80, 170, 45), (45, 115, 25), (28, 75, 15), (245, 165, 45), (70, 150, 225)),
            "split":   ((245, 245, 245), (200, 200, 200), (145, 145, 145), (90, 90, 90), (90, 175, 55), (60, 130, 40)),
            "line":    ((255, 170, 80), (235, 120, 40), (165, 70, 18), (105, 45, 10), (255, 220, 120), (200, 90, 30)),
            "smart":   ((250, 220, 110), (210, 165, 50), (140, 105, 25), (90, 65, 15), (255, 255, 210), (100, 185, 70)),
        },
    },
    "L5": {
        "title": "Gray Blocks + Accent · Light Grey",
        "note": "灰块主体 + 大块彩色点缀（非碎点），家族统一。",
        "C": {
            "data":    ((250, 250, 250), (200, 200, 200), (145, 145, 145), (90, 90, 90), (65, 165, 50), (45, 125, 35)),
            "train":   ((235, 235, 235), (180, 180, 180), (125, 125, 125), (80, 80, 80), (75, 170, 50), (255, 200, 55)),
            "predict": ((235, 235, 235), (180, 180, 180), (125, 125, 125), (80, 80, 80), (50, 140, 225), (50, 140, 225)),
            "eval":    ((225, 235, 225), (165, 185, 160), (110, 135, 105), (70, 90, 65), (70, 165, 50), (25, 25, 25)),
            "cluster": ((235, 235, 235), (180, 180, 180), (125, 125, 125), (80, 80, 80), (230, 140, 45), (60, 140, 220)),
            "split":   ((245, 245, 245), (195, 195, 195), (140, 140, 140), (90, 90, 90), (70, 170, 50), (50, 130, 35)),
            "line":    ((245, 245, 245), (195, 195, 195), (140, 140, 140), (90, 90, 90), (230, 125, 45), (240, 190, 80)),
            "smart":   ((245, 245, 245), (195, 195, 195), (140, 140, 140), (90, 90, 90), (70, 165, 230), (240, 195, 70)),
        },
    },
}


def render(vid: str, key: str, drawer: Callable) -> Image.Image:
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    drawer(im, VERSIONS[vid]["C"][key])
    return soft_shadow(im)


def on_bg(ico: Image.Image, bg) -> Image.Image:
    base = Image.new("RGBA", (SIZE, SIZE), bg)
    return Image.alpha_composite(base, ico)


def font(size=14):
    for name in ("segoeui.ttf", "arial.ttf", "msyh.ttc"):
        try:
            return ImageFont.truetype(name, size)
        except Exception:
            continue
    return ImageFont.load_default()


def version_card(vid: str, scale=3) -> Image.Image:
    meta = VERSIONS[vid]
    n = len(KEYS)
    cell = SIZE * scale + 18
    head = 58
    pad = 14
    w = pad * 2 + n * cell
    h = pad + head + cell + 30
    card = Image.new("RGBA", (w, h), GH_TOOLBAR)
    d = ImageDraw.Draw(card)
    d.rectangle([0, 0, w, 40], fill=(245, 245, 245, 255))
    d.text((pad, 8), f"{vid}  ·  {meta['title']}", font=font(15), fill=(40, 40, 40, 255))
    d.text((pad, 42), meta["note"], font=font(11), fill=(90, 90, 90, 255))

    y0 = head + 4
    for i, (key, drawer, name) in enumerate(KEYS):
        ico = render(vid, key, drawer)
        big = ico.resize((SIZE * scale, SIZE * scale), Image.Resampling.NEAREST)
        x = pad + i * cell
        d.rounded_rectangle([x, y0, x + cell - 10, y0 + cell - 10], radius=5, fill=GH_WELL)
        ox = x + (cell - 10 - big.width) // 2
        oy = y0 + (cell - 10 - big.height) // 2
        # paste on light well already; icon has transparency
        card.paste(big, (ox, oy), big)
        tw = d.textlength(name, font=font(11)) if hasattr(d, "textlength") else len(name) * 6
        d.text((x + (cell - 10 - tw) / 2, y0 + cell - 6), name, font=font(11), fill=(70, 70, 70, 255))
    return card


def strip_24(vid: str) -> Image.Image:
    n = len(KEYS)
    pad = 10
    cell = 30
    w = pad * 2 + n * cell + 80
    h = 44
    im = Image.new("RGBA", (w, h), GH_CANVAS)
    d = ImageDraw.Draw(im)
    d.text((pad, 4), f"{vid} @24px on canvas grey", font=font(10), fill=(70, 70, 70, 255))
    for i, (key, drawer, _) in enumerate(KEYS):
        ico = render(vid, key, drawer)
        # composite onto canvas grey cell
        cell_im = on_bg(ico, GH_CANVAS)
        im.paste(cell_im, (pad + 90 + i * cell, 14))
    return im


def master_board() -> Image.Image:
    cards = [version_card(v) for v in VERSIONS]
    strips = [strip_24(v) for v in VERSIONS]
    gap = 12
    w = max(c.width for c in cards) + 24
    h = 80 + sum(c.height + s.height + gap for c, s in zip(cards, strips))
    board = Image.new("RGBA", (w, h), (240, 240, 240, 255))
    d = ImageDraw.Draw(board)
    d.text((12, 10), "LIGHT grey preview (Grasshopper-like)  |  pixel-native 24px  |  no fragile dots", font=font(16), fill=(30, 30, 30, 255))
    d.text((12, 36), "Logic: draw chunky shapes at 24px (not HD then shrink). Pick L1–L5.", font=font(12), fill=(90, 90, 90, 255))
    y = 70
    for card, strip in zip(cards, strips):
        board.paste(card, (12, y), card)
        y += card.height + 4
        board.paste(strip, (12, y), strip)
        y += strip.height + gap
    return board


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for vid in VERSIONS:
        for key, drawer, _ in KEYS:
            ico = render(vid, key, drawer)
            ico.save(OUT / f"{vid}_{key}.png")
            on_bg(ico, GH_TOOLBAR).save(OUT / f"{vid}_{key}_toolbar.png")
            on_bg(ico, GH_CANVAS).save(OUT / f"{vid}_{key}_canvas.png")
        version_card(vid).save(OUT / f"{vid}_card.png")
        strip_24(vid).save(OUT / f"{vid}_24px.png")
        print("wrote", vid)
    board = master_board()
    bp = OUT.parent / "icon_choose_light_board.png"
    board.save(bp)
    print("board", bp, board.size)


if __name__ == "__main__":
    main()
