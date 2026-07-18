"""
SimpleML 核心自动化测试（不依赖 Rhino / Grasshopper）

运行：
  python3 tests/test_simpleml.py
  # 或
  python3 -m pytest tests/test_simpleml.py -q
"""

from __future__ import annotations

import os
import sys
import unittest

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if ROOT not in sys.path:
    sys.path.insert(0, ROOT)


class TestEnvAndHealth(unittest.TestCase):
    def test_bootstrap_and_health_pass(self):
        from core.env_bootstrap import bootstrap_python_paths, ensure_project_on_path
        from components.health_check import run_health_check

        added = bootstrap_python_paths(ROOT)
        self.assertTrue(len(added) >= 1)
        self.assertEqual(ensure_project_on_path(ROOT), ROOT)

        result = run_health_check(ROOT, auto_fix=False)
        self.assertEqual(result["status"], "PASS", result.get("report"))
        self.assertIn("下一步", result["next_steps"])
        self.assertTrue(os.path.isdir(result["project_dir"]))


class TestClassificationPipeline(unittest.TestCase):
    def test_iris_smart_train_predict_evaluate(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto, model_card, next_steps_for_model
        from components.explain_components import calculate_feature_importance
        from core.model_bundle import SimpleMLModel

        X, y, feature_names, _, info = load_sklearn_dataset("iris", return_X_y=True)
        self.assertIn("鸢尾", info)

        ds = create_dataset(X, y, X_names=list(feature_names))
        train_ds, test_ds = split_dataset(ds, test_size=0.2, random_state=42)

        model, model_info, explanation, readme, steps, card = smart_train(
            train_ds, task="classification"
        )
        self.assertIsInstance(model, SimpleMLModel)
        self.assertEqual(model.model_type, "classification")
        self.assertIn("下一步", steps)
        self.assertIn("模型卡片", card)
        self.assertIn("算法", explanation)

        preds, proba, _, task = predict_auto(model, dataset=test_ds)
        self.assertEqual(task, "classification")
        self.assertEqual(len(preds), test_ds.n_samples)

        metrics_json, report, verdict, _, cm = evaluate_auto(model, dataset=test_ds)
        self.assertIn("accuracy", metrics_json)
        self.assertIn("结论", verdict)
        self.assertIn("结论", report)

        fi_json, fi_exp, _ = calculate_feature_importance(model, list(feature_names))
        self.assertTrue(
            "petal" in fi_json.lower() or "sepal" in fi_json.lower(),
            f"feature importance should mention iris features, got: {fi_json[:200]}",
        )
        self.assertIn("特征重要性", fi_exp)

        self.assertIn("classification", next_steps_for_model(model))
        self.assertIn("模型卡片", model_card(model))


class TestRegressionPipeline(unittest.TestCase):
    def test_diabetes_smart_train(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset, split_dataset
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto

        X, y, feature_names, _, _ = load_sklearn_dataset("diabetes", return_X_y=True)
        ds = create_dataset(X, y, X_names=list(feature_names) if feature_names is not None else None)
        train_ds, test_ds = split_dataset(ds, test_size=0.25, random_state=0)

        model, *_ = smart_train(train_ds, task="regression")
        self.assertEqual(model.model_type, "regression")

        preds, _, _, task = predict_auto(model, dataset=test_ds)
        self.assertEqual(task, "regression")
        self.assertEqual(len(preds), test_ds.n_samples)

        _, report, verdict, _, _ = evaluate_auto(model, dataset=test_ds)
        self.assertIn("结论", verdict)
        self.assertTrue("R" in verdict or "R²" in verdict or "R2" in verdict or "r2" in report.lower() or "结论" in verdict)


class TestClusteringPipeline(unittest.TestCase):
    def test_blobs_cluster_and_silhouette(self):
        from components.dataset_loader import load_sklearn_dataset
        from components.dataset_components import create_dataset
        from components.smart_train import smart_train
        from components.ux_helpers import predict_auto, evaluate_auto
        from components.explain_components import calculate_silhouette

        X, y, _, _, _ = load_sklearn_dataset("make_blobs", return_X_y=True)
        ds = create_dataset(X, None)
        model, *_ = smart_train(ds, task="clustering", n_clusters=3)
        self.assertEqual(model.model_type, "clustering")

        labels, _, _, task = predict_auto(model, dataset=ds)
        self.assertEqual(task, "clustering")
        self.assertEqual(len(labels), ds.n_samples)

        _, _, verdict, _, _ = evaluate_auto(model, dataset=ds)
        self.assertIn("结论", verdict)

        score, explanation, _ = calculate_silhouette(dataset=ds, model=model)
        self.assertGreater(score, 0.2)
        self.assertIn("轮廓", explanation)


class TestPreprocessorBundle(unittest.TestCase):
    def test_scaler_packed_with_model(self):
        import numpy as np
        from sklearn.datasets import make_classification
        from core.data_preprocessing import DataPreprocessor
        from components.dataset_components import create_dataset, split_dataset
        from components.smart_train import smart_train
        from core.model_bundle import SimpleMLModel

        X, y = make_classification(n_samples=80, n_features=4, random_state=42)
        prep = DataPreprocessor()
        Xs = prep.normalize(X, "standard")
        ds = create_dataset(Xs, y)
        ds.set_preprocessor(prep, {"normalize": True})
        train_ds, _ = split_dataset(ds, test_size=0.25, random_state=1)

        model, *_ = smart_train(train_ds, task="classification")
        self.assertIsInstance(model, SimpleMLModel)
        self.assertIsNotNone(model.preprocessor)

        # 已处理数据：默认 predict 不再二次变换
        pred = model.predict(train_ds.get_X()[:5])
        self.assertEqual(len(pred), 5)
        # 原始数据：predict_raw 会套用 scaler
        pred_raw = model.predict_raw(X[:5])
        self.assertEqual(len(pred_raw), 5)


class TestWizardAndDocsPresence(unittest.TestCase):
    def test_wizard_recipes(self):
        from components.ux_helpers import wizard_recipe

        for task in ("classification", "regression", "clustering"):
            text = wizard_recipe(task)
            self.assertIn("配方", text)
            self.assertGreater(len(text), 40)

    def test_docs_exist(self):
        for rel in (
            "docs/INSTALL_GUIDE.md",
            "docs/USER_GUIDE.md",
            "docs/COMPONENT_REFERENCE.md",
            "docs/FOR_RHINO_PC.md",
            "examples/01_classification_iris.md",
            "install.bat",
            "install.sh",
            "FOOD4RHINO.md",
            "requirements.txt",
        ):
            path = os.path.join(ROOT, rel)
            self.assertTrue(os.path.isfile(path), f"missing {rel}")


def main():
    print("=" * 60)
    print("SimpleML test suite")
    print("ROOT =", ROOT)
    print("=" * 60)
    suite = unittest.defaultTestLoader.loadTestsFromModule(sys.modules[__name__])
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    print("=" * 60)
    print(
        f"Ran {result.testsRun} tests | "
        f"failures={len(result.failures)} errors={len(result.errors)} skipped={len(result.skipped)}"
    )
    if result.wasSuccessful():
        print("ALL TESTS PASSED")
        return 0
    print("TESTS FAILED")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
