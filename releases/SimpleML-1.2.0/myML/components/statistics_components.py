"""
统计分析组件
用于 Grasshopper Python 组件
提供基本统计分析功能：均值、标准差、相关性等
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

import pandas as pd
import numpy as np


def calculate_statistics(data):
    """
    计算基本统计信息组件
    
    输入:
        data: 数据（pandas DataFrame 或 numpy 数组）
    
    输出:
        stats: 统计信息字典
            - mean: 均值（每列的均值）
            - std: 标准差（每列的标准差）
            - min: 最小值
            - max: 最大值
            - median: 中位数
            - count: 数据点数量
    """
    # 转换为DataFrame（如果不是）
    if isinstance(data, np.ndarray):
        data = pd.DataFrame(data)
    elif not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    stats = {
        'mean': data.mean().to_dict(),
        'std': data.std().to_dict(),
        'min': data.min().to_dict(),
        'max': data.max().to_dict(),
        'median': data.median().to_dict(),
        'count': data.count().to_dict()
    }
    
    return stats


def calculate_correlation(data, method='pearson'):
    """
    计算相关性矩阵组件
    
    输入:
        data: 数据（pandas DataFrame 或 numpy 数组）
        method: 相关性计算方法（字符串）
            - 'pearson': 皮尔逊相关系数（默认，适用于线性关系）
            - 'spearman': 斯皮尔曼相关系数（适用于单调关系）
            - 'kendall': 肯德尔相关系数（适用于有序数据）
    
    输出:
        correlation_matrix: 相关性矩阵（pandas DataFrame）
        correlation_dict: 相关性字典（便于查看特定变量对的相关性）
    """
    # 转换为DataFrame（如果不是）
    if isinstance(data, np.ndarray):
        data = pd.DataFrame(data)
    elif not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    # 计算相关性矩阵
    correlation_matrix = data.corr(method=method)
    
    # 转换为字典格式（便于访问）
    correlation_dict = correlation_matrix.to_dict()
    
    return correlation_matrix, correlation_dict


def get_data_summary(data):
    """
    获取数据摘要组件（综合统计信息）
    
    输入:
        data: 数据（pandas DataFrame 或 numpy 数组）
    
    输出:
        summary: 数据摘要字典
            - statistics: 基本统计信息
            - correlation: 相关性矩阵
            - missing_values: 缺失值统计
            - data_types: 数据类型
    """
    # 转换为DataFrame（如果不是）
    if isinstance(data, np.ndarray):
        data = pd.DataFrame(data)
    elif not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    # 基本统计信息
    statistics = calculate_statistics(data)
    
    # 相关性矩阵
    correlation_matrix, correlation_dict = calculate_correlation(data)
    
    # 缺失值统计
    missing_values = data.isnull().sum().to_dict()
    
    # 数据类型
    data_types = data.dtypes.to_dict()
    
    summary = {
        'statistics': statistics,
        'correlation': {
            'matrix': correlation_matrix.to_dict(),
            'method': 'pearson'
        },
        'missing_values': missing_values,
        'data_types': {str(k): str(v) for k, v in data_types.items()},
        'shape': data.shape
    }
    
    return summary


def describe_features(data, feature_names=None):
    """
    描述特征统计信息组件
    
    输入:
        data: 数据（pandas DataFrame 或 numpy 数组）
        feature_names: 特征名称列表（可选，如果data是numpy数组则需要提供）
    
    输出:
        description: 特征描述字典（每列的详细统计信息）
    """
    # 转换为DataFrame（如果不是）
    if isinstance(data, np.ndarray):
        if feature_names is None:
            feature_names = [f'Feature_{i}' for i in range(data.shape[1])]
        data = pd.DataFrame(data, columns=feature_names)
    elif not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    # 使用pandas的describe方法获取详细统计信息
    description = data.describe().to_dict()
    
    return description
