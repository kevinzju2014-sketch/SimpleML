#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Full integration tests against the *installed* SimpleML package
(SIMPLEML_PATH or ~/.config/Grasshopper/Libraries/SimpleML/myML).

This mirrors Grasshopper component call paths without requiring Rhino GUI.
"""

from __future__ import annotations

def _has_any(text, *needles):
    s = text or ""
    return any(n in s for n in needles)


import json
import os
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def _resolve_install_root() -> Path:
    env = os.environ.get("SIMPLEML_PATH")
    candidates = []
    if env:
        candidates.append(Path(env))
    candidates.extend(
        [
            Path.home() / ".config/Grasshopper/Libraries/SimpleML/myML",
            Path.home()
            / "Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/myML",
            Path.home()
            / "Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/myML",
            Path(r"D:/Helio/250928_机器学习课程"),
            ROOT,  # source / synced Helio folder
        ]
    )
    for c in candidates:
        if (c / "components").is_dir() and (c / "core").is_dir():
            return c.resolve()
    raise RuntimeError("No SimpleML root found (need components/ + core/).")


INSTALL = _resolve_install_root()
if str(INSTALL) not in sys.path:
    sys.path.insert(0, str(INSTALL))
os.environ.setdefault("SIMPLEML_PATH", str(INSTALL))


class TestInstalledLayout(unittest.TestCase):
    def test_layout_and_gha(self):
        self.assertTrue((INSTALL / "components").is_dir())
        self.assertTrue((INSTALL / "core").is_dir())
        # Optional: built gha (dev machine / Libraries)
        candidates = [
            INSTALL.parent / "SimpleML.gha",
            ROOT / "GHA_Project/bin/Release/SimpleML.gha",
            ROOT / "GHA_Project/bin/Debug/SimpleML.gha",
        ]
        gha = next((p for p in candidates if p.is_file()), None)
        if gha is None:
            self.skipTest("SimpleML.gha not built yet (ok for source-only sync)")
        self.assertGreater(gha.stat().st_size, 100_000)


class TestHealth(unittest.TestCase):
    def test_health_pass(self):
        from components.health_check import run_health_check

        result = run_health_check(str(INSTALL), auto_fix=False)
        self.assertEqual(result["status"], "PASS", result.get("report"))
        self.assertTrue(_has_any(result["next_steps"], "下一步", "Next"))
        self.assertTrue(result["project_dir"])


class TestClassificationFull(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset

        X, y, fn, _, _ = load_sklearn_dataset("iris", return_X_y=True)
        cls.fn = list(fn) if fn is not None else None
        cls.ds = create_dataset(X, y, X_names=cls.fn)
        cls.train, cls.test = split_dataset(cls.ds, test_size=0.25, random_state=0)

    def test_train_name_json_dict(self):
        from components.train_components import train_classifier
        from core.model_bundle import SimpleMLModel

        m1, _, info1 = train_classifier(self.train, "logistic_regression")
        self.assertIsInstance(m1, SimpleMLModel)
        self.assertTrue(
            "logistic" in info1.lower() or "算法" in info1,
            f"unexpected model info: {info1[:200]}",
        )

        m2, _, _ = train_classifier(
            self.train, '{"algorithm":"knn","n_neighbors":3,"algorithm_type":"auto"}'
        )
        self.assertEqual(m2.model_type, "classification")

        m3, _, _ = train_classifier(
            self.train, {"algorithm": "random_forest", "n_estimators": 30}
        )
        self.assertEqual(m3.model_type, "classification")

    def test_algorithm_batteries_to_train(self):
        from components.algorithms.train_logistic_regression_classifier import (
            train_logistic_regression_classifier,
        )
        from components.algorithms.train_random_forest_classifier import (
            train_random_forest_classifier,
        )
        from components.train_components import train_classifier

        params, _ = train_logistic_regression_classifier()
        self.assertIn("algorithm", json.loads(params))
        m, _, _ = train_classifier(self.train, params)
        self.assertEqual(m.model_type, "classification")

        params2, _ = train_random_forest_classifier(n_estimators=25)
        m2, _, _ = train_classifier(self.train, params2)
        self.assertEqual(m2.model_type, "classification")

    def test_smart_predict_evaluate_importance_io(self):
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto, model_card
        from components.explain_components import calculate_feature_importance
        from components.model_io_components import save_model, load_model
        from components.predict_components import predict_classifier
        from components.evaluate_components import evaluate_classification

        model, info, expl, readme, steps, card = smart_train(
            self.train, task="classification"
        )
        self.assertEqual(len((model, info, expl, readme, steps, card)), 6)
        self.assertTrue(_has_any(evaluate_auto(model, dataset=self.test)[2] or "", "结论", "Verdict"))

        preds, proba, _, task = predict_auto(model, dataset=self.test)
        self.assertEqual(task, "classification")
        self.assertEqual(len(preds), self.test.n_samples)

        p2, _, _ = predict_classifier(model, dataset=self.test)
        self.assertEqual(len(p2), len(preds))

        metrics, report, cm, _ = evaluate_classification(
            model, X=self.test.X, y_true=self.test.y
        )
        self.assertIn("accuracy", metrics.lower())
        self.assertTrue(report)

        fi, fi_exp, _ = calculate_feature_importance(model, self.fn)
        self.assertTrue("petal" in fi.lower() or "sepal" in fi.lower() or "feature" in fi.lower())
        self.assertTrue(_has_any(fi_exp, "特征", "Feature", "Top"))

        self.assertTrue(_has_any(model_card(model), "模型", "Model"))

        with tempfile.TemporaryDirectory() as td:
            path = os.path.join(td, "iris_model.pkl")
            save_model(model, path, model_type=model.model_type, algorithm=model.algorithm)
            loaded, meta = load_model(path)
            preds2, _, _, _ = predict_auto(loaded, dataset=self.test)
            self.assertEqual(len(preds2), len(preds))


class TestRegressionFull(unittest.TestCase):
    def test_diabetes_pipeline(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto
        from components.train_components import train_regressor
        from components.algorithms.train_ridge_regression import train_ridge_regression

        X, y, fn, _, _ = load_sklearn_dataset("diabetes", return_X_y=True)
        ds = create_dataset(X, y, X_names=list(fn) if fn is not None else None)
        train, test = split_dataset(ds, test_size=0.25, random_state=0)

        model, *_ = smart_train(train, task="regression")
        self.assertEqual(model.model_type, "regression")
        preds, _, _, task = predict_auto(model, dataset=test)
        self.assertEqual(task, "regression")
        self.assertEqual(len(preds), test.n_samples)
        metrics, report, verdict, _, _ = evaluate_auto(model, dataset=test)
        self.assertTrue("r2" in metrics.lower() or "mse" in metrics.lower())
        self.assertTrue(_has_any(verdict, "结论", "Verdict"))

        params, _ = train_ridge_regression()
        m2, _, _ = train_regressor(train, params)
        self.assertEqual(m2.model_type, "regression")


class TestClusteringFull(unittest.TestCase):
    def test_blobs_pipeline(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto
        from components.explain_components import calculate_silhouette
        from components.visualization_components import (
            reduce_dimensions,
            calculate_elbow_score,
        )
        from components.train_components import train_cluster

        X, y, _, _, _ = load_sklearn_dataset("make_blobs", return_X_y=True)
        ds = create_dataset(X, None)
        model, *_ = smart_train(ds, task="clustering", n_clusters=3)
        self.assertEqual(model.model_type, "clustering")
        labels, _, _, task = predict_auto(model, dataset=ds)
        self.assertEqual(task, "clustering")
        self.assertEqual(len(labels), ds.n_samples)
        metrics, report, verdict, _, _ = evaluate_auto(model, dataset=ds)
        self.assertTrue(_has_any(verdict, "结论", "Verdict"))

        score, exp, _ = calculate_silhouette(dataset=ds, model=model)
        self.assertGreater(float(score), 0.2)

        coords, _, _ = reduce_dimensions(X, method="pca", n_components=2)
        self.assertTrue(len(coords) > 0)
        ks, scores, _ = calculate_elbow_score(X, max_clusters=5)
        self.assertTrue(len(ks) >= 2)

        m2, _, _ = train_cluster(ds, '{"algorithm":"kmeans","n_clusters":3}')
        self.assertEqual(m2.model_type, "clustering")


class TestPreprocessorBundle(unittest.TestCase):
    def test_no_double_scale_default_predict(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset
        from components.train_components import train_classifier
        from core.data_preprocessing import DataPreprocessor
        import numpy as np

        X, y, fn, _, _ = load_sklearn_dataset("iris", return_X_y=True)
        prep = DataPreprocessor()
        Xs, ys = prep.prepare_data(X, y, normalize=True, normalize_method="standard")
        ds = create_dataset(Xs, ys, X_names=list(fn) if fn is not None else None)
        ds.set_preprocessor(prep, {"normalize": True, "normalize_method": "standard"})
        train, test = split_dataset(ds, test_size=0.25, random_state=0)
        model, _, _ = train_classifier(train, "logistic_regression")
        self.assertIsNotNone(model.preprocessor)
        # Default predict assumes already-processed GH dataset features
        p1 = model.predict(test.X[:5])
        self.assertEqual(len(p1), 5)
        # Raw path still works
        raw = np.asarray(X[:5], dtype=float)
        p2 = model.predict_raw(raw)
        self.assertEqual(len(p2), 5)


class TestFileIOAndWizard(unittest.TestCase):
    def test_csv_roundtrip_and_wizard(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.file_io_components import write_csv, read_csv
        from components.ux_helpers import wizard_recipe
        import numpy as np
        import pandas as pd

        from components import file_io_components as fio

        X, y, _, _, _ = load_sklearn_dataset("iris", return_X_y=True)
        df = pd.DataFrame(np.asarray(X)[:, :2], columns=["a", "b"])
        with tempfile.TemporaryDirectory() as td:
            path = os.path.join(td, "t.csv")
            fio.write_csv(df, path)
            out = fio.read_csv(path)
            data = out[0] if isinstance(out, tuple) else out
            self.assertTrue(len(data) > 0)

        for task in ("classification", "regression", "clustering"):
            text = wizard_recipe(task)
            self.assertTrue(
                _has_any(text, "配方", "Recipe", "环境体检", "Health Check", "智能训练", "Smart Train"),
                f"wizard recipe too short/empty for {task}: {text[:120]}",
            )


class TestNegativeCases(unittest.TestCase):
    def test_invalid_algorithm(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset
        from components.train_components import train_classifier

        X, y, _, _, _ = load_sklearn_dataset("iris", return_X_y=True)
        ds = create_dataset(X, y)
        train, _ = split_dataset(ds, 0.2, 0)
        with self.assertRaises(Exception):
            train_classifier(train, "no_such_algo_xyz")

    def test_split_unlabeled_fails(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset

        X, _, _, _, _ = load_sklearn_dataset("make_blobs", return_X_y=True)
        ds = create_dataset(X, None)
        with self.assertRaises(Exception):
            split_dataset(ds, 0.2, 0)


def main():
    print("=" * 60)
    print("SimpleML INSTALLED package tests")
    print("INSTALL =", INSTALL)
    print("=" * 60)
    suite = unittest.defaultTestLoader.loadTestsFromModule(sys.modules[__name__])
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    print("=" * 60)
    print(
        f"Ran {result.testsRun} | failures={len(result.failures)} "
        f"errors={len(result.errors)} skipped={len(result.skipped)}"
    )
    if result.wasSuccessful():
        print("ALL INSTALLED TESTS PASSED")
        return 0
    print("INSTALLED TESTS FAILED")
    return 1


if __name__ == "__main__":
    sys.exit(main())
