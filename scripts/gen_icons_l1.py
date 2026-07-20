# -*- coding: utf-8 -*-
"""
Generate full SimpleML plugin icons — Plan L1 (Official Vivid).
Pixel-native 24x24, writes into GHA_Project/icons.
"""

from __future__ import annotations

from pathlib import Path
from typing import Callable, Dict, Tuple

from PIL import Image

OUT = Path(__file__).resolve().parents[1] / "GHA_Project" / "icons"
SIZE = 24

# L1 palettes: hi, mid, lo, edge, accent, accent2
PAL = {
    "util":  ((245, 245, 245), (200, 200, 200), (140, 140, 140), (90, 90, 90), (70, 160, 50), (50, 120, 35)),
    "clf":   ((170, 220, 100), (90, 170, 45), (50, 110, 25), (35, 75, 15), (255, 220, 80), (200, 160, 40)),
    "clf2":  ((255, 230, 120), (230, 190, 50), (160, 130, 25), (100, 80, 15), (70, 150, 220), (220, 100, 70)),
    "reg":   ((255, 190, 110), (230, 130, 55), (160, 80, 30), (100, 50, 18), (255, 230, 120), (200, 100, 40)),
    "clus":  ((150, 210, 95), (80, 165, 50), (45, 110, 30), (30, 75, 18), (240, 160, 50), (70, 150, 220)),
    "predict": ((160, 210, 255), (60, 140, 220), (30, 90, 160), (20, 55, 110), (255, 255, 255), (200, 200, 200)),
    "eval":  ((180, 230, 90), (100, 185, 45), (55, 120, 25), (35, 80, 15), (30, 30, 30), (30, 30, 30)),
    "smart": ((255, 230, 130), (90, 175, 230), (40, 110, 170), (25, 70, 120), (255, 200, 60), (80, 200, 120)),
    "help":  ((180, 210, 240), (100, 150, 200), (55, 100, 150), (35, 65, 100), (255, 255, 255), (200, 200, 200)),
}


def fill_rect(im, x0, y0, x1, y1, rgba):
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            px[x, y] = rgba


def fill_ellipse(im, x0, y0, x1, y1, rgba):
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    cx, cy = (x0 + x1) / 2.0, (y0 + y1) / 2.0
    rx, ry = max(0.5, (x1 - x0) / 2.0), max(0.5, (y1 - y0) / 2.0)
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            dx, dy = (x - cx) / rx, (y - cy) / ry
            if dx * dx + dy * dy <= 1.02:
                px[x, y] = rgba


def stroke_ellipse(im, x0, y0, x1, y1, rgba, thick=1):
    x0, x1 = sorted((int(x0), int(x1)))
    y0, y1 = sorted((int(y0), int(y1)))
    cx, cy = (x0 + x1) / 2.0, (y0 + y1) / 2.0
    rx, ry = max(0.5, (x1 - x0) / 2.0), max(0.5, (y1 - y0) / 2.0)
    inner = 1.0 - (thick + 0.35) / max(rx, ry)
    px = im.load()
    for y in range(max(0, y0), min(SIZE, y1 + 1)):
        for x in range(max(0, x0), min(SIZE, x1 + 1)):
            dx, dy = (x - cx) / rx, (y - cy) / ry
            r2 = dx * dx + dy * dy
            if inner * inner <= r2 <= 1.05:
                px[x, y] = rgba


def stroke_rect(im, x0, y0, x1, y1, rgba):
    fill_rect(im, x0, y0, x1, y0, rgba)
    fill_rect(im, x0, y1, x1, y1, rgba)
    fill_rect(im, x0, y0, x0, y1, rgba)
    fill_rect(im, x1, y0, x1, y1, rgba)


def hline(im, x0, x1, y, rgba):
    fill_rect(im, x0, y, x1, y, rgba)


def line_thick(im, x0, y0, x1, y1, rgba, w=2):
    x0, y0, x1, y1 = int(x0), int(y0), int(x1), int(y1)
    dx, dy = abs(x1 - x0), -abs(y1 - y0)
    sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
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


def fill_poly(im, pts, rgba):
    px = im.load()
    ys = [p[1] for p in pts]
    for y in range(max(0, min(ys)), min(SIZE, max(ys) + 1)):
        xs = []
        for i in range(len(pts)):
            x0, y0 = pts[i]
            x1, y1 = pts[(i + 1) % len(pts)]
            if y0 == y1:
                continue
            if min(y0, y1) <= y <= max(y0, y1):
                t = (y - y0) / (y1 - y0) if y1 != y0 else 0
                xs.append(x0 + t * (x1 - x0))
        if len(xs) < 2:
            continue
        xs.sort()
        for x in range(max(0, int(xs[0] + 0.5)), min(SIZE, int(xs[-1] + 0.5) + 1)):
            px[x, y] = rgba


def shadow(im: Image.Image) -> Image.Image:
    out = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    src, dst = im.load(), out.load()
    for y in range(SIZE):
        for x in range(SIZE):
            if src[x, y][3] < 250:
                continue
            sx, sy = x + 1, y + 1
            if 0 <= sx < SIZE and 0 <= sy < SIZE and dst[sx, sy][3] < 20:
                dst[sx, sy] = (0, 0, 0, 40)
    return Image.alpha_composite(out, im)


# ---- metaphors ----

def m_data(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 4, 5, 19, 18, mid + (255,))
    fill_rect(im, 4, 5, 19, 8, acc + (255,))
    stroke_rect(im, 4, 5, 19, 18, edge + (255,))
    hline(im, 4, 19, 12, edge + (255,))
    hline(im, 4, 19, 15, edge + (255,))
    fill_rect(im, 11, 8, 11, 18, edge + (255,))
    fill_rect(im, 5, 6, 18, 6, hi + (255,))


def m_doc(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 6, 3, 17, 20, mid + (255,))
    stroke_rect(im, 6, 3, 17, 20, edge + (255,))
    fill_rect(im, 12, 3, 17, 8, lo + (255,))
    hline(im, 8, 15, 10, edge + (255,))
    hline(im, 8, 15, 13, edge + (255,))
    hline(im, 8, 13, 16, edge + (255,))
    fill_rect(im, 7, 4, 11, 5, hi + (255,))


def m_doc_out(im, p):
    m_doc(im, p)
    hi, mid, lo, edge, acc, _ = p
    fill_poly(im, [(16, 14), (22, 17), (16, 20)], edge + (255,))


def m_train(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 4, 9, 19, 14, mid + (255,))
    fill_rect(im, 9, 4, 14, 19, mid + (255,))
    stroke_rect(im, 4, 9, 19, 14, edge + (255,))
    stroke_rect(im, 9, 4, 14, 19, edge + (255,))
    fill_rect(im, 10, 10, 13, 13, lo + (255,))
    fill_rect(im, 5, 10, 8, 11, hi + (255,))
    fill_rect(im, 10, 5, 11, 8, hi + (255,))


def m_predict(im, p):
    hi, mid, lo, edge, _, _ = p
    fill_poly(im, [(5, 4), (19, 12), (5, 19)], mid + (255,))
    fill_poly(im, [(5, 4), (19, 12), (7, 12)], hi + (255,))
    fill_poly(im, [(5, 19), (19, 12), (7, 12)], lo + (255,))
    line_thick(im, 5, 4, 19, 12, edge + (255,), 1)
    line_thick(im, 19, 12, 5, 19, edge + (255,), 1)
    line_thick(im, 5, 19, 5, 4, edge + (255,), 1)


def m_eval(im, p):
    hi, mid, lo, edge, _, _ = p
    fill_ellipse(im, 3, 3, 20, 20, mid + (255,))
    stroke_ellipse(im, 3, 3, 20, 20, edge + (255,), thick=1)
    fill_ellipse(im, 5, 5, 10, 9, hi + (255,))
    line_thick(im, 7, 12, 11, 16, (25, 25, 25, 255), 3)
    line_thick(im, 11, 16, 17, 8, (25, 25, 25, 255), 3)


def m_cluster(im, p):
    hi, mid, lo, edge, acc, ac2 = p
    fill_ellipse(im, 2, 2, 12, 12, mid + (255,))
    stroke_ellipse(im, 2, 2, 12, 12, edge + (255,), thick=2)
    fill_rect(im, 4, 4, 7, 6, hi + (255,))
    fill_ellipse(im, 12, 2, 22, 12, acc + (255,))
    stroke_ellipse(im, 12, 2, 22, 12, edge + (255,), thick=2)
    fill_rect(im, 14, 4, 17, 6, hi + (255,))
    fill_ellipse(im, 7, 11, 17, 21, ac2 + (255,))
    stroke_ellipse(im, 7, 11, 17, 21, edge + (255,), thick=2)
    fill_rect(im, 9, 13, 12, 15, hi + (255,))


def m_split(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 5, 10, 18, mid + (255,))
    fill_rect(im, 13, 5, 20, 18, acc + (255,))
    stroke_rect(im, 3, 5, 10, 18, edge + (255,))
    stroke_rect(im, 13, 5, 20, 18, edge + (255,))
    fill_rect(im, 4, 6, 9, 7, hi + (255,))
    fill_rect(im, 14, 6, 19, 7, hi + (255,))


def m_bars(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 4, 20, 19, mid + (255,))
    stroke_rect(im, 3, 4, 20, 19, edge + (255,))
    for i, h in enumerate((6, 9, 13)):
        x0 = 5 + i * 5
        fill_rect(im, x0, 18 - h, x0 + 3, 18, (acc if i == 2 else lo) + (255,))


def m_line(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 4, 20, 19, mid + (255,))
    stroke_rect(im, 3, 4, 20, 19, edge + (255,))
    for i, h in enumerate((6, 9, 13)):
        x0 = 5 + i * 5
        fill_rect(im, x0, 18 - h, x0 + 3, 18, (acc if i == 2 else lo) + (255,))
    line_thick(im, 5, 15, 10, 11, hi + (255,), 2)
    line_thick(im, 10, 11, 15, 12, hi + (255,), 2)
    line_thick(im, 15, 12, 19, 7, hi + (255,), 2)


def m_smart(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 4, 4, 19, 19, mid + (255,))
    stroke_ellipse(im, 4, 4, 19, 19, edge + (255,))
    fill_ellipse(im, 6, 6, 11, 10, hi + (255,))
    fill_rect(im, 18, 3, 20, 8, acc + (255,))
    fill_rect(im, 16, 5, 22, 7, acc + (255,))
    stroke_rect(im, 16, 3, 22, 8, edge + (255,))


def m_disk(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 4, 4, 19, 19, mid + (255,))
    stroke_ellipse(im, 4, 4, 19, 19, edge + (255,), thick=2)
    fill_ellipse(im, 9, 9, 14, 14, lo + (255,))
    fill_rect(im, 6, 6, 9, 8, hi + (255,))


def m_tree(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 9, 3, 14, 8, mid + (255,))
    stroke_ellipse(im, 9, 3, 14, 8, edge + (255,))
    fill_ellipse(im, 3, 13, 10, 20, mid + (255,))
    stroke_ellipse(im, 3, 13, 10, 20, edge + (255,))
    fill_ellipse(im, 13, 13, 20, 20, acc + (255,))
    stroke_ellipse(im, 13, 13, 20, 20, edge + (255,))
    line_thick(im, 11, 8, 11, 13, edge + (255,), 2)
    line_thick(im, 11, 13, 6, 15, edge + (255,), 2)
    line_thick(im, 11, 13, 17, 15, edge + (255,), 2)


def m_knn(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 8, 8, 15, 15, mid + (255,))
    stroke_ellipse(im, 8, 8, 15, 15, edge + (255,), thick=2)
    for box in ((3, 3, 8, 8), (15, 3, 20, 8), (3, 15, 8, 20), (15, 15, 20, 20)):
        fill_ellipse(im, *box, acc + (255,))
        stroke_ellipse(im, *box, edge + (255,))


def m_svm(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    line_thick(im, 5, 18, 18, 6, hi + (255,), 3)
    fill_ellipse(im, 5, 5, 10, 10, acc + (255,))
    stroke_ellipse(im, 5, 5, 10, 10, edge + (255,))
    fill_ellipse(im, 13, 13, 18, 18, lo + (255,))
    stroke_ellipse(im, 13, 13, 18, 18, edge + (255,))


def m_diag(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    line_thick(im, 5, 18, 18, 6, hi + (255,), 3)
    fill_ellipse(im, 6, 14, 10, 18, acc + (255,))
    fill_ellipse(im, 14, 6, 18, 10, lo + (255,))


def m_matrix(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    for r in range(3):
        for c in range(3):
            x0, y0 = 5 + c * 5, 5 + r * 5
            fill_rect(im, x0, y0, x0 + 3, y0 + 3, (acc if (r + c) % 2 == 0 else lo) + (255,))


def m_pca(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    line_thick(im, 5, 18, 18, 6, hi + (255,), 3)
    line_thick(im, 6, 7, 17, 17, edge + (255,), 2)


def m_elbow(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    line_thick(im, 5, 6, 9, 10, hi + (255,), 2)
    line_thick(im, 9, 10, 12, 16, hi + (255,), 2)
    line_thick(im, 12, 16, 19, 18, hi + (255,), 2)


def m_globe(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 3, 3, 20, 20, mid + (255,))
    stroke_ellipse(im, 3, 3, 20, 20, edge + (255,), thick=2)
    fill_rect(im, 11, 4, 12, 19, edge + (255,))
    fill_ellipse(im, 5, 5, 10, 9, hi + (255,))


def m_book(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 4, 4, 19, 20, mid + (255,))
    stroke_rect(im, 4, 4, 19, 20, edge + (255,))
    fill_rect(im, 11, 4, 12, 20, edge + (255,))
    hline(im, 6, 10, 8, edge + (255,))
    hline(im, 14, 18, 8, edge + (255,))
    fill_rect(im, 5, 5, 10, 6, hi + (255,))


def m_info(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 3, 3, 20, 20, mid + (255,))
    stroke_ellipse(im, 3, 3, 20, 20, edge + (255,), thick=2)
    fill_rect(im, 10, 6, 13, 9, (25, 25, 25, 255))
    fill_rect(im, 10, 11, 13, 17, (25, 25, 25, 255))


def m_wiz(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 7, 3, 16, 12, mid + (255,))
    stroke_ellipse(im, 7, 3, 16, 12, edge + (255,))
    fill_poly(im, [(5, 20), (12, 11), (19, 20)], acc + (255,))
    line_thick(im, 5, 20, 12, 11, edge + (255,), 1)
    line_thick(im, 12, 11, 19, 20, edge + (255,), 1)
    line_thick(im, 5, 20, 19, 20, edge + (255,), 1)


def m_reset(im, p):
    hi, mid, lo, edge, acc, _ = p
    stroke_ellipse(im, 4, 4, 19, 19, mid + (255,), thick=3)
    # open gap
    fill_rect(im, 14, 3, 20, 9, (0, 0, 0, 0))
    # clear gap properly by redrawing bg transparent — just cover with empty: redraw arc gap
    px = im.load()
    for y in range(3, 10):
        for x in range(14, 21):
            px[x, y] = (0, 0, 0, 0)
    stroke_ellipse(im, 4, 4, 19, 19, mid + (255,), thick=3)
    for y in range(3, 10):
        for x in range(15, 21):
            if px[x, y][3] > 0 and y < 8 and x > 16:
                px[x, y] = (0, 0, 0, 0)
    fill_poly(im, [(15, 3), (21, 7), (14, 8)], edge + (255,))


def m_deconstruct(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 13, 13, mid + (255,))
    stroke_rect(im, 3, 3, 13, 13, edge + (255,))
    fill_ellipse(im, 11, 11, 21, 21, acc + (255,))
    stroke_ellipse(im, 11, 11, 21, 21, edge + (255,), thick=2)


def m_color(im, p):
    m_cluster(im, p)


def m_bayes(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_ellipse(im, 4, 4, 19, 19, mid + (255,))
    stroke_ellipse(im, 4, 4, 19, 19, edge + (255,), thick=2)
    fill_ellipse(im, 8, 8, 15, 15, lo + (255,))
    fill_rect(im, 6, 6, 9, 8, hi + (255,))


def m_sigmoid(im, p):
    hi, mid, lo, edge, acc, _ = p
    fill_rect(im, 3, 3, 20, 20, mid + (255,))
    stroke_rect(im, 3, 3, 20, 20, edge + (255,))
    line_thick(im, 5, 17, 9, 16, hi + (255,), 2)
    line_thick(im, 9, 16, 12, 12, hi + (255,), 2)
    line_thick(im, 12, 12, 15, 8, hi + (255,), 2)
    line_thick(im, 15, 8, 19, 7, hi + (255,), 2)


def make(drawer: Callable, fam: str) -> Image.Image:
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    drawer(im, PAL[fam])
    return shadow(im)


# filename -> (family, drawer)
ICONS: Dict[str, Tuple[str, Callable]] = {
    "read_csv.png": ("util", m_doc),
    "read_excel.png": ("util", m_doc),
    "write_csv.png": ("util", m_doc_out),
    "write_excel.png": ("util", m_doc_out),
    "load_dataset.png": ("util", m_data),
    "quick_dataset.png": ("util", m_data),
    "create_dataset.png": ("util", m_data),
    "deconstruct_dataset.png": ("util", m_deconstruct),
    "split_data.png": ("util", m_split),
    "calculate_statistics.png": ("util", m_bars),
    "calculate_correlation.png": ("util", m_matrix),
    "describe_features.png": ("util", m_bars),
    "get_data_summary.png": ("util", m_bars),
    "save_model.png": ("util", m_disk),
    "load_model.png": ("util", m_disk),
    "feature_importance.png": ("util", m_bars),
    "reduce_dimensions.png": ("util", m_pca),
    # classification
    "train_classifier.png": ("clf", m_train),
    "predict_classifier.png": ("predict", m_predict),
    "evaluate_classification.png": ("eval", m_eval),
    "visualize_classification_labels.png": ("clf2", m_color),
    "random_forest_classifier.png": ("clf", m_tree),
    "support_vector_machine_classifier.png": ("clf", m_svm),
    "k_nearest_neighbors_classifier.png": ("clf", m_knn),
    "logistic_regression.png": ("clf", m_sigmoid),
    "naive_bayes_classifier.png": ("clf", m_bayes),
    "decision_tree_classifier.png": ("clf", m_tree),
    # regression
    "train_regressor.png": ("reg", m_train),
    "predict_regressor.png": ("predict", m_predict),
    "evaluate_regression.png": ("reg", m_line),
    "visualize_regression.png": ("reg", m_line),
    "random_forest_regressor.png": ("reg", m_tree),
    "support_vector_regression.png": ("reg", m_svm),
    "linear_regression.png": ("reg", m_diag),
    "ridge_regression.png": ("reg", m_diag),
    "lasso_regression.png": ("reg", m_diag),
    "k_nearest_neighbors_regressor.png": ("reg", m_knn),
    # clustering
    "train_cluster.png": ("clus", m_train),
    "predict_cluster.png": ("predict", m_predict),
    "evaluate_clustering.png": ("eval", m_eval),
    "visualize_cluster_labels.png": ("clus", m_color),
    "quick_cluster_color.png": ("clus", m_color),
    "k_means.png": ("clus", m_cluster),
    "density_baised_spatial_clistering_of_applications_with_noise.png": ("clus", m_cluster),
    "agglomerative_clustering.png": ("clus", m_tree),
    "silhouette_score.png": ("clus", m_eval),
    "elbow_method.png": ("clus", m_elbow),
    # smart / help
    "smart_train.png": ("smart", m_smart),
    "predict_auto.png": ("predict", m_predict),
    "evaluate_auto.png": ("eval", m_eval),
    "about.png": ("help", m_info),
    "language.png": ("help", m_globe),
    "installation_guide.png": ("help", m_book),
    "health_check.png": ("eval", m_eval),
    "beginner_wizard.png": ("help", m_wiz),
    "reset_python.png": ("help", m_reset),
}


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for name, (fam, drawer) in sorted(ICONS.items()):
        make(drawer, fam).save(OUT / name, "PNG")
        print("wrote", name)
    # contact sheet on light grey
    icons = sorted(OUT.glob("*.png"))
    cols = 10
    cell = 36
    rows = (len(icons) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * cell, rows * cell), (232, 232, 232, 255))
    for i, p in enumerate(icons):
        im = Image.open(p).convert("RGBA")
        big = im.resize((30, 30), Image.Resampling.NEAREST)
        sheet.paste(big, ((i % cols) * cell + 3, (i // cols) * cell + 3), big)
    sheet_path = OUT.parents[1] / "docs" / "icon_l1_contact_sheet.png"
    sheet.save(sheet_path)
    print("sheet", sheet_path, "n=", len(icons))


if __name__ == "__main__":
    main()
