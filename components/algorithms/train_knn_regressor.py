"""
KNN回归器参数配置组件
用于 Grasshopper Python 组件
注意：此组件只负责配置算法参数，不进行实际训练
"""

import sys
import os
import site
import json

# 添加项目路径
current_dir = os.path.dirname(os.path.abspath(__file__))
components_dir = os.path.dirname(current_dir)
project_dir = os.path.dirname(components_dir)
if project_dir not in sys.path:
    sys.path.insert(0, project_dir)

# 跨平台引导
try:
    from core.env_bootstrap import bootstrap_python_paths
    bootstrap_python_paths(project_dir)
except Exception:
    pass


def train_knn_regressor(n_neighbors=5, weights='uniform', algorithm='auto',
                       leaf_size=30, p=2, metric='minkowski', n_jobs=None):
    """
    配置KNN回归器参数（不进行实际训练）
    
    输入参数:
        n_neighbors: 邻居数，默认5
        weights: 权重函数，默认"uniform"（可选：uniform, distance）
        algorithm: 计算最近邻的算法，默认"auto"（可选：auto, ball_tree, kd_tree, brute）
        leaf_size: 叶子节点大小，默认30（用于ball_tree和kd_tree）
        p: 闵可夫斯基距离的幂参数，默认2（2为欧氏距离，1为曼哈顿距离）
        metric: 距离度量，默认"minkowski"（可选：euclidean, manhattan, chebyshev等）
        n_jobs: 并行任务数，默认None（-1表示使用所有CPU）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    # 注意：sklearn的KNeighborsRegressor也有algorithm参数，需要重命名以避免冲突
    params = {
        'algorithm': 'knn',  # 我们的算法名称
        'n_neighbors': int(n_neighbors),
        'weights': str(weights),
        'algorithm_type': str(algorithm),  # sklearn的algorithm参数（重命名）
        'leaf_size': int(leaf_size),
        'p': int(p),
        'metric': str(metric)
    }
    
    if n_jobs is not None:
        params['n_jobs'] = int(n_jobs)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""KNN回归器参数配置
==================
算法名称: K-Nearest Neighbors Regressor
适用范围: 回归任务
参数配置:
  - 邻居数: {n_neighbors}
  - 权重函数: {weights}
  - 算法: {algorithm}
  - 叶子节点大小: {leaf_size}
  - 距离幂参数: {p}
  - 距离度量: {metric}
  - 并行任务数: {n_jobs if n_jobs is not None else 'None'}

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Regressor组件完成训练。
"""
    
    return algorithm_params, readme
