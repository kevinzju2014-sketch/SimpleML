# -*- coding: utf-8 -*-
"""
High-clarity SimpleML icons for Grasshopper 24x24.
Design rules:
  - Flat task-hue plate (no noisy gradients inside glyph)
  - ONE bold white silhouette OR 2-letter code
  - Strong contrast; integer pixel strokes
"""

from __future__ import annotations

from pathlib import Path
from typing import Callable, Dict, Tuple

from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).resolve().parents[1] / "GHA_Project" / "icons"
SIZE = 24

# Task hues: fill, edge
PAL = {
    "clf":  ((210, 170, 40), (90, 70, 15)),
    "reg":  ((210, 115, 55), (95, 45, 18)),
    "clus": ((85, 150, 70), (35, 70, 28)),
    "util": ((140, 145, 155), (55, 58, 64)),
    "help": ((110, 130, 160), (40, 55, 75)),
    "smart":((190, 155, 75), (85, 65, 30)),
}

INK = (255, 255, 255, 255)
INK_DIM = (255, 255, 255, 200)


def plate(draw: ImageDraw.ImageDraw, fill, edge, r=5):
    draw.rounded_rectangle([1, 1, 22, 22], radius=r, fill=fill + (255,), outline=edge + (255,), width=1)


def font(size=11):
    for name in ("segoeui.ttf", "arial.ttf", "DejaVuSans-Bold.ttf", "msyh.ttc"):
        try:
            return ImageFont.truetype(name, size)
        except Exception:
            continue
    return ImageFont.load_default()


def badge(draw, text, fill, edge):
    """Big 1–2 letter mark — highest recognition at 24px."""
    plate(draw, fill, edge)
    f = font(12 if len(text) <= 2 else 10)
    bbox = draw.textbbox((0, 0), text, font=f)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    x = (SIZE - tw) // 2 - bbox[0]
    y = (SIZE - th) // 2 - bbox[1] - 1
    draw.text((x, y), text, font=f, fill=INK)


# ---- Silhouette drawers (no letters) ----

def g_play(d, fill, edge):
    plate(d, fill, edge)
    d.polygon([(8, 6), (18, 12), (8, 18)], fill=INK)


def g_check(d, fill, edge):
    plate(d, fill, edge)
    d.line([(6, 12), (10, 17), (18, 7)], fill=INK, width=3)


def g_table(d, fill, edge):
    plate(d, fill, edge)
    d.rectangle([5, 5, 19, 9], fill=INK)
    d.line([(5, 13), (19, 13)], fill=INK, width=2)
    d.line([(5, 17), (19, 17)], fill=INK, width=2)
    d.line([(12, 9), (12, 19)], fill=INK, width=2)


def g_split(d, fill, edge):
    plate(d, fill, edge)
    d.rounded_rectangle([4, 5, 10, 19], radius=2, fill=INK)
    d.rounded_rectangle([14, 5, 20, 19], radius=2, fill=INK)


def g_bars(d, fill, edge):
    plate(d, fill, edge)
    d.rectangle([6, 14, 9, 19], fill=INK)
    d.rectangle([11, 10, 14, 19], fill=INK)
    d.rectangle([16, 6, 19, 19], fill=INK)


def g_line(d, fill, edge):
    plate(d, fill, edge)
    d.line([(5, 17), (10, 12), (14, 14), (19, 6)], fill=INK, width=3)


def g_cluster(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([4, 4, 12, 12], fill=INK)
    d.ellipse([13, 4, 21, 12], fill=INK)
    d.ellipse([8, 13, 16, 21], fill=INK)


def g_disk(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([5, 5, 19, 19], outline=INK, width=2)
    d.ellipse([9, 9, 15, 15], fill=INK)


def g_doc(d, fill, edge):
    plate(d, fill, edge)
    d.rounded_rectangle([7, 4, 17, 20], radius=2, fill=INK)
    d.rectangle([9, 8, 15, 10], fill=fill + (255,))
    d.rectangle([9, 12, 15, 14], fill=fill + (255,))


def g_doc_out(d, fill, edge):
    g_doc(d, fill, edge)
    d.polygon([(16, 14), (22, 17), (16, 20)], fill=edge + (255,))


def g_tree(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([9, 3, 15, 9], fill=INK)
    d.ellipse([4, 13, 10, 19], fill=INK)
    d.ellipse([14, 13, 20, 19], fill=INK)
    d.line([(12, 9), (12, 13)], fill=INK, width=2)
    d.line([(12, 13), (7, 15)], fill=INK, width=2)
    d.line([(12, 13), (17, 15)], fill=INK, width=2)


def g_knn(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([9, 9, 15, 15], fill=INK)
    for b in ((4, 4), (17, 4), (4, 17), (17, 17)):
        d.ellipse([b[0], b[1], b[0] + 4, b[1] + 4], fill=INK_DIM)


def g_svm(d, fill, edge):
    plate(d, fill, edge)
    d.line([(5, 18), (19, 6)], fill=INK, width=3)
    d.ellipse([5, 5, 10, 10], fill=INK)
    d.ellipse([14, 14, 19, 19], fill=INK)


def g_sigmoid(d, fill, edge):
    plate(d, fill, edge)
    d.line([(5, 17), (9, 16), (12, 12), (15, 8), (19, 7)], fill=INK, width=3)


def g_diag(d, fill, edge):
    plate(d, fill, edge)
    d.line([(5, 18), (19, 6)], fill=INK, width=3)


def g_pca(d, fill, edge):
    plate(d, fill, edge)
    d.line([(6, 18), (18, 6)], fill=INK, width=3)
    d.line([(7, 7), (17, 17)], fill=INK, width=2)


def g_elbow(d, fill, edge):
    plate(d, fill, edge)
    d.line([(5, 6), (9, 10), (12, 16), (19, 18)], fill=INK, width=3)


def g_globe(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([5, 5, 19, 19], outline=INK, width=2)
    d.line([(12, 5), (12, 19)], fill=INK, width=2)
    d.arc([5, 8, 19, 16], 0, 180, fill=INK, width=2)


def g_book(d, fill, edge):
    plate(d, fill, edge)
    d.rounded_rectangle([5, 4, 19, 20], radius=2, fill=INK)
    d.line([(12, 4), (12, 20)], fill=fill + (255,), width=2)


def g_info(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([10, 5, 14, 9], fill=INK)
    d.rectangle([10, 11, 14, 18], fill=INK)


def g_wiz(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([8, 3, 16, 11], fill=INK)
    d.polygon([(6, 20), (12, 11), (18, 20)], fill=INK)


def g_reset(d, fill, edge):
    plate(d, fill, edge)
    d.arc([6, 6, 18, 18], 40, 300, fill=INK, width=3)
    d.polygon([(16, 4), (21, 9), (14, 9)], fill=INK)


def g_smart(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([7, 3, 17, 13], fill=INK)
    d.rounded_rectangle([5, 13, 19, 21], radius=3, fill=INK)
    d.line([(19, 3), (22, 6)], fill=INK, width=2)


def g_matrix(d, fill, edge):
    plate(d, fill, edge)
    for r in range(3):
        for c in range(3):
            x0, y0 = 5 + c * 5, 5 + r * 5
            if (r + c) % 2 == 0:
                d.rectangle([x0, y0, x0 + 4, y0 + 4], fill=INK)


def g_color(d, fill, edge):
    plate(d, fill, edge)
    d.ellipse([4, 4, 11, 11], fill=INK)
    d.ellipse([13, 5, 20, 12], fill=INK_DIM)
    d.ellipse([8, 13, 16, 21], fill=INK)


def g_deconstruct(d, fill, edge):
    plate(d, fill, edge)
    d.rounded_rectangle([4, 4, 13, 13], radius=2, fill=INK)
    d.ellipse([12, 12, 21, 21], fill=INK)


def make_draw(drawer: Callable, fam: str) -> Image.Image:
    fill, edge = PAL[fam]
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    drawer(ImageDraw.Draw(im), fill, edge)
    return im


def make_badge(text: str, fam: str) -> Image.Image:
    fill, edge = PAL[fam]
    im = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    badge(ImageDraw.Draw(im), text, fill, edge)
    return im


# filename -> (family, drawer | ("badge", "XX"))
ICONS: Dict[str, Tuple] = {
    # Data / util
    "read_csv.png": ("util", g_doc),
    "read_excel.png": ("util", g_doc),
    "write_csv.png": ("util", g_doc_out),
    "write_excel.png": ("util", g_doc_out),
    "load_dataset.png": ("util", g_table),
    "quick_dataset.png": ("util", g_table),
    "create_dataset.png": ("util", g_table),
    "deconstruct_dataset.png": ("util", g_deconstruct),
    "split_data.png": ("util", g_split),
    "calculate_statistics.png": ("util", g_bars),
    "calculate_correlation.png": ("util", g_matrix),
    "describe_features.png": ("util", ("badge", "Fx")),
    "get_data_summary.png": ("util", ("badge", "Σ")),
    "save_model.png": ("util", g_disk),
    "load_model.png": ("util", g_disk),
    "feature_importance.png": ("util", g_bars),
    "reduce_dimensions.png": ("util", g_pca),
    # Classification
    "train_classifier.png": ("clf", ("badge", "Tr")),
    "predict_classifier.png": ("clf", g_play),
    "evaluate_classification.png": ("clf", g_check),
    "visualize_classification_labels.png": ("clf", g_color),
    "random_forest_classifier.png": ("clf", ("badge", "RF")),
    "support_vector_machine_classifier.png": ("clf", ("badge", "SV")),
    "k_nearest_neighbors_classifier.png": ("clf", ("badge", "KN")),
    "logistic_regression.png": ("clf", ("badge", "LR")),
    "naive_bayes_classifier.png": ("clf", ("badge", "NB")),
    "decision_tree_classifier.png": ("clf", g_tree),
    # Regression
    "train_regressor.png": ("reg", ("badge", "Tr")),
    "predict_regressor.png": ("reg", g_play),
    "evaluate_regression.png": ("reg", g_line),
    "visualize_regression.png": ("reg", g_line),
    "random_forest_regressor.png": ("reg", ("badge", "RF")),
    "support_vector_regression.png": ("reg", ("badge", "SV")),
    "linear_regression.png": ("reg", g_diag),
    "ridge_regression.png": ("reg", ("badge", "Rd")),
    "lasso_regression.png": ("reg", ("badge", "La")),
    "k_nearest_neighbors_regressor.png": ("reg", ("badge", "KN")),
    # Clustering
    "train_cluster.png": ("clus", ("badge", "Tr")),
    "predict_cluster.png": ("clus", g_play),
    "evaluate_clustering.png": ("clus", g_check),
    "visualize_cluster_labels.png": ("clus", g_color),
    "quick_cluster_color.png": ("clus", g_color),
    "k_means.png": ("clus", ("badge", "K")),
    "density_baised_spatial_clistering_of_applications_with_noise.png": ("clus", ("badge", "DB")),
    "agglomerative_clustering.png": ("clus", ("badge", "Ag")),
    "silhouette_score.png": ("clus", ("badge", "Si")),
    "elbow_method.png": ("clus", g_elbow),
    # Smart / help
    "smart_train.png": ("smart", g_smart),
    "predict_auto.png": ("smart", g_play),
    "evaluate_auto.png": ("smart", g_check),
    "about.png": ("help", g_info),
    "language.png": ("help", g_globe),
    "installation_guide.png": ("help", g_book),
    "health_check.png": ("help", g_check),
    "beginner_wizard.png": ("help", g_wiz),
    "reset_python.png": ("help", g_reset),
}


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for name, (fam, kind) in sorted(ICONS.items()):
        if isinstance(kind, tuple) and kind[0] == "badge":
            im = make_badge(kind[1], fam)
        else:
            im = make_draw(kind, fam)
        im.save(OUT / name, "PNG")
        print("wrote", name)

    icons = sorted(OUT.glob("*.png"))
    cols = 10
    rows = (len(icons) + cols - 1) // cols
    cell = 36
    sheet = Image.new("RGBA", (cols * cell, rows * cell), (40, 40, 40, 255))
    for i, p in enumerate(icons):
        im = Image.open(p).convert("RGBA")
        # upscale 1.25x for contact sheet readability
        big = im.resize((30, 30), Image.Resampling.NEAREST)
        sheet.paste(big, ((i % cols) * cell + 3, (i // cols) * cell + 3), big)
    sheet_path = OUT.parents[1] / "docs" / "icon_d2_contact_sheet.png"
    sheet.save(sheet_path)
    print("sheet", sheet_path, "n=", len(icons))


if __name__ == "__main__":
    main()
