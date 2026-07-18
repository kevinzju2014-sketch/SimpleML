"""
三条任务链路快通（分类 / 回归 / 聚类），不依赖 Grasshopper。
"""

import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, ROOT)


def run_classification():
    from components.dataset_loader import load_sklearn_dataset
    from components.dataset_components import create_dataset, split_dataset
    from components.smart_train import smart_train
    from components.ux_helpers import predict_auto, evaluate_auto

    X, y, names, _, info = load_sklearn_dataset("iris", return_X_y=True)
    ds = create_dataset(X, y, X_names=list(names))
    tr, te = split_dataset(ds, test_size=0.2, random_state=42)
    model, *_ = smart_train(tr, task="classification")
    predict_auto(model, dataset=te)
    _, _, verdict, _, _ = evaluate_auto(model, dataset=te)
    print("[classification]", info.split("：")[0], "|", verdict)


def run_regression():
    from components.dataset_loader import load_sklearn_dataset
    from components.dataset_components import create_dataset, split_dataset
    from components.smart_train import smart_train
    from components.ux_helpers import evaluate_auto

    X, y, names, _, info = load_sklearn_dataset("diabetes", return_X_y=True)
    ds = create_dataset(X, y, X_names=list(names) if names is not None else None)
    tr, te = split_dataset(ds, test_size=0.25, random_state=0)
    model, *_ = smart_train(tr, task="regression")
    _, _, verdict, _, _ = evaluate_auto(model, dataset=te)
    print("[regression]", info.split("：")[0], "|", verdict)


def run_clustering():
    from components.dataset_loader import load_sklearn_dataset
    from components.dataset_components import create_dataset
    from components.smart_train import smart_train
    from components.ux_helpers import evaluate_auto

    X, y, _, _, info = load_sklearn_dataset("make_blobs", return_X_y=True)
    ds = create_dataset(X, None)
    model, *_ = smart_train(ds, task="clustering", n_clusters=3)
    _, _, verdict, _, _ = evaluate_auto(model, dataset=ds)
    print("[clustering]", info.split("：")[0], "|", verdict)


def main():
    run_classification()
    run_regression()
    run_clustering()
    print("OK: quickstart_all passed")


if __name__ == "__main__":
    main()
