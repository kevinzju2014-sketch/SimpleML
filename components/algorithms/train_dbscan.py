"""
DBSCAN聚类参数配置组件
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


def train_dbscan(eps=0.5, min_samples=5, metric='euclidean',
                 algorithm='auto', leaf_size=30, p=2, n_jobs=None):
    """
    配置DBSCAN聚类参数（不进行实际训练）
    
    输入参数:
        eps: 邻域半径，默认0.5
        min_samples: 最小样本数，默认5
        metric: 距离度量，默认"euclidean"（可选：euclidean, manhattan, cosine等）
        algorithm: 最近邻算法，默认"auto"（可选：auto, ball_tree, kd_tree, brute）
        leaf_size: 叶子节点大小，默认30（用于ball_tree和kd_tree）
        p: 闵可夫斯基距离的幂参数，默认2（仅用于minkowski距离）
        n_jobs: 并行任务数，默认None（-1表示使用所有CPU）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    # 注意：sklearn的DBSCAN也有algorithm参数，需要重命名
    params = {
        'algorithm': 'dbscan',  # 我们的算法名称
        'eps': float(eps),
        'min_samples': int(min_samples),
        'metric': str(metric),
        'algorithm_type': str(algorithm),  # sklearn的algorithm参数（重命名）
        'leaf_size': int(leaf_size),
        'p': float(p)
    }
    
    if n_jobs is not None:
        params['n_jobs'] = int(n_jobs)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""DBSCAN聚类参数配置
==================
算法名称: DBSCAN Clustering
适用范围: 聚类任务，发现任意形状的聚类
参数配置:
  - 邻域半径: {eps}
  - 最小样本数: {min_samples}
  - 距离度量: {metric}
  - 算法类型: {algorithm}
  - 叶子节点大小: {leaf_size}
  - 距离幂参数: {p}
  - 并行任务数: {n_jobs if n_jobs is not None else 'None'}

预期结果:
  - 适用于任意形状的聚类
  - 可以发现噪声点
  - 不需要预先指定聚类数量

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Cluster组件完成训练。
"""
    
    return algorithm_params, readme
