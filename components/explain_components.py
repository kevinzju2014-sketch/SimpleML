"""
可解释性与通俗结果解读组件。
"""

from __future__ import annotations

import json
from typing import Any, List, Tuple

import numpy as np
from sklearn.metrics import silhouette_score

from core.model_bundle import SimpleMLModel, unwrap_model


def explain_classification_metrics(metrics: dict) -> str:
    acc = metrics.get("accuracy")
    lines = ["通俗解读（分类）", "=" * 40]
    if acc is not None:
        pct = float(acc) * 100
        lines.append(f"准确率 {pct:.1f}%：大约每 100 次预测，对了 {pct:.0f} 次。")
        if acc >= 0.9:
            lines.append("表现：很强。可以进入业务试用，但仍建议检查混淆矩阵是否偏科。")
        elif acc >= 0.7:
            lines.append("表现：可用。若某类别很少，请同时看精确率/召回率。")
        else:
            lines.append("表现：偏弱。可尝试更多数据、特征工程或换算法（如随机森林）。")
    if metrics.get("precision") is not None:
        lines.append(f"精确率 {float(metrics['precision'])*100:.1f}%：模型说“是”时，真的是的比例。")
    if metrics.get("recall") is not None:
        lines.append(f"召回率 {float(metrics['recall'])*100:.1f}%：真实“是”里，被找出来的比例。")
    if metrics.get("f1_score") is not None:
        lines.append(f"F1 {float(metrics['f1_score']):.3f}：精确率与召回率的平衡分，越接近 1 越好。")
    return "\n".join(lines)


def explain_regression_metrics(metrics: dict) -> str:
    lines = ["通俗解读（回归）", "=" * 40]
    r2 = metrics.get("r2_score")
    rmse = metrics.get("rmse")
    mae = metrics.get("mae")
    if r2 is not None:
        lines.append(f"R² = {float(r2):.3f}：模型解释了大约 {float(r2)*100:.1f}% 的目标变化。")
        if r2 >= 0.9:
            lines.append("表现：拟合很好。")
        elif r2 >= 0.7:
            lines.append("表现：较好，可作为实用基线。")
        elif r2 >= 0.3:
            lines.append("表现：一般，还有较大改进空间。")
        else:
            lines.append("表现：偏弱，建议检查特征是否相关、是否需要非线性模型。")
    if rmse is not None:
        lines.append(f"RMSE = {float(rmse):.4f}：典型误差量级（与目标同单位）。")
    if mae is not None:
        lines.append(f"MAE = {float(mae):.4f}：平均绝对误差，对异常值更稳健。")
    return "\n".join(lines)


def explain_clustering_metrics(metrics: dict) -> str:
    lines = ["通俗解读（聚类）", "=" * 40]
    sil = metrics.get("silhouette_score")
    if sil is not None:
        s = float(sil)
        lines.append(f"轮廓系数 = {s:.3f}（范围约 -1~1，越高簇越紧凑且分离）。")
        if s >= 0.5:
            lines.append("表现：簇结构比较清晰。")
        elif s >= 0.25:
            lines.append("表现：有一定结构，可尝试调整簇数或特征缩放。")
        else:
            lines.append("表现：簇边界较弱，建议用 Elbow Method 换 K，或改 DBSCAN。")
    if metrics.get("davies_bouldin_score") is not None:
        lines.append(f"Davies-Bouldin = {float(metrics['davies_bouldin_score']):.3f}：越小越好。")
    if metrics.get("calinski_harabasz_score") is not None:
        lines.append(f"Calinski-Harabasz = {float(metrics['calinski_harabasz_score']):.3f}：越大越好。")
    if metrics.get("adjusted_rand_score") is not None:
        lines.append(f"ARI = {float(metrics['adjusted_rand_score']):.3f}：与真实标签一致性（有标签时）。")
    return "\n".join(lines)


def calculate_feature_importance(model, feature_names=None) -> Tuple[str, str, str]:
    """
    返回: importance_tree_json, explanation, readme
    """
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
        raise ValueError("当前模型不支持特征重要性（无 feature_importances_ 或 coef_）")

    n = len(values)
    if not names or len(names) != n:
        names = [f"feature_{i}" for i in range(n)]

    order = np.argsort(-values)
    tree = [[str(names[i]), float(values[i])] for i in order]
    top = tree[: min(5, len(tree))]

    lines = [
        "特征重要性解读",
        "=" * 40,
        f"计算方法: {method}",
        "数值越大，表示该特征对模型决策影响越大。",
        "",
        "Top 特征:",
    ]
    for name, val in top:
        lines.append(f"  • {name}: {val:.4f}")
    lines.append("")
    lines.append("注意: 重要性是模型视角的相对贡献，不等于因果。")

    explanation = "\n".join(lines)
    readme = "将 Model 接入本组件，输出按重要性排序的特征列表。"
    return json.dumps(tree, ensure_ascii=False, separators=(",", ":")), explanation, readme


def calculate_silhouette(X=None, labels=None, dataset=None, model=None) -> Tuple[float, str, str]:
    """
    计算轮廓系数。
    可用 labels，或用 model 对 X/dataset 预测得到标签。
    """
    if dataset is not None:
        X = dataset.get_X()
    if X is None:
        raise ValueError("必须提供 X 或 Dataset")
    X = np.array(X)

    if labels is None:
        if model is None:
            raise ValueError("必须提供 labels 或 model")
        predictable = model
        if hasattr(predictable, "predict"):
            labels = predictable.predict(X)
        elif hasattr(unwrap_model(predictable), "labels_"):
            labels = unwrap_model(predictable).labels_
        else:
            raise ValueError("无法从模型获得聚类标签")
    labels = np.array(labels).ravel()

    unique = np.unique(labels)
    # DBSCAN 噪声 -1 需排除；至少 2 个簇
    mask = labels >= 0 if np.any(labels < 0) else np.ones(len(labels), dtype=bool)
    labels_use = labels[mask]
    X_use = X[mask]
    if len(np.unique(labels_use)) < 2 or len(labels_use) < 3:
        raise ValueError("有效簇数量不足，无法计算轮廓系数（需要至少 2 个簇）")

    score = float(silhouette_score(X_use, labels_use))
    explanation = explain_clustering_metrics({"silhouette_score": score})
    readme = "轮廓系数用于评价聚类紧凑度与分离度，通常配合 Elbow Method 选 K。"
    return score, explanation, readme
