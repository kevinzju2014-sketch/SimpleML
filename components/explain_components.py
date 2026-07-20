"""
可解释性与通俗结果解读组件（中英切换）。
"""

from __future__ import annotations

import json
from typing import Any, List, Tuple

import numpy as np
from sklearn.metrics import silhouette_score

from core.model_bundle import SimpleMLModel, unwrap_model
from components.i18n import is_zh, t


def verdict_classification(metrics: dict) -> str:
    acc = metrics.get("accuracy")
    if acc is None:
        return t(
            "Verdict: accuracy unavailable. Check test labels.",
            "结论: 无法计算准确率，请检查测试集标签。",
        )
    a = float(acc)
    if a >= 0.9:
        level_en, tip_en = "strong", "Ready to try; still check the confusion matrix for weak classes."
        level_zh, tip_zh = "很强", "可进入试用；仍建议看混淆矩阵是否某类偏弱。"
    elif a >= 0.7:
        level_en, tip_en = "usable", "A solid baseline; try more features or Random Forest."
        level_zh, tip_zh = "可用", "作为基线可用；可尝试更多特征或随机森林。"
    elif a >= 0.5:
        level_en, tip_en = "weak", "Check data quality / class balance, or switch algorithms."
        level_zh, tip_zh = "偏弱", "建议检查数据质量、类别平衡，或换算法。"
    else:
        level_en, tip_en = "poor", "Confirm labels and features are wired correctly, then retrain."
        level_zh, tip_zh = "较差", "先确认标签与特征是否接对，再考虑重新训练。"
    if is_zh():
        return f"结论: {level_zh}（准确率 {a*100:.1f}%）。{tip_zh}"
    return f"Verdict: {level_en} (accuracy {a*100:.1f}%). {tip_en}"


def verdict_regression(metrics: dict) -> str:
    r2 = metrics.get("r2_score")
    if r2 is None:
        return t(
            "Verdict: R² unavailable. Check the test set.",
            "结论: 无法计算 R²，请检查测试集。",
        )
    v = float(r2)
    if v >= 0.9:
        level_en, tip_en = "strong", "Great fit; watch for overfitting."
        level_zh, tip_zh = "很强", "拟合很好，注意是否过拟合。"
    elif v >= 0.7:
        level_en, tip_en = "good", "Useful as a practical baseline."
        level_zh, tip_zh = "较好", "可作为实用基线。"
    elif v >= 0.3:
        level_en, tip_en = "fair", "Try nonlinear models or feature engineering."
        level_zh, tip_zh = "一般", "可尝试非线性模型或特征工程。"
    else:
        level_en, tip_en = "weak", "Check feature relevance or collect more data."
        level_zh, tip_zh = "偏弱", "检查特征是否相关，或增加数据量。"
    if is_zh():
        return f"结论: {level_zh}（R²={v:.3f}）。{tip_zh}"
    return f"Verdict: {level_en} (R²={v:.3f}). {tip_en}"


def verdict_clustering(metrics: dict) -> str:
    sil = metrics.get("silhouette_score")
    if sil is None:
        return t(
            "Verdict: silhouette unavailable (too few clusters). Try Elbow Method for K.",
            "结论: 无法计算轮廓系数（簇数可能不足）。可用肘部法则重选 K。",
        )
    s = float(sil)
    if s >= 0.5:
        level_en, tip_en = "clear clusters", "Good for grouping / color visualization."
        level_zh, tip_zh = "簇结构清晰", "可直接用于分组可视化。"
    elif s >= 0.25:
        level_en, tip_en = "some structure", "Try adjusting K or scaling features."
        level_zh, tip_zh = "有一定结构", "可调整 K 或先标准化。"
    else:
        level_en, tip_en = "weak boundaries", "Try Elbow Method for K, or switch to DBSCAN."
        level_zh, tip_zh = "边界较弱", "建议 Elbow Method 换 K，或改用 DBSCAN。"
    if is_zh():
        return f"结论: {level_zh}（轮廓系数={s:.3f}）。{tip_zh}"
    return f"Verdict: {level_en} (silhouette={s:.3f}). {tip_en}"


def explain_classification_metrics(metrics: dict) -> str:
    acc = metrics.get("accuracy")
    lines = [
        t("Plain-language notes (classification)", "通俗解读（分类）"),
        "=" * 40,
    ]
    if acc is not None:
        pct = float(acc) * 100
        lines.append(
            t(
                f"Accuracy {pct:.1f}%: about {pct:.0f} of every 100 predictions are correct.",
                f"准确率 {pct:.1f}%：大约每 100 次预测，对了 {pct:.0f} 次。",
            )
        )
        if acc >= 0.9:
            lines.append(
                t(
                    "Performance: strong. Safe to trial, but still check the confusion matrix.",
                    "表现：很强。可以进入业务试用，但仍建议检查混淆矩阵是否偏科。",
                )
            )
        elif acc >= 0.7:
            lines.append(
                t(
                    "Performance: usable. If a class is rare, also check precision/recall.",
                    "表现：可用。若某类别很少，请同时看精确率/召回率。",
                )
            )
        else:
            lines.append(
                t(
                    "Performance: weak. Try more data, feature engineering, or Random Forest.",
                    "表现：偏弱。可尝试更多数据、特征工程或换算法（如随机森林）。",
                )
            )
    if metrics.get("precision") is not None:
        lines.append(
            t(
                f"Precision {float(metrics['precision'])*100:.1f}%: when the model says yes, how often it is right.",
                f"精确率 {float(metrics['precision'])*100:.1f}%：模型说“是”时，真的是的比例。",
            )
        )
    if metrics.get("recall") is not None:
        lines.append(
            t(
                f"Recall {float(metrics['recall'])*100:.1f}%: of true positives, how many were found.",
                f"召回率 {float(metrics['recall'])*100:.1f}%：真实“是”里，被找出来的比例。",
            )
        )
    if metrics.get("f1_score") is not None:
        lines.append(
            t(
                f"F1 {float(metrics['f1_score']):.3f}: balance of precision and recall (closer to 1 is better).",
                f"F1 {float(metrics['f1_score']):.3f}：精确率与召回率的平衡分，越接近 1 越好。",
            )
        )
    return "\n".join(lines)


def explain_regression_metrics(metrics: dict) -> str:
    lines = [
        t("Plain-language notes (regression)", "通俗解读（回归）"),
        "=" * 40,
    ]
    r2 = metrics.get("r2_score")
    rmse = metrics.get("rmse")
    mae = metrics.get("mae")
    if r2 is not None:
        lines.append(
            t(
                f"R² = {float(r2):.3f}: the model explains about {float(r2)*100:.1f}% of target variation.",
                f"R² = {float(r2):.3f}：模型解释了大约 {float(r2)*100:.1f}% 的目标变化。",
            )
        )
        if r2 >= 0.9:
            lines.append(t("Performance: excellent fit.", "表现：拟合很好。"))
        elif r2 >= 0.7:
            lines.append(t("Performance: good practical baseline.", "表现：较好，可作为实用基线。"))
        elif r2 >= 0.3:
            lines.append(t("Performance: fair; room to improve.", "表现：一般，还有较大改进空间。"))
        else:
            lines.append(
                t(
                    "Performance: weak; check feature relevance or try nonlinear models.",
                    "表现：偏弱，建议检查特征是否相关、是否需要非线性模型。",
                )
            )
    if rmse is not None:
        lines.append(
            t(
                f"RMSE = {float(rmse):.4f}: typical error magnitude (same units as the target).",
                f"RMSE = {float(rmse):.4f}：典型误差量级（与目标同单位）。",
            )
        )
    if mae is not None:
        lines.append(
            t(
                f"MAE = {float(mae):.4f}: mean absolute error (more robust to outliers).",
                f"MAE = {float(mae):.4f}：平均绝对误差，对异常值更稳健。",
            )
        )
    return "\n".join(lines)


def explain_clustering_metrics(metrics: dict) -> str:
    lines = [
        t("Plain-language notes (clustering)", "通俗解读（聚类）"),
        "=" * 40,
    ]
    sil = metrics.get("silhouette_score")
    if sil is not None:
        s = float(sil)
        lines.append(
            t(
                f"Silhouette = {s:.3f} (about -1..1; higher means tighter, better-separated clusters).",
                f"轮廓系数 = {s:.3f}（范围约 -1~1，越高簇越紧凑且分离）。",
            )
        )
        if s >= 0.5:
            lines.append(t("Performance: clusters look clear.", "表现：簇结构比较清晰。"))
        elif s >= 0.25:
            lines.append(
                t(
                    "Performance: some structure; try another K or feature scaling.",
                    "表现：有一定结构，可尝试调整簇数或特征缩放。",
                )
            )
        else:
            lines.append(
                t(
                    "Performance: weak boundaries; try Elbow Method or DBSCAN.",
                    "表现：簇边界较弱，建议用 Elbow Method 换 K，或改 DBSCAN。",
                )
            )
    if metrics.get("davies_bouldin_score") is not None:
        lines.append(
            t(
                f"Davies-Bouldin = {float(metrics['davies_bouldin_score']):.3f}: lower is better.",
                f"Davies-Bouldin = {float(metrics['davies_bouldin_score']):.3f}：越小越好。",
            )
        )
    if metrics.get("calinski_harabasz_score") is not None:
        lines.append(
            t(
                f"Calinski-Harabasz = {float(metrics['calinski_harabasz_score']):.3f}: higher is better.",
                f"Calinski-Harabasz = {float(metrics['calinski_harabasz_score']):.3f}：越大越好。",
            )
        )
    if metrics.get("adjusted_rand_score") is not None:
        lines.append(
            t(
                f"ARI = {float(metrics['adjusted_rand_score']):.3f}: agreement with true labels (when available).",
                f"ARI = {float(metrics['adjusted_rand_score']):.3f}：与真实标签一致性（有标签时）。",
            )
        )
    return "\n".join(lines)


def calculate_feature_importance(model, feature_names=None) -> Tuple[str, str, str]:
    est = unwrap_model(model)
    names = feature_names
    if names is None and isinstance(model, SimpleMLModel):
        names = model.feature_names

    values = None
    method = None

    if hasattr(est, "feature_importances_"):
        values = np.array(est.feature_importances_, dtype=float)
        method = "feature_importances_"
    elif hasattr(est, "coef_"):
        coef = np.array(est.coef_, dtype=float)
        if coef.ndim > 1:
            values = np.mean(np.abs(coef), axis=0)
        else:
            values = np.abs(coef)
        method = "abs(coef_)"
    else:
        raise ValueError(
            t(
                "This model does not support feature importance (no feature_importances_ or coef_).",
                "当前模型不支持特征重要性（无 feature_importances_ 或 coef_）",
            )
        )

    n = len(values)
    if not names or len(names) != n:
        names = [f"feature_{i}" for i in range(n)]

    order = np.argsort(-values)
    tree = [[str(names[i]), float(values[i])] for i in order]
    top = tree[: min(5, len(tree))]

    lines = [
        t("Feature importance notes", "特征重要性解读"),
        "=" * 40,
        t(f"Method: {method}", f"计算方法: {method}"),
        t(
            "Larger values mean stronger influence on the model decision.",
            "数值越大，表示该特征对模型决策影响越大。",
        ),
        "",
        t("Top features:", "Top 特征:"),
    ]
    for name, val in top:
        lines.append(f"  • {name}: {val:.4f}")
    lines.append("")
    lines.append(
        t(
            "Note: importance is model-relative contribution, not causality.",
            "注意: 重要性是模型视角的相对贡献，不等于因果。",
        )
    )

    explanation = "\n".join(lines)
    readme = t(
        "Connect a Model to list features sorted by importance.",
        "将 Model 接入本组件，输出按重要性排序的特征列表。",
    )
    return json.dumps(tree, ensure_ascii=False, separators=(",", ":")), explanation, readme


def calculate_silhouette(X=None, labels=None, dataset=None, model=None) -> Tuple[float, str, str]:
    if dataset is not None:
        X = dataset.get_X()
    if X is None:
        raise ValueError(t("Provide X or Dataset", "必须提供 X 或 Dataset"))
    X = np.array(X)

    if labels is None:
        if model is None:
            raise ValueError(t("Provide labels or model", "必须提供 labels 或 model"))
        predictable = model
        if hasattr(predictable, "predict"):
            labels = predictable.predict(X)
        elif hasattr(unwrap_model(predictable), "labels_"):
            labels = unwrap_model(predictable).labels_
        else:
            raise ValueError(t("Cannot obtain cluster labels from the model", "无法从模型获得聚类标签"))
    labels = np.array(labels).ravel()

    mask = labels >= 0 if np.any(labels < 0) else np.ones(len(labels), dtype=bool)
    labels_use = labels[mask]
    X_use = X[mask]
    if len(np.unique(labels_use)) < 2 or len(labels_use) < 3:
        raise ValueError(
            t(
                "Not enough valid clusters for silhouette (need at least 2).",
                "有效簇数量不足，无法计算轮廓系数（需要至少 2 个簇）",
            )
        )

    score = float(silhouette_score(X_use, labels_use))
    explanation = explain_clustering_metrics({"silhouette_score": score})
    readme = t(
        "Silhouette scores cluster compactness/separation; often used with Elbow Method.",
        "轮廓系数用于评价聚类紧凑度与分离度，通常配合 Elbow Method 选 K。",
    )
    return score, explanation, readme
