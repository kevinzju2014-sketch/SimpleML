"""
模型训练组件
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
import json
import pandas as pd


def _extract_X_y_by_names(dataset, X_names=None, y_names=None):
    """
    根据列名从dataset中提取X和y
    
    参数:
        dataset: Dataset对象
        X_names: X列名/索引列表（可选），如果提供，将从dataset.X中提取对应的列作为特征
        y_names: y列名/索引列表（可选），如果提供，将从dataset.X中提取对应的列作为标签
    
    返回:
        X: 特征数据（numpy数组）
        y: 标签数据（numpy数组）
        has_labels: 是否有标签
    """
    # 如果提供了列名，从dataset.X中提取对应的列
    if X_names is not None or y_names is not None:
        all_data = dataset.get_X()
        
        # 确保all_data是2D数组
        if len(all_data.shape) == 1:
            all_data = all_data.reshape(-1, 1)
        
        n_cols = all_data.shape[1]
        
        # 检查dataset是否有列名属性
        has_column_names = hasattr(dataset, 'column_names') and dataset.column_names is not None
        
        # 将列名/索引转换为列索引
        def get_column_indices(names, all_columns=None):
            """将列名或索引转换为列索引列表"""
            if names is None:
                return None
            
            indices = []
            for name in names:
                if isinstance(name, int):
                    # 直接是索引
                    if name < 0 or name >= n_cols:
                        raise ValueError(f"列索引 {name} 超出范围 [0, {n_cols-1}]")
                    indices.append(name)
                elif isinstance(name, str):
                    # 是列名或字符串形式的索引
                    if has_column_names and all_columns is not None:
                        # 尝试按列名查找
                        if name in all_columns:
                            indices.append(all_columns.index(name))
                        else:
                            raise ValueError(f"找不到列名: {name}")
                    else:
                        # 尝试解析为索引
                        try:
                            idx = int(name)
                            if idx < 0 or idx >= n_cols:
                                raise ValueError(f"列索引 {idx} 超出范围 [0, {n_cols-1}]")
                            indices.append(idx)
                        except ValueError:
                            raise ValueError(f"无法解析列名/索引: {name}（dataset没有列名信息）")
                else:
                    raise ValueError(f"无效的列名/索引类型: {type(name)}")
            
            return indices
        
        column_names = dataset.column_names if has_column_names else None
        
        # 提取X列
        if X_names is not None:
            X_indices = get_column_indices(X_names, column_names)
            if len(X_indices) == 1:
                X = all_data[:, X_indices[0]:X_indices[0]+1]
            else:
                X = all_data[:, X_indices]
        else:
            # 如果没有指定X_names，使用所有非y列
            if y_names is not None:
                try:
                    y_indices = get_column_indices(y_names, column_names)
                    all_indices = set(range(n_cols))
                    X_indices = sorted(list(all_indices - set(y_indices)))
                    if len(X_indices) == 0:
                        raise ValueError("没有剩余列作为特征（所有列都被指定为y）")
                    if len(X_indices) == 1:
                        X = all_data[:, X_indices[0]:X_indices[0]+1]
                    else:
                        X = all_data[:, X_indices]
                except ValueError as e:
                    # 如果找不到列名 "target"，使用所有列作为特征
                    if len(y_names) == 1 and y_names[0] == 'target':
                        X = all_data
                    else:
                        raise e
            else:
                X = all_data
        
        # 提取y列
        if y_names is not None:
            try:
                y_indices = get_column_indices(y_names, column_names)
                if len(y_indices) == 1:
                    y = all_data[:, y_indices[0]]
                else:
                    # 多个y列，展平或保持2D
                    y = all_data[:, y_indices]
                    if len(y.shape) > 1 and y.shape[1] == 1:
                        y = y.ravel()
                has_labels = True
            except ValueError as e:
                # 如果找不到列名，检查是否是 "target" 且 dataset 有标签
                if len(y_names) == 1 and y_names[0] == 'target' and dataset.has_labels():
                    # 使用 dataset 原有的 y
                    y = dataset.get_y()
                    has_labels = True
                else:
                    # 重新抛出异常
                    raise e
        else:
            # 如果没有指定y_names，使用dataset原有的y
            y = dataset.get_y() if dataset.has_labels() else None
            has_labels = dataset.has_labels()
        
        return X, y, has_labels
    
    # 如果没有提供列名，使用原始方法
    result = deconstruct_dataset(dataset)
    return result[0], result[1], result[2]  # X, y, has_labels


def train_classifier(dataset=None, algorithm='random_forest', X_names=None, y_names=None):
    """
    训练分类模型组件（通用，执行实际训练）
    
    输入参数:
        dataset: Dataset对象（必需）
        algorithm: 算法参数配置，可以是：
            - 算法名称字符串（如"random_forest"）
            - JSON格式的参数字典字符串（来自算法特定训练组件的Algorithm Params输出）
        X_names: X列名列表（可选），如果提供，将从dataset中提取对应的列作为特征
        y_names: y列名列表（可选），如果提供，将从dataset中提取对应的列作为标签
    
    输出参数:
        model: 训练好的模型对象
        readme: 组件使用说明
    """
    if dataset is None:
        raise ValueError("必须提供Dataset对象")
    
    # 从dataset中提取X和y（根据列名或使用原始方法）
    X, y, has_labels = _extract_X_y_by_names(dataset, X_names, y_names)
    if not has_labels:
        raise ValueError("分类模型需要标签数据，但dataset中没有标签")
    
    # 解析algorithm参数
    algorithm_name = None
    algorithm_params = {}
    
    # 调试：输出algorithm参数的值和类型
    import sys
    print(f'DEBUG train_classifier: algorithm type = {type(algorithm)}, value = {repr(algorithm)}', file=sys.stderr)
    
    if isinstance(algorithm, str):
        # 尝试解析为JSON
        try:
            params_dict = json.loads(algorithm)
            print(f'DEBUG train_classifier: parsed JSON = {params_dict}', file=sys.stderr)
            if isinstance(params_dict, dict):
                # 如果是字典，提取算法名称和参数
                algorithm_name = params_dict.get('algorithm', 'random_forest')
                algorithm_params = {k: v for k, v in params_dict.items() if k != 'algorithm'}
                print(f'DEBUG train_classifier: algorithm_name = {algorithm_name}, algorithm_params = {algorithm_params}', file=sys.stderr)
            else:
                # 如果不是字典，当作算法名称
                algorithm_name = algorithm
        except (json.JSONDecodeError, TypeError) as e:
            # 不是JSON，当作算法名称
            print(f'DEBUG train_classifier: JSON解析失败: {e}', file=sys.stderr)
            algorithm_name = algorithm
    elif isinstance(algorithm, dict):
        # 直接是字典
        algorithm_name = algorithm.get('algorithm', 'random_forest')
        algorithm_params = {k: v for k, v in algorithm.items() if k != 'algorithm'}
    else:
        raise ValueError("algorithm参数必须是字符串或字典")
    
    # 训练模型
    manager = MLModelManager()
    print(f'DEBUG train_classifier: 准备调用 manager.train_classifier, algorithm_name = {repr(algorithm_name)}, algorithm_params = {algorithm_params}', file=sys.stderr)
    model = manager.train_classifier(X, y, algorithm=algorithm_name, **algorithm_params)
    
    # 收集模型信息
    n_samples = X.shape[0]
    n_features = X.shape[1] if len(X.shape) > 1 else 1
    n_classes = len(np.unique(y)) if y is not None else 0
    
    # 构建详细的模型信息（Tree格式：列表的列表）
    model_info_tree = []
    
    # 基本信息
    model_info_tree.append(["算法", str(algorithm_name)])
    model_info_tree.append(["模型类型", "分类"])
    model_info_tree.append(["样本数量", str(n_samples)])
    model_info_tree.append(["特征维度", str(n_features)])
    model_info_tree.append(["类别数量", str(n_classes)])
    
    # 添加类别信息
    if y is not None:
        unique_classes = np.unique(y)
        if len(unique_classes) <= 10:  # 如果类别数不多，显示所有类别
            classes_str = ', '.join(map(str, unique_classes))
            model_info_tree.append(["类别标签", classes_str])
        else:
            classes_str = ', '.join(map(str, unique_classes[:5])) + f"... (共{len(unique_classes)}个类别)"
            model_info_tree.append(["类别标签", classes_str])
    
    # 添加算法参数信息
    if algorithm_params:
        for key, value in algorithm_params.items():
            # 格式化参数值
            if isinstance(value, (int, float)):
                model_info_tree.append([f"参数: {key}", str(value)])
            elif isinstance(value, str):
                model_info_tree.append([f"参数: {key}", value])
            elif isinstance(value, bool):
                model_info_tree.append([f"参数: {key}", str(value)])
            else:
                model_info_tree.append([f"参数: {key}", str(value)])
    else:
        model_info_tree.append(["算法参数", "使用默认参数"])
    
    # 转换为JSON字符串以便C#解析（紧凑格式，单行）
    model_info = json.dumps(model_info_tree, ensure_ascii=False, separators=(',', ':'))
    
    readme = f"已使用{algorithm_name}算法训练分类模型"
    
    return model, readme, model_info


def train_regressor(dataset=None, algorithm='linear_regression', X_names=None, y_names=None):
    """
    训练回归模型组件（通用，执行实际训练）
    
    输入参数:
        dataset: Dataset对象（必需）
        algorithm: 算法参数配置，可以是：
            - 算法名称字符串（如"linear_regression"）
            - JSON格式的参数字典字符串（来自算法特定训练组件的Algorithm Params输出）
        X_names: X列名列表（可选），如果提供，将从dataset中提取对应的列作为特征
        y_names: y列名列表（可选），如果提供，将从dataset中提取对应的列作为目标值
    
    输出参数:
        model: 训练好的模型对象
        readme: 组件使用说明
    """
    if dataset is None:
        raise ValueError("必须提供Dataset对象")
    
    # 从dataset中提取X和y（根据列名或使用原始方法）
    X, y, has_labels = _extract_X_y_by_names(dataset, X_names, y_names)
    if not has_labels:
        raise ValueError("回归模型需要目标值，但dataset中没有标签")
    
    # 解析algorithm参数
    algorithm_name = None
    algorithm_params = {}
    
    if isinstance(algorithm, str):
        # 尝试解析为JSON
        try:
            params_dict = json.loads(algorithm)
            if isinstance(params_dict, dict):
                algorithm_name = params_dict.get('algorithm', 'linear_regression')
                algorithm_params = {k: v for k, v in params_dict.items() if k != 'algorithm'}
            else:
                algorithm_name = algorithm
        except (json.JSONDecodeError, TypeError):
            algorithm_name = algorithm
    elif isinstance(algorithm, dict):
        algorithm_name = algorithm.get('algorithm', 'linear_regression')
        algorithm_params = {k: v for k, v in algorithm.items() if k != 'algorithm'}
    else:
        raise ValueError("algorithm参数必须是字符串或字典")
    
    # 训练模型
    manager = MLModelManager()
    model = manager.train_regressor(X, y, algorithm=algorithm_name, **algorithm_params)
    
    # 收集模型信息
    n_samples = X.shape[0]
    n_features = X.shape[1] if len(X.shape) > 1 else 1
    
    # 构建详细的模型信息（Tree格式：列表的列表）
    model_info_tree = []
    
    # 基本信息
    model_info_tree.append(["算法", str(algorithm_name)])
    model_info_tree.append(["模型类型", "回归"])
    model_info_tree.append(["样本数量", str(n_samples)])
    model_info_tree.append(["特征维度", str(n_features)])
    
    # 添加目标值统计信息
    if y is not None:
        y_array = np.array(y).ravel()
        y_min = np.min(y_array)
        y_max = np.max(y_array)
        y_mean = np.mean(y_array)
        y_std = np.std(y_array)
        model_info_tree.append(["目标值最小值", f"{y_min:.4f}"])
        model_info_tree.append(["目标值最大值", f"{y_max:.4f}"])
        model_info_tree.append(["目标值平均值", f"{y_mean:.4f}"])
        model_info_tree.append(["目标值标准差", f"{y_std:.4f}"])
    
    # 添加算法参数信息
    if algorithm_params:
        for key, value in algorithm_params.items():
            # 格式化参数值
            if isinstance(value, (int, float)):
                model_info_tree.append([f"参数: {key}", str(value)])
            elif isinstance(value, str):
                model_info_tree.append([f"参数: {key}", value])
            elif isinstance(value, bool):
                model_info_tree.append([f"参数: {key}", str(value)])
            else:
                model_info_tree.append([f"参数: {key}", str(value)])
    else:
        model_info_tree.append(["算法参数", "使用默认参数"])
    
    # 转换为JSON字符串以便C#解析（紧凑格式，单行）
    model_info = json.dumps(model_info_tree, ensure_ascii=False, separators=(',', ':'))
    
    readme = f"已使用{algorithm_name}算法训练回归模型"
    
    return model, readme, model_info


def train_cluster(dataset=None, algorithm='kmeans'):
    """
    训练聚类模型组件（通用，执行实际训练）
    
    输入参数:
        dataset: Dataset对象（必需）
        algorithm: 算法参数配置，可以是：
            - 算法名称字符串（如"kmeans"）
            - JSON格式的参数字典字符串（来自算法特定训练组件的Algorithm Params输出）
    
    输出参数:
        model: 训练好的模型对象
        readme: 组件使用说明
    """
    if dataset is None:
        raise ValueError("必须提供Dataset对象")
    
    # 从dataset中提取X（聚类不需要y）
    result = deconstruct_dataset(dataset)
    X = result[0]  # 只需要X
    
    # 解析algorithm参数
    algorithm_name = None
    algorithm_params = {}
    
    if isinstance(algorithm, str):
        # 尝试解析为JSON
        try:
            params_dict = json.loads(algorithm)
            if isinstance(params_dict, dict):
                algorithm_name = params_dict.get('algorithm', 'kmeans')
                algorithm_params = {k: v for k, v in params_dict.items() if k != 'algorithm'}
            else:
                algorithm_name = algorithm
        except (json.JSONDecodeError, TypeError):
            algorithm_name = algorithm
    elif isinstance(algorithm, dict):
        algorithm_name = algorithm.get('algorithm', 'kmeans')
        algorithm_params = {k: v for k, v in algorithm.items() if k != 'algorithm'}
    else:
        raise ValueError("algorithm参数必须是字符串或字典")
    
    # 训练模型
    manager = MLModelManager()
    model = manager.train_cluster(X, algorithm=algorithm_name, **algorithm_params)
    
    # 收集模型信息
    n_samples = X.shape[0]
    n_features = X.shape[1] if len(X.shape) > 1 else 1
    
    # 获取聚类数量（如果模型支持）
    n_clusters = None
    if hasattr(model, 'n_clusters_'):
        n_clusters = model.n_clusters_
    elif hasattr(model, 'labels_'):
        n_clusters = len(np.unique(model.labels_))
    elif 'n_clusters' in algorithm_params:
        n_clusters = algorithm_params['n_clusters']
    
    # 构建详细的模型信息（Tree格式：列表的列表）
    model_info_tree = []
    
    # 基本信息
    model_info_tree.append(["算法", str(algorithm_name)])
    model_info_tree.append(["模型类型", "聚类"])
    model_info_tree.append(["样本数量", str(n_samples)])
    model_info_tree.append(["特征维度", str(n_features)])
    if n_clusters is not None:
        model_info_tree.append(["聚类数量", str(n_clusters)])
    
    # 添加算法参数信息
    if algorithm_params:
        for key, value in algorithm_params.items():
            # 格式化参数值
            if isinstance(value, (int, float)):
                model_info_tree.append([f"参数: {key}", str(value)])
            elif isinstance(value, str):
                model_info_tree.append([f"参数: {key}", value])
            elif isinstance(value, bool):
                model_info_tree.append([f"参数: {key}", str(value)])
            else:
                model_info_tree.append([f"参数: {key}", str(value)])
    else:
        model_info_tree.append(["算法参数", "使用默认参数"])
    
    # 转换为JSON字符串以便C#解析（紧凑格式，单行）
    model_info = json.dumps(model_info_tree, ensure_ascii=False, separators=(',', ':'))
    
    readme = f"已使用{algorithm_name}算法训练聚类模型"
    
    return model, readme, model_info


def get_available_algorithms(model_type):
    """
    获取可用算法列表组件
    
    输入:
        model_type: 模型类型 ('classification', 'regression', 'clustering')
    
    输出:
        algorithms: 可用算法列表
    """
    manager = MLModelManager()
    return manager.get_available_algorithms(model_type)
