"""
K-Means聚类参数配置组件
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


def train_kmeans(n_clusters=3, init='k-means++', n_init=10,
                max_iter=300, tol=1e-4, random_state=None, algorithm='lloyd'):
    """
    配置K-Means聚类参数（不进行实际训练）
    
    输入参数:
        n_clusters: 聚类数量，默认3
        init: 初始化方式，默认"k-means++"（可选：k-means++, random或数组）
        n_init: 不同初始化的运行次数，默认10
        max_iter: 单次运行的最大迭代次数，默认300
        tol: 收敛容差，默认1e-4
        random_state: 随机种子，默认None
        algorithm: K-means算法，默认"lloyd"（可选：lloyd, elkan, auto）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    # 注意：sklearn的KMeans也有algorithm参数，需要重命名以避免冲突
    params = {
        'algorithm': 'kmeans',  # 我们的算法名称
        'n_clusters': int(n_clusters),
        'init': str(init),
        'n_init': int(n_init),
        'max_iter': int(max_iter),
        'tol': float(tol),
        'algorithm_type': str(algorithm)  # sklearn的algorithm参数（重命名）
    }
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""K-Means聚类参数配置
==================
算法名称: K-Means Clustering
适用范围: 聚类任务，球形聚类
参数配置:
  - 聚类数量: {n_clusters}
  - 初始化方式: {init}
  - 运行次数: {n_init}
  - 最大迭代次数: {max_iter}
  - 收敛容差: {tol}
  - 算法类型: {algorithm}
  - 随机种子: {random_state if random_state is not None else 'None'}

预期结果:
  - 适用于球形聚类
  - 需要预先指定聚类数量
  - 对初始值敏感

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Cluster组件完成训练。
"""
    
    return algorithm_params, readme
