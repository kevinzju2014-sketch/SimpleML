"""
一键智能训练：Dataset → Model，自动选择稳妥默认算法。
"""

from __future__ import annotations

import json
from typing import Any, Dict, Optional, Tuple

import numpy as np

from components.dataset_components import Dataset
from components.train_components import train_classifier, train_cluster, train_regressor
from components.ux_helpers import next_steps_for_model, model_card
from core.model_bundle import SimpleMLModel


def _infer_task(dataset: Dataset, task: str) -> str:
    task = (task or "auto").strip().lower()
    if task in ("classification", "classify", "clf", "分类"):
        return "classification"
    if task in ("regression", "regress", "reg", "回归"):
        return "regression"
    if task in ("clustering", "cluster", "clust", "聚类"):
        return "clustering"

    # auto
    if not dataset.has_labels():
        return "clustering"
    y = np.array(dataset.get_y()).ravel()
    # 整数且类别数较少 → 分类；否则回归
    unique = np.unique(y)
    if np.issubdtype(y.dtype, np.integer) or (
        np.issubdtype(y.dtype, np.floating) and np.all(np.equal(np.mod(y, 1), 0))
    ):
        if len(unique) <= max(20, int(0.2 * len(y))):
            return "classification"
    return "regression"


def _default_algorithm(task: str, n_samples: int, n_features: int) -> str:
    if task == "classification":
        if n_samples < 200:
            return "logistic_regression"
        return "random_forest"
    if task == "regression":
        if n_features <= 2 and n_samples < 500:
            return "linear_regression"
        return "random_forest"
    # clustering
    return "kmeans"


def smart_train(
    dataset: Dataset,
    task: str = "auto",
    algorithm: Optional[str] = None,
    n_clusters: int = 3,
) -> Tuple[Any, str, str, str, str, str]:
    """
    返回: model, model_info_json, explanation, readme, next_steps, model_card_text
    """
    if dataset is None or not isinstance(dataset, Dataset):
        raise ValueError("必须提供有效的 Dataset 对象")

    resolved_task = _infer_task(dataset, task)
    X = dataset.get_X()
    n_samples = int(getattr(dataset, "n_samples", len(X)))
    n_features = int(getattr(dataset, "n_features", np.array(X).shape[1] if len(np.array(X).shape) > 1 else 1))

    algo = algorithm
    if not algo or str(algo).strip().lower() in ("auto", "none", ""):
        algo = _default_algorithm(resolved_task, n_samples, n_features)

    # 聚类默认补 n_clusters
    if resolved_task == "clustering" and isinstance(algo, str) and not algo.strip().startswith("{"):
        if algo in ("kmeans", "agglomerative"):
            algo = json.dumps({"algorithm": algo, "n_clusters": int(n_clusters)}, ensure_ascii=False)

    if resolved_task == "classification":
        model, readme, model_info = train_classifier(dataset=dataset, algorithm=algo)
    elif resolved_task == "regression":
        model, readme, model_info = train_regressor(dataset=dataset, algorithm=algo)
    else:
        model, readme, model_info = train_cluster(dataset=dataset, algorithm=algo)

    algo_name = algo
    if isinstance(model, SimpleMLModel):
        algo_name = model.algorithm
        task_name = model.model_type
    else:
        task_name = resolved_task
        if isinstance(algo, str) and algo.strip().startswith("{"):
            try:
                algo_name = json.loads(algo).get("algorithm", algo)
            except Exception:
                pass

    explanation = _build_explanation(task_name, algo_name, n_samples, n_features, model)
    readme = (
        "Smart Train 一键训练完成。\n"
        f"任务: {task_name} | 算法: {algo_name}\n"
        "新手建议: Dataset → Smart Train → 预测 / 评估。\n"
        "进阶调参: 使用 05 Algorithm 参数电池连接到训练组件。"
    )
    steps = next_steps_for_model(model)
    card = model_card(model)
    return model, model_info, explanation, readme, steps, card


def _build_explanation(task: str, algorithm: str, n_samples: int, n_features: int, model) -> str:
    lines = [
        "智能训练说明",
        "=" * 40,
        f"识别任务: {task}",
        f"选用算法: {algorithm}",
        f"样本数: {n_samples}",
        f"特征数: {n_features}",
        "",
        "为什么选它:",
    ]
    tips = {
        "logistic_regression": "小样本分类默认选逻辑回归，训练快、结果稳，适合快速验证。",
        "random_forest": "随机森林对噪声和特征尺度较宽容，适合作为通用默认模型。",
        "linear_regression": "特征少时线性回归直观可解释，便于理解趋势。",
        "kmeans": "无标签数据默认 K-Means；可用 Elbow Method 帮助选择簇数。",
        "agglomerative": "层次聚类适合探索不同粒度的分组结构。",
        "dbscan": "DBSCAN 适合发现不规则形状簇，并自动识别噪声点。",
    }
    lines.append(tips.get(str(algorithm), "已按任务类型选择常用稳妥算法。"))
    if isinstance(model, SimpleMLModel) and model.preprocessor is not None:
        lines.append("")
        lines.append("预处理: 已随模型打包（预测时会自动套用同一变换）。")
    else:
        lines.append("")
        lines.append("预处理: 未检测到 scaler；若训练前做了标准化，请用同一 Create Dataset 设置。")
    lines.append("")
    lines.append("下一步: 连接 Predict 与 Evaluate，查看通俗解读报告。")
    return "\n".join(lines)
