# -*- coding: utf-8 -*-
"""
Icon versions for CHOICE only — previewed on GH-like grey toolbar background.
Does NOT overwrite plugin icons.
"""

from __future__ import annotations

import math
from pathlib import Path
from typing import Callable, Dict, List, Tuple

from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = Path(__file__).resolve().parents[1] / "docs" / "icon_choose"
SIZE = 24
# Grasshopper-ish toolbar / dark panel grey
GH_GREY = (55, 55, 55, 255)
GH_GREY_LIGHT = (72, 72, 72, 255)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(min(len(a), len(b))))


def shadow(fg: Image.Image) -> Image.Image:
    a = fg.split()[-1]
    tmp = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    tmp.putalpha(a.point(lambda p: int(p * 0.32) if p else 0))
    sh = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    sh.paste(tmp, (1, 1), tmp)
    sh = sh.filter(ImageFilter.GaussianBlur(1.1))
    return Image.alpha_composite(sh, fg)


def ball(draw, box, hi, mid, lo, edge=None):
    x0, y0, x1, y1 = box
    for i in range(6):
        t = i / 5
        c = lerp(hi, lo, t)
        inset = t * min(x1 - x0, y1 - y0) / 2 * 0.5
        draw.ellipse([x0 + inset, y0 + inset, x1 - inset, y1 - inset], fill=c + (255,))
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    rx = (x1 - x0) / 2
    draw.ellipse([cx - rx * 0.4, cy - rx * 0.55, cx + rx * 0.05, cy - rx * 0.15], fill=(255, 255, 255, 100))
    if edge:
        draw.ellipse(box, outline=edge + (230,), width=1)


# ---- metaphors ----

def m_data(d, C):
    hi, mid, lo, edge, acc, _ = C
    d.rounded_rectangle([4, 5, 20, 19], radius=2, fill=mid + (255,), outline=edge + (255,))
    d.rectangle([4, 5, 20, 9], fill=acc + (255,))
    for y in (12, 15):
        d.line([(5, y), (19, y)], fill=edge + (200,), width=1)
    d.line([(12, 9), (12, 19)], fill=edge + (200,), width=1)


def m_train(d, C):
    hi, mid, lo, edge, acc, _ = C
    ball(d, [5, 5, 19, 19], hi, mid, lo, edge)
    for ang in range(0, 360, 60):
        rad = math.radians(ang)
        cx = 12 + math.cos(rad) * 8
        cy = 12 + math.sin(rad) * 8
        d.ellipse([cx - 2.2, cy - 2.2, cx + 2.2, cy + 2.2], fill=mid + (255,), outline=edge + (200,))
    ball(d, [9, 9, 15, 15], lo, edge, edge, edge)


def m_predict(d, C):
    hi, mid, lo, edge, _, _ = C
    d.polygon([(6, 5), (19, 12), (6, 19)], fill=mid + (255,), outline=edge + (255,))
    d.polygon([(6, 5), (19, 12), (8, 12)], fill=hi + (255,))
    d.polygon([(6, 19), (19, 12), (8, 12)], fill=lo + (255,))


def m_eval(d, C):
    hi, mid, lo, edge, _, _ = C
    ball(d, [3, 3, 21, 21], hi, mid, lo, edge)
    d.line([(7, 12), (11, 16), (17, 8)], fill=(15, 15, 15, 255), width=2)


def m_cluster(d, C):
    hi, mid, lo, edge, acc, ac2 = C
    ball(d, [3, 3, 13, 13], hi, mid, lo, edge)
    ball(d, [11, 3, 21, 13], acc, lerp(acc, lo, 0.35), lerp(acc, (30, 30, 30), 0.45), edge)
    ball(d, [7, 11, 17, 21], ac2, lerp(ac2, lo, 0.35), lerp(ac2, (30, 30, 30), 0.45), edge)


def m_split(d, C):
    hi, mid, lo, edge, acc, _ = C
    d.rounded_rectangle([3, 5, 11, 19], radius=2, fill=mid + (255,), outline=edge + (255,))
    d.rounded_rectangle([13, 5, 21, 19], radius=2, fill=acc + (255,), outline=edge + (255,))
    d.line([(4, 7), (10, 7)], fill=hi + (220,), width=1)
    d.line([(14, 7), (20, 7)], fill=hi + (220,), width=1)


def m_line(d, C):
    hi, mid, lo, edge, acc, _ = C
    # soft board
    d.rounded_rectangle([3, 4, 21, 20], radius=2, fill=mid + (255,), outline=edge + (255,))
    d.line([(5, 16), (10, 11), (14, 13), (19, 7)], fill=acc + (255,), width=2)
    for x, y in ((5, 16), (10, 11), (14, 13), (19, 7)):
        d.ellipse([x - 1.5, y - 1.5, x + 1.5, y + 1.5], fill=hi + (255,))


def m_smart(d, C):
    hi, mid, lo, edge, acc, _ = C
    ball(d, [4, 4, 20, 20], hi, mid, lo, edge)
    d.line([(17, 3), (21, 7)], fill=acc + (255,), width=2)
    d.line([(19, 2), (19, 8)], fill=acc + (255,), width=2)


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

# Versions: material colors only — craft is always official object style
VERSIONS: Dict[str, Dict] = {
    "V1": {
        "title": "Official Vivid",
        "note": "最像官方：鲜绿 / 蓝 / 橙立体件，灰底上对比最强。",
        "C": {
            "data":    ((230, 230, 230), (185, 185, 185), (125, 125, 125), (70, 70, 70), (70, 175, 55), (50, 130, 40)),
            "train":   ((150, 215, 90), (75, 165, 40), (40, 100, 22), (25, 65, 12), (255, 230, 90), (200, 160, 40)),
            "predict": ((130, 200, 255), (45, 135, 220), (20, 80, 150), (12, 50, 95), (255, 255, 255), (200, 200, 200)),
            "eval":    ((165, 230, 75), (95, 185, 40), (50, 115, 22), (30, 75, 12), (20, 20, 20), (20, 20, 20)),
            "cluster": ((145, 215, 95), (70, 165, 45), (40, 100, 28), (25, 65, 16), (255, 175, 55), (85, 165, 235)),
            "split":   ((220, 220, 220), (170, 170, 170), (115, 115, 115), (70, 70, 70), (90, 185, 70), (70, 140, 50)),
            "line":    ((245, 175, 100), (225, 125, 50), (155, 75, 28), (95, 45, 15), (255, 230, 120), (200, 100, 40)),
            "smart":   ((255, 235, 130), (100, 185, 230), (45, 105, 165), (25, 65, 110), (255, 255, 210), (90, 210, 120)),
        },
    },
    "V2": {
        "title": "Official Soft",
        "note": "同立体语言，饱和略降，灰底上仍清晰、更耐看。",
        "C": {
            "data":    ((220, 220, 220), (175, 175, 175), (120, 120, 120), (70, 70, 70), (95, 160, 85), (70, 120, 60)),
            "train":   ((165, 195, 130), (105, 150, 80), (65, 100, 50), (40, 70, 32), (230, 205, 110), (175, 150, 60)),
            "predict": ((155, 190, 225), (95, 145, 190), (55, 100, 140), (35, 65, 95), (240, 240, 240), (190, 190, 190)),
            "eval":    ((175, 205, 125), (115, 165, 75), (75, 115, 50), (48, 78, 32), (30, 30, 30), (30, 30, 30)),
            "cluster": ((155, 190, 135), (100, 150, 85), (60, 100, 55), (38, 68, 35), (230, 165, 95), (105, 155, 200)),
            "split":   ((215, 215, 215), (165, 165, 165), (115, 115, 115), (70, 70, 70), (110, 170, 95), (80, 130, 70)),
            "line":    ((230, 170, 120), (195, 125, 75), (135, 80, 45), (85, 50, 28), (235, 200, 130), (165, 100, 55)),
            "smart":   ((230, 215, 150), (125, 170, 200), (75, 115, 150), (48, 75, 100), (245, 245, 215), (110, 180, 140)),
        },
    },
    "V3": {
        "title": "High Contrast on Grey",
        "note": "专为灰工具栏优化：更亮高光 + 更深描边，小尺寸更易认。",
        "C": {
            "data":    ((245, 245, 245), (200, 200, 200), (130, 130, 130), (50, 50, 50), (60, 200, 50), (40, 140, 30)),
            "train":   ((180, 240, 100), (90, 190, 40), (45, 110, 18), (20, 55, 8), (255, 245, 120), (220, 180, 40)),
            "predict": ((160, 220, 255), (40, 150, 240), (15, 85, 160), (8, 45, 100), (255, 255, 255), (220, 220, 220)),
            "eval":    ((190, 250, 80), (100, 205, 35), (50, 125, 15), (25, 70, 8), (10, 10, 10), (10, 10, 10)),
            "cluster": ((160, 240, 100), (70, 185, 40), (35, 110, 20), (18, 60, 10), (255, 190, 40), (70, 170, 255)),
            "split":   ((245, 245, 245), (190, 190, 190), (120, 120, 120), (50, 50, 50), (70, 210, 55), (50, 150, 35)),
            "line":    ((255, 190, 90), (240, 130, 35), (160, 70, 15), (90, 35, 8), (255, 240, 140), (220, 100, 30)),
            "smart":   ((255, 245, 140), (80, 195, 255), (30, 110, 180), (15, 65, 120), (255, 255, 230), (80, 230, 120)),
        },
    },
    "V4": {
        "title": "Task Hue Objects",
        "note": "黄=分类相关 / 橙=回归 / 绿=聚类；灰底上靠物体颜色分任务。",
        "C": {
            "data":    ((235, 235, 235), (185, 185, 185), (125, 125, 125), (70, 70, 70), (140, 140, 140), (100, 100, 100)),
            "train":   ((245, 215, 80), (210, 170, 35), (140, 105, 18), (85, 60, 10), (255, 245, 160), (200, 160, 40)),
            "predict": ((245, 215, 80), (210, 170, 35), (140, 105, 18), (85, 60, 10), (255, 255, 255), (200, 200, 200)),
            "eval":    ((245, 215, 80), (210, 170, 35), (140, 105, 18), (85, 60, 10), (20, 20, 20), (20, 20, 20)),
            "cluster": ((120, 210, 80), (70, 165, 40), (40, 105, 22), (22, 65, 12), (255, 175, 50), (80, 155, 230)),
            "split":   ((230, 230, 230), (175, 175, 175), (120, 120, 120), (70, 70, 70), (100, 185, 70), (70, 140, 50)),
            "line":    ((255, 160, 70), (230, 115, 35), (155, 65, 15), (95, 40, 8), (255, 220, 120), (200, 90, 30)),
            "smart":   ((245, 215, 100), (200, 160, 50), (130, 100, 28), (80, 60, 15), (255, 255, 210), (110, 190, 80)),
        },
    },
    "V5": {
        "title": "Gray Body + Color Accent",
        "note": "主体灰塑料（像官方灰控件），彩色只作点缀；家族统一。",
        "C": {
            "data":    ((230, 230, 230), (180, 180, 180), (120, 120, 120), (65, 65, 65), (65, 175, 55), (45, 130, 40)),
            "train":   ((220, 220, 220), (165, 165, 165), (110, 110, 110), (60, 60, 60), (75, 180, 55), (255, 210, 60)),
            "predict": ((220, 220, 220), (165, 165, 165), (110, 110, 110), (60, 60, 60), (45, 145, 230), (45, 145, 230)),
            "eval":    ((210, 220, 210), (155, 175, 150), (100, 125, 95), (55, 75, 50), (70, 170, 50), (15, 15, 15)),
            "cluster": ((220, 220, 220), (165, 165, 165), (110, 110, 110), (60, 60, 60), (235, 145, 45), (65, 145, 220)),
            "split":   ((225, 225, 225), (170, 170, 170), (115, 115, 115), (65, 65, 65), (70, 175, 55), (50, 130, 40)),
            "line":    ((225, 225, 225), (170, 170, 170), (115, 115, 115), (65, 65, 65), (235, 125, 45), (240, 190, 80)),
            "smart":   ((225, 225, 225), (170, 170, 170), (115, 115, 115), (65, 65, 65), (80, 175, 230), (240, 200, 70)),
        },
    },
}


def render(vid: str, key: str, drawer: Callable) -> Image.Image:
    C = VERSIONS[vid]["C"][key]
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    drawer(ImageDraw.Draw(im, "RGBA"), C)
    return shadow(im)


def font(size=14):
    for name in ("segoeui.ttf", "arial.ttf", "msyh.ttc"):
        try:
            return ImageFont.truetype(name, size)
        except Exception:
            continue
    return ImageFont.load_default()


def version_card(vid: str, scale=3) -> Image.Image:
    """One version on grey background, large enough to judge."""
    meta = VERSIONS[vid]
    n = len(KEYS)
    cell = SIZE * scale + 16
    label_h = 36
    sub_h = 44
    pad = 16
    w = pad * 2 + n * cell
    h = pad + label_h + sub_h + cell + 28
    card = Image.new("RGBA", (w, h), GH_GREY)
    # subtle top strip like toolbar
    d = ImageDraw.Draw(card)
    d.rectangle([0, 0, w, label_h + 8], fill=GH_GREY_LIGHT)
    d.text((pad, 10), f"{vid}  ·  {meta['title']}", font=font(16), fill=(245, 245, 245, 255))
    d.text((pad, label_h + 6), meta["note"], font=font(12), fill=(190, 190, 190, 255))

    y0 = label_h + sub_h
    for i, (key, drawer, name) in enumerate(KEYS):
        ico = render(vid, key, drawer)
        big = ico.resize((SIZE * scale, SIZE * scale), Image.Resampling.NEAREST)
        x = pad + i * cell
        # icon well
        d.rounded_rectangle([x, y0, x + cell - 8, y0 + cell - 8], radius=6, fill=(48, 48, 48, 255))
        ox = x + (cell - 8 - big.width) // 2
        oy = y0 + (cell - 8 - big.height) // 2
        card.paste(big, (ox, oy), big)
        # caption under
        tw = d.textlength(name, font=font(11)) if hasattr(d, "textlength") else len(name) * 6
        d.text((x + (cell - 8 - tw) / 2, y0 + cell - 4), name, font=font(11), fill=(200, 200, 200, 255))
    return card


def actual_size_strip(vid: str) -> Image.Image:
    """True 24px on grey — how it looks in GH."""
    n = len(KEYS)
    pad = 10
    cell = 32
    w = pad * 2 + n * cell
    h = 48
    im = Image.new("RGBA", (w, h), GH_GREY)
    d = ImageDraw.Draw(im)
    d.text((pad, 4), f"{vid} @24px", font=font(10), fill=(180, 180, 180, 255))
    for i, (key, drawer, _) in enumerate(KEYS):
        ico = render(vid, key, drawer)
        im.paste(ico, (pad + i * cell + 4, 20), ico)
    return im


def master_board() -> Image.Image:
    cards = [version_card(v) for v in VERSIONS]
    strips = [actual_size_strip(v) for v in VERSIONS]
    gap = 14
    w = max(c.width for c in cards) + 24
    h = 70 + sum(c.height + s.height + gap * 2 for c, s in zip(cards, strips))
    board = Image.new("RGBA", (w, h), (40, 40, 40, 255))
    d = ImageDraw.Draw(board)
    d.text((12, 12), "Choose on GREY background (like Grasshopper toolbar)", font=font(18), fill=(250, 250, 250, 255))
    d.text((12, 38), "Top of each block = 3x zoom | Bottom = real 24px size. Pick V1–V5.", font=font(13), fill=(180, 180, 180, 255))
    y = 70
    for card, strip in zip(cards, strips):
        board.paste(card, (12, y), card)
        y += card.height + 6
        board.paste(strip, (12, y), strip)
        y += strip.height + gap
    return board


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for vid in VERSIONS:
        for key, drawer, _ in KEYS:
            # save on transparent
            render(vid, key, drawer).save(OUT / f"{vid}_{key}.png")
            # save on grey (for direct viewing)
            g = Image.new("RGBA", (SIZE, SIZE), GH_GREY)
            ico = render(vid, key, drawer)
            g = Image.alpha_composite(g, ico)
            g.save(OUT / f"{vid}_{key}_on_grey.png")
        version_card(vid).save(OUT / f"{vid}_card.png")
        actual_size_strip(vid).save(OUT / f"{vid}_24px.png")
        print("wrote", vid)
    board = master_board()
    bp = OUT.parent / "icon_choose_board.png"
    board.save(bp)
    print("board", bp, board.size)


if __name__ == "__main__":
    main()
