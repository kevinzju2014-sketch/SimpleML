"""
模型预测组件
用于 Grasshopper Python 组件
"""

import sys
import os
import site

# 添加项目路径
current_dir = os.path.dirname(os.path.abspath(__file__))
project_dir = os.path.dirname(current_dir)
if project_dir not in sys.path:
    sys.path.insert(0, project_dir)

# 确保Rhino Python的site-packages在路径中
# 跨平台引导 Rhino / 系统 site-packages
try:
    from core.env_bootstrap import bootstrap_python_paths
    bootstrap_python_paths(project_dir)
except Exception:
    pass

from core.ml_models import MLModelManager
from core.model_bundle import unwrap_model
from components.dataset_components import Dataset, deconstruct_dataset
import numpy as np


def predict_classifier(model, X=None, dataset=None):
    """
    使用训练好的分类模型进行预测
    
    输入参数:
        model: 训练好的分类模型对象
        X: 待预测的特征数据（Tree结构，可选）
        dataset: Dataset对象（可选），如果提供，将从dataset中提取X
    
    输出参数:
        predictions: 预测结果（Tree结构，每个分支包含一个预测类别）
        probabilities: 预测概率（Tree结构，每个分支包含各类别的概率）
        readme: 组件使用说明
    """
    # 如果提供了dataset，从dataset中提取X
    if dataset is not None:
        if not isinstance(dataset, Dataset):
            raise ValueError("dataset参数必须是Dataset对象")
        X = dataset.get_X()
    elif X is None:
        raise ValueError("必须提供X或dataset参数")
    
    X = np.array(X)
    inner = unwrap_model(model)

    # 兼容 Grasshopper 常见数据组织方式：
    # - 有时每个分支代表“一个特征列”，会导致 X 形状为 (n_features, n_samples)
    # - sklearn 期望 (n_samples, n_features)
    n_features_in = getattr(model, 'n_features_in_', None) or getattr(inner, 'n_features_in_', None)
    if n_features_in is not None and len(X.shape) == 2:
        expected_features = int(n_features_in)
        if X.shape[1] != expected_features and X.shape[0] == expected_features:
            X = X.T

    # 尽量把字符串数字转成 float（例如从 Panel/CSV 来的文本）
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            pass
    
    # 进行预测（SimpleMLModel 会自动套用预处理）
    predictions = model.predict(X)
    
    # 尝试获取预测概率（如果模型支持）
    probabilities = None
    prob_reason = ""
    if hasattr(model, 'predict_proba') or hasattr(inner, 'predict_proba'):
        try:
            # 检查是否是 SVM 且未启用概率预测
            model_type = type(inner).__name__
            if model_type == 'SVC':
                # 检查 probability 属性（SVM 特有）
                if hasattr(inner, 'probability') and not inner.probability:
                    # SVM 未启用概率预测，返回空列表而不是 None
                    probabilities = []
                    prob_reason = "SVM模型未启用概率预测（训练时需要设置probability=True）"
                else:
                    # SVM 已启用概率预测，或其他支持概率的模型
                    probabilities = model.predict_proba(X).tolist()
            else:
                # 其他支持概率的模型
                probabilities = model.predict_proba(X).tolist()
        except Exception as e:
            # 如果 predict_proba 失败，返回空列表
            probabilities = []
            prob_reason = f"模型不支持概率预测或预测失败: {str(e)}"
    else:
        # 模型不支持概率预测，返回空列表
        probabilities = []
        prob_reason = "模型类型不支持概率预测"
    
    if prob_reason:
        readme = f"分类模型预测完成\n\n注意: {prob_reason}"
    else:
        readme = "分类模型预测完成"
    
    return predictions, probabilities, readme


def predict_regressor(model, X=None, dataset=None):
    """
    使用训练好的回归模型进行预测
    
    输入参数:
        model: 训练好的回归模型对象
        X: 待预测的特征数据（Tree结构，可选），可以是列表的列表或numpy数组
        dataset: Dataset对象（可选），如果提供，将从dataset中提取X
    
    输出参数:
        predictions: 预测结果（numpy数组）
        readme: 组件使用说明
    """
    # 如果提供了dataset，从dataset中提取X
    if dataset is not None:
        if not isinstance(dataset, Dataset):
            raise ValueError("dataset参数必须是Dataset对象")
        X = dataset.get_X()
    elif X is None:
        raise ValueError("必须提供X或dataset参数")
    
    # 处理X：确保是2D数组
    # 如果X是列表的列表（Tree结构），直接转换为数组
    # 如果X是一维列表，转换为2D数组
    X = np.array(X)

    # 尽量把字符串数字转成 float（例如从 Panel/CSV 来的文本），避免 KNN 回归在内部对字符串求均值报错
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            raise ValueError("回归模型预测失败：X 中包含非数值内容（例如文本）。请确保特征列都是数值。")
    if len(X.shape) == 1:
        X = X.reshape(-1, 1)
    
    # 进行预测
    predictions = model.predict(X)
    
    readme = "回归模型预测完成"
    
    return predictions, readme


def predict_cluster(model, X=None, dataset=None):
    """
    使用训练好的聚类模型进行预测（分配聚类标签）
    
    输入参数:
        model: 训练好的聚类模型对象
        X: 待预测的特征数据（Tree结构，可选）
        dataset: Dataset对象（可选），如果提供，将从dataset中提取X
    
    输出参数:
        labels: 聚类标签（Tree结构，每个分支包含一个聚类标签）
        readme: 组件使用说明
    """
    # 如果提供了dataset，从dataset中提取X
    if dataset is not None:
        if not isinstance(dataset, Dataset):
            raise ValueError("dataset参数必须是Dataset对象")
        X = dataset.get_X()
    elif X is None:
        raise ValueError("必须提供X或dataset参数")
    
    X = np.array(X)
    
    # 进行预测（聚类）
    labels = model.predict(X) if hasattr(model, 'predict') else model.fit_predict(X)
    
    readme = "聚类模型预测完成"
    
    return labels, readme
