"""
模型打包：将 sklearn 模型与预处理管道一起保存，避免预测时分布偏移。
"""

from __future__ import annotations

from typing import Any, Dict, Optional

import numpy as np


class SimpleMLModel:
    """
    可序列化的模型包装器。
    - model: 底层 sklearn 估计器
    - preprocessor: 可选，具备 transform() 的对象（如 DataPreprocessor）
    - metadata: 算法/类型等元数据
    """

    def __init__(
        self,
        model: Any,
        model_type: str,
        algorithm: str,
        preprocessor: Any = None,
        feature_names=None,
        metadata: Optional[Dict] = None,
    ):
        self.model = model
        self.model_type = model_type
        self.algorithm = algorithm
        self.preprocessor = preprocessor
        self.feature_names = feature_names
        self.metadata = metadata or {}
        # 透传常见 sklearn 属性，兼容旧评估代码
        if hasattr(model, "n_features_in_"):
            self.n_features_in_ = model.n_features_in_

    def transform_features(self, X):
        X = np.array(X)
        if self.preprocessor is not None and hasattr(self.preprocessor, "transform"):
            try:
                if (
                    getattr(self.preprocessor, "scaler", None) is not None
                    or getattr(self.preprocessor, "imputer", None) is not None
                ):
                    return self.preprocessor.transform(X)
            except Exception:
                pass
        return X

    def _maybe_transform(self, X, assume_raw=None):
        """
        assume_raw:
          - True  : 输入为原始特征，需要套用预处理
          - False : 输入已在 Create Dataset 中处理过，不再变换
          - None  : 默认不变换（兼容 Grasshopper 内已处理的 Dataset）
                    对外部原始数据请传 assume_raw=True 或调用 predict_raw
        """
        if assume_raw is True:
            return self.transform_features(X)
        return np.array(X)

    def predict(self, X, assume_raw=None):
        X = self._maybe_transform(X, assume_raw=assume_raw)
        return self.model.predict(X)

    def predict_raw(self, X):
        """对原始（未标准化）特征预测。"""
        return self.predict(X, assume_raw=True)

    def predict_proba(self, X, assume_raw=None):
        if not hasattr(self.model, "predict_proba"):
            raise AttributeError("底层模型不支持 predict_proba")
        X = self._maybe_transform(X, assume_raw=assume_raw)
        return self.model.predict_proba(X)

    def fit_predict(self, X, assume_raw=None):
        if hasattr(self.model, "fit_predict"):
            X = self._maybe_transform(X, assume_raw=assume_raw)
            return self.model.fit_predict(X)
        raise AttributeError("底层模型不支持 fit_predict")

    @property
    def probability(self):
        return getattr(self.model, "probability", None)

    def __repr__(self):
        return (
            f"SimpleMLModel(type={self.model_type}, algorithm={self.algorithm}, "
            f"has_preprocessor={self.preprocessor is not None})"
        )


def unwrap_model(model_or_bundle):
    """取出底层 sklearn 模型（若已是裸模型则原样返回）。"""
    if isinstance(model_or_bundle, SimpleMLModel):
        return model_or_bundle.model
    return model_or_bundle


def as_predictable(model_or_bundle):
    """
    返回可用于 predict 的对象。
    SimpleMLModel 自身可 predict；裸模型直接返回。
    """
    return model_or_bundle


def wrap_trained_model(model, model_type, algorithm, dataset=None, metadata=None):
    """训练完成后打包模型与数据集上的预处理器。"""
    preprocessor = None
    feature_names = None
    if dataset is not None:
        preprocessor = getattr(dataset, "preprocessor", None)
        feature_names = getattr(dataset, "column_names", None)
    return SimpleMLModel(
        model=model,
        model_type=model_type,
        algorithm=algorithm,
        preprocessor=preprocessor,
        feature_names=feature_names,
        metadata=metadata or {},
    )
