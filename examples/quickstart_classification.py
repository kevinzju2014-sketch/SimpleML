"""
可在系统 Python 中运行的分类快通脚本（不依赖 Grasshopper）。
用于验证 SimpleML 核心链路。
"""

import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, ROOT)

from components.dataset_loader import load_sklearn_dataset
from components.dataset_components import create_dataset, split_dataset
from components.smart_train import smart_train
from components.evaluate_components import evaluate_classification
from components.explain_components import calculate_feature_importance


def main():
    X, y, feature_names, target_names, info = load_sklearn_dataset("iris", return_X_y=True)
    print(info)

    ds = create_dataset(X, y, X_names=list(feature_names) if feature_names is not None else None)
    train_ds, test_ds = split_dataset(ds, test_size=0.2, random_state=42)
    model, _info, explanation, _readme, next_steps, card = smart_train(train_ds, task="classification")
    _metrics_json, report, _cm, _ = evaluate_classification(
        model, X=test_ds.get_X(), y_true=test_ds.get_y()
    )
    _fi_json, fi_exp, _ = calculate_feature_importance(
        model, list(feature_names) if feature_names is not None else None
    )

    print("=== Smart Train ===")
    print(explanation)
    print(card)
    print(next_steps)
    print("\n=== Evaluation (excerpt) ===")
    print(report[-900:] if len(report) > 900 else report)
    print("\n=== Feature Importance ===")
    print(fi_exp)
    print("\nOK: classification quickstart passed")


if __name__ == "__main__":
    main()
