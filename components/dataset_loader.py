"""
数据集加载组件
用于加载scikit-learn内置数据集
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

import numpy as np
import pandas as pd
from sklearn import datasets


def load_sklearn_dataset(dataset_name='iris', return_X_y=False):
    """
    加载scikit-learn内置数据集
    
    输入:
        dataset_name: 数据集名称（字符串）
            可选值:
            - 'iris': 鸢尾花数据集（分类，150样本，4特征）
            - 'wine': 葡萄酒数据集（分类，178样本，13特征）
            - 'breast_cancer': 乳腺癌数据集（分类，569样本，30特征）
            - 'digits': 手写数字数据集（分类，1797样本，64特征）
            - 'diabetes': 糖尿病数据集（回归，442样本，10特征）
            - 'boston': 波士顿房价数据集（回归，506样本，13特征，已弃用）
            - 'california_housing': 加州房价数据集（回归，20640样本，8特征）
            - 'linnerud': Linnerud数据集（多输出回归，20样本，3特征）
            - 'make_classification': 生成分类数据集（可配置参数）
            - 'make_regression': 生成回归数据集（可配置参数）
            - 'make_blobs': 生成聚类数据集（可配置参数）
        return_X_y: 是否返回X和y分开，默认False（返回DataFrame）
    
    输出:
        如果return_X_y=False:
            data: 包含特征和标签的DataFrame
            columns: 列名列表
            shape: 数据形状
            info: 数据集信息
        如果return_X_y=True:
            X: 特征数据（numpy数组）
            y: 标签数据（numpy数组）
            feature_names: 特征名称列表
            target_names: 标签名称列表（如果有）
            info: 数据集信息
    """
    dataset_name = dataset_name.lower()
    
    # 加载数据集
    if dataset_name == 'iris':
        data_obj = datasets.load_iris()
        info = "鸢尾花数据集（分类）：150个样本，4个特征，3个类别"
    elif dataset_name == 'wine':
        data_obj = datasets.load_wine()
        info = "葡萄酒数据集（分类）：178个样本，13个特征，3个类别"
    elif dataset_name == 'breast_cancer':
        data_obj = datasets.load_breast_cancer()
        info = "乳腺癌数据集（分类）：569个样本，30个特征，2个类别"
    elif dataset_name == 'digits':
        data_obj = datasets.load_digits()
        info = "手写数字数据集（分类）：1797个样本，64个特征，10个类别（0-9）"
    elif dataset_name == 'diabetes':
        data_obj = datasets.load_diabetes()
        info = "糖尿病数据集（回归）：442个样本，10个特征"
    elif dataset_name == 'boston':
        try:
            data_obj = datasets.fetch_california_housing()
            info = "注意：波士顿房价数据集已弃用，已替换为加州房价数据集（回归）：20640个样本，8个特征"
        except:
            data_obj = datasets.load_diabetes()  # 备用
            info = "波士顿数据集不可用，已使用糖尿病数据集（回归）：442个样本，10个特征"
    elif dataset_name == 'california_housing':
        data_obj = datasets.fetch_california_housing()
        info = "加州房价数据集（回归）：20640个样本，8个特征"
    elif dataset_name == 'linnerud':
        data_obj = datasets.load_linnerud()
        info = "Linnerud数据集（多输出回归）：20个样本，3个特征，3个目标"
    elif dataset_name == 'make_classification':
        X, y = datasets.make_classification(n_samples=100, n_features=4, n_informative=2, 
                                           n_redundant=0, n_classes=2, random_state=42)
        feature_names = [f'feature_{i}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'target_names': ['class_0', 'class_1'],
            'DESCR': '生成的分类数据集'
        })()
        info = "生成的分类数据集：100个样本，4个特征，2个类别"
    elif dataset_name == 'make_regression':
        X, y = datasets.make_regression(n_samples=100, n_features=4, n_informative=2, 
                                       noise=10, random_state=42)
        feature_names = [f'feature_{i}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'DESCR': '生成的回归数据集'
        })()
        info = "生成的回归数据集：100个样本，4个特征"
    elif dataset_name == 'make_blobs':
        X, y = datasets.make_blobs(n_samples=100, n_features=2, centers=3, 
                                   random_state=42)
        feature_names = [f'feature_{i}' for i in range(X.shape[1])]
        data_obj = type('obj', (object,), {
            'data': X,
            'target': y,
            'feature_names': feature_names,
            'DESCR': '生成的聚类数据集'
        })()
        info = "生成的聚类数据集：100个样本，2个特征，3个聚类中心"
    else:
        raise ValueError(f"未知的数据集名称: {dataset_name}。可用数据集: iris, wine, breast_cancer, digits, diabetes, california_housing, linnerud, make_classification, make_regression, make_blobs")
    
    if return_X_y:
        X = data_obj.data
        y = data_obj.target
        feature_names = data_obj.feature_names if hasattr(data_obj, 'feature_names') else [f'feature_{i}' for i in range(X.shape[1])]
        target_names = data_obj.target_names if hasattr(data_obj, 'target_names') else None
        return X, y, feature_names, target_names, info
    else:
        # 创建DataFrame
        X = data_obj.data
        y = data_obj.target
        
        # 获取特征名称
        if hasattr(data_obj, 'feature_names'):
            feature_names = data_obj.feature_names
        else:
            feature_names = [f'feature_{i}' for i in range(X.shape[1])]
        
        # 创建DataFrame
        data_dict = {}
        for i, name in enumerate(feature_names):
            data_dict[name] = X[:, i]
        data_dict['target'] = y
        
        data = pd.DataFrame(data_dict)
        columns = data.columns.tolist()
        shape = data.shape
        
        return data, columns, shape, info
