"""
数据准备和处理组件
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

from core.data_preprocessing import DataPreprocessor
from components.dataset_components import Dataset, deconstruct_dataset
import numpy as np


def prepare_data(X, y=None, normalize=False, normalize_method='standard',
                remove_outliers=False, handle_missing=False, missing_strategy='mean'):
    """
    数据准备组件
    
    输入:
        X: 特征数据（列表或 numpy 数组）
        y: 标签数据（可选，列表或 numpy 数组）
        normalize: 是否标准化（布尔值）
        normalize_method: 标准化方法 ('standard', 'minmax', 'robust')
        remove_outliers: 是否移除异常值（布尔值）
        handle_missing: 是否处理缺失值（布尔值）
        missing_strategy: 缺失值处理策略 ('mean', 'median', 'most_frequent', 'drop')
    
    输出:
        X_processed: 处理后的特征数据
        y_processed: 处理后的标签数据（如果提供了 y）
    """
    preprocessor = DataPreprocessor()
    
    result = preprocessor.prepare_data(
        X=X,
        y=y,
        normalize=normalize,
        normalize_method=normalize_method,
        remove_outliers=remove_outliers,
        handle_missing=handle_missing,
        missing_strategy=missing_strategy
    )
    
    if y is not None:
        return result[0], result[1]
    else:
        return result


def split_data(X=None, y=None, test_size=0.2, random_state=42, dataset=None):
    """
    数据集划分组件
    
    输入（两种方式）:
        方式1: 分别输入X和y
            X: 特征数据
            y: 标签数据
        方式2: 输入dataset对象（推荐）
            dataset: Dataset对象（包含X和y）
            X和y可以留空
        
        test_size: 测试集比例（0-1之间的浮点数）
        random_state: 随机种子（整数）
    
    输出:
        X_train: 训练集特征
        X_test: 测试集特征
        y_train: 训练集标签
        y_test: 测试集标签
    """
    # 如果提供了dataset，从dataset中提取X和y
    if dataset is not None:
        result = deconstruct_dataset(dataset)
        X, y, has_labels = result[0], result[1], result[2]
        if not has_labels:
            raise ValueError("数据集划分需要标签数据，但dataset中没有标签")
    
    # 检查X和y是否都提供了
    if X is None or y is None:
        raise ValueError("必须提供X和y，或者提供包含X和y的dataset对象")
    
    preprocessor = DataPreprocessor()
    
    X_train, X_test, y_train, y_test = preprocessor.split_data(
        X, y, test_size=test_size, random_state=random_state
    )
    
    return X_train, X_test, y_train, y_test


def normalize_data(X, method='standard'):
    """
    数据标准化组件
    
    输入:
        X: 输入数据
        method: 标准化方法 ('standard', 'minmax', 'robust')
    
    输出:
        X_normalized: 标准化后的数据
    """
    preprocessor = DataPreprocessor()
    return preprocessor.normalize(X, method=method)


def handle_missing_data(X, strategy='mean'):
    """
    处理缺失值组件
    
    输入:
        X: 输入数据
        strategy: 处理策略 ('mean', 'median', 'most_frequent', 'drop')
    
    输出:
        X_processed: 处理后的数据
    """
    preprocessor = DataPreprocessor()
    return preprocessor.handle_missing_values(X, strategy=strategy)
