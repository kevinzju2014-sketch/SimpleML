"""
上架向用户体验辅助：结论句、下一步、向导配方、统一预测/评估。
"""

from __future__ import annotations

import json
from typing import Any, Dict, Optional, Tuple

import numpy as np

from core.model_bundle import SimpleMLModel, unwrap_model
from components.dataset_components import Dataset
from components.predict_components import predict_classifier, predict_regressor, predict_cluster
from components.explain_components import (
    verdict_classification,
    verdict_regression,
    verdict_clustering,
)


def next_steps_for_model(model) -> str:
    task = "classification"
    algo = "unknown"
    if isinstance(model, SimpleMLModel):
        task = model.model_type or task
        algo = model.algorithm or algo
    mapping = {
        "classification": (
            "下一步:\n"
            "1) 用「预测 Predict」或「预测分类」接 Model + 测试 Dataset/X\n"
            "2) 用「评估 Evaluate」或「评估分类」接 Model + 测试 Dataset\n"
            "3) 可选：「特征重要性」「可视化分类标签」"
        ),
        "regression": (
            "下一步:\n"
            "1) 用「预测」或「预测回归」\n"
            "2) 用「评估」或「评估回归」\n"
            "3) 可选：「可视化回归」"
        ),
        "clustering": (
            "下一步:\n"
            "1) 用「预测」或「预测聚类」\n"
            "2) 用「评估」或「评估聚类」/「轮廓系数」\n"
            "3) 可选：「可视化聚类标签」或「一键聚类上色」"
        ),
    }
    header = f"模型类型: {task} | 算法: {algo}\n"
    return header + mapping.get(task, mapping["classification"])


def model_card(model) -> str:
    lines = ["模型卡片", "=" * 32]
    if isinstance(model, SimpleMLModel):
        lines.append(f"任务: {model.model_type}")
        lines.append(f"算法: {model.algorithm}")
        lines.append(f"预处理打包: {'是' if model.preprocessor is not None else '否'}")
        if model.feature_names:
            lines.append(f"特征名: {', '.join(map(str, model.feature_names[:12]))}")
        nfi = getattr(model, "n_features_in_", None) or getattr(model.model, "n_features_in_", None)
        if nfi is not None:
            lines.append(f"特征数: {nfi}")
        lines.append(f"底层: {type(unwrap_model(model)).__name__}")
    else:
        lines.append(f"任务: 未知（裸模型）")
        lines.append(f"底层: {type(model).__name__}")
        if hasattr(model, "n_features_in_"):
            lines.append(f"特征数: {model.n_features_in_}")
    return "\n".join(lines)


def _resolve_xy(model, X=None, dataset=None, need_y=False):
    y = None
    if dataset is not None:
        if not isinstance(dataset, Dataset):
            raise ValueError("dataset 必须是 Dataset 对象")
        X = dataset.get_X()
        if need_y:
            y = dataset.get_y()
            if y is None:
                raise ValueError("评估需要带标签的 Dataset（测试集）")
    if X is None:
        raise ValueError("请提供 Dataset 或 X")
    return np.array(X), (None if y is None else np.array(y).ravel())


def detect_task(model) -> str:
    if isinstance(model, SimpleMLModel) and model.model_type:
        return model.model_type
    name = type(unwrap_model(model)).__name__.lower()
    if any(k in name for k in ("classif", "svc", "logistic", "forestclassifier", "nb", "tree")):
        return "classification"
    if any(k in name for k in ("regress", "svr", "lasso", "ridge", "elastic")):
        return "regression"
    if any(k in name for k in ("kmeans", "dbscan", "agglomerative", "cluster")):
        return "clustering"
    # fallback: has predict_proba → classif
    if hasattr(unwrap_model(model), "predict_proba"):
        return "classification"
    return "classification"


def predict_auto(model, X=None, dataset=None) -> Tuple[Any, Any, str, str]:
    task = detect_task(model)
    if task == "regression":
        preds, readme = predict_regressor(model, X=X, dataset=dataset)
        return preds, None, readme, task
    if task == "clustering":
        labels, readme = predict_cluster(model, X=X, dataset=dataset)
        return labels, None, readme, task
    preds, proba, readme = predict_classifier(model, X=X, dataset=dataset)
    return preds, proba, readme, task


def evaluate_auto(model, dataset=None, X=None, y_true=None) -> Tuple[str, str, str, str, Any]:
    """
    返回: metrics_json, report, verdict, readme, extra(混淆矩阵或 None)
    延迟导入 evaluate_*，避免与 evaluate_components 循环依赖。
    """
    from components.evaluate_components import (
        evaluate_classification,
        evaluate_regression,
        evaluate_clustering,
    )

    task = detect_task(model)
    if dataset is not None:
        X, y_true = _resolve_xy(model, dataset=dataset, need_y=(task != "clustering"))
        if task == "clustering" and y_true is None:
            # 允许无标签聚类评估
            X = dataset.get_X()
            y_true = dataset.get_y()

    if task == "regression":
        metrics_json, report, readme = evaluate_regression(model, X=X, y_true=y_true)
        metrics = json.loads(metrics_json)
        verdict = verdict_regression(metrics)
        report = report + "\n\n" + verdict
        return metrics_json, report, verdict, readme, None
    if task == "clustering":
        metrics_json, report, readme = evaluate_clustering(model, X=X, y_true=y_true)
        metrics = json.loads(metrics_json)
        verdict = verdict_clustering(metrics)
        report = report + "\n\n" + verdict
        return metrics_json, report, verdict, readme, None

    metrics_json, report, cm, readme = evaluate_classification(model, X=X, y_true=y_true)
    metrics = json.loads(metrics_json)
    verdict = verdict_classification(metrics)
    report = report + "\n\n" + verdict
    return metrics_json, report, verdict, readme, cm


WIZARD_RECIPES = {
    "classification": """【配方】鸢尾花分类（约 5 分钟）
1. 环境体检 → Run=true → 确认 PASS
2. 加载示例数据集 → DN=iris → 接「Dataset」输出
3. 分割数据集 → 得到 Train / Test
4. 智能训练 → Dataset=Train, Task=classification
5. 预测 → Model + Test Dataset
6. 评估 → Model + Test Dataset → 看 Verdict
7. （可选）特征重要性

搜索组件名即可：环境体检 / 加载示例数据集 / 分割数据集 / 智能训练 / 预测 / 评估
""",
    "clustering": """【配方】聚类 + 上色
1. 环境体检 PASS
2. 加载示例数据集 → make_blobs（或自有点）
3. 智能训练 → Task=clustering, K=3
4. 一键聚类上色 → Points + Model（或 Labels）
   或：预测聚类 → 可视化聚类标签
5. 轮廓系数 / 评估 查看 Verdict
""",
    "regression": """【配方】回归
1. 环境体检 PASS
2. 加载示例数据集 → diabetes
3. 分割数据集
4. 智能训练 → Task=regression
5. 预测 + 评估 → 看 R² Verdict
6. （可选）可视化回归
""",
}


def wizard_recipe(task: str = "classification") -> str:
    key = (task or "classification").strip().lower()
    if key in ("classify", "clf", "分类"):
        key = "classification"
    if key in ("regress", "reg", "回归"):
        key = "regression"
    if key in ("cluster", "clust", "聚类"):
        key = "clustering"
    return WIZARD_RECIPES.get(key, WIZARD_RECIPES["classification"])
