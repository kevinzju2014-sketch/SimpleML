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
try:
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    # 添加Rhino Python的site-envs虚拟环境路径
    rhino_site_envs = r'C:\Users\Administrator\.rhinocode\py39-rh8\site-envs'
    if os.path.exists(rhino_site_envs):
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception:
    pass

from core.ml_models import MLModelManager
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

    # 兼容 Grasshopper 常见数据组织方式：
    # - 有时每个分支代表“一个特征列”，会导致 X 形状为 (n_features, n_samples)
    # - sklearn 期望 (n_samples, n_features)
    if hasattr(model, 'n_features_in_') and len(X.shape) == 2:
        expected_features = int(getattr(model, 'n_features_in_', X.shape[1]))
        if X.shape[1] != expected_features and X.shape[0] == expected_features:
            X = X.T

    # 尽量把字符串数字转成 float（例如从 Panel/CSV 来的文本）
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            pass
    
    # 进行预测
    predictions = model.predict(X)
    
    # 尝试获取预测概率（如果模型支持）
    probabilities = None
    prob_reason = ""
    if hasattr(model, 'predict_proba'):
        try:
            # 检查是否是 SVM 且未启用概率预测
            model_type = type(model).__name__
            if model_type == 'SVC':
                # 检查 probability 属性（SVM 特有）
                if hasattr(model, 'probability') and not model.probability:
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
