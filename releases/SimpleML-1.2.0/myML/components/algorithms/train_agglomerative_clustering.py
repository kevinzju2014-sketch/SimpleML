"""
层次聚类参数配置组件
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


def train_agglomerative_clustering(n_clusters=3, linkage='ward',
                                   affinity='euclidean', compute_full_tree=False,
                                   distance_threshold=None, compute_distances=False):
    """
    配置层次聚类参数（不进行实际训练）
    
    输入参数:
        n_clusters: 聚类数量，默认3
        linkage: 聚合方式，默认"ward"（可选：ward, complete, average, single）
        affinity: 距离度量，默认"euclidean"（可选：euclidean, l1, l2, manhattan, cosine等）
        compute_full_tree: 是否计算完整树，默认False
        distance_threshold: 距离阈值，默认None（如果设置，n_clusters必须为None）
        compute_distances: 是否计算距离，默认False
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    params = {
        'algorithm': 'agglomerative',
        'n_clusters': int(n_clusters) if distance_threshold is None else None,
        'linkage': str(linkage),
        'affinity': str(affinity),
        'compute_full_tree': bool(compute_full_tree),
        'compute_distances': bool(compute_distances)
    }
    
    if distance_threshold is not None:
        params['distance_threshold'] = float(distance_threshold)
        params['n_clusters'] = None
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""层次聚类参数配置
==================
算法名称: Agglomerative Clustering
适用范围: 聚类任务，层次聚类
参数配置:
  - 聚类数量: {n_clusters if distance_threshold is None else '由距离阈值决定'}
  - 聚合方式: {linkage}
  - 距离度量: {affinity}
  - 计算完整树: {compute_full_tree}
  - 距离阈值: {distance_threshold if distance_threshold is not None else 'None'}
  - 计算距离: {compute_distances}

预期结果:
  - 适用于层次结构数据
  - 可以生成树状图
  - 计算复杂度较高

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Cluster组件完成训练。
"""
    
    return algorithm_params, readme
