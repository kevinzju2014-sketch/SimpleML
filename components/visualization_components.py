"""
可视化组件
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
from sklearn.decomposition import PCA
from sklearn.manifold import TSNE


def reduce_dimensions(X, method='pca', n_components=2, labels=None):
    """
    降维可视化组件
    
    输入参数:
        X: 特征数据（numpy数组或列表的列表）
        method: 降维方法，'pca' 或 'tsne'，默认'pca'
        n_components: 降维后的维度（2或3），默认2
        labels: 标签（可选，用于着色）
    
    输出参数:
        reduced_X: 降维后的数据（numpy数组，shape: (n_samples, n_components)）
        explained_variance: 解释方差（仅PCA，None表示不使用）
        readme: 组件使用说明
    """
    X = np.array(X)
    
    # 确保X是2D数组
    if len(X.shape) == 1:
        X = X.reshape(-1, 1)
    
    # 处理字符串/对象数组
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            raise ValueError("降维失败：X 中包含非数值内容。请确保特征列都是数值。")
    
    if method == 'pca':
        reducer = PCA(n_components=min(n_components, X.shape[1]))
        reduced_X = reducer.fit_transform(X)
        explained_variance = reducer.explained_variance_ratio_.tolist() if hasattr(reducer, 'explained_variance_ratio_') else None
    elif method == 'tsne':
        reducer = TSNE(n_components=n_components, random_state=42, perplexity=min(30, X.shape[0] - 1))
        reduced_X = reducer.fit_transform(X)
        explained_variance = None
    else:
        raise ValueError(f"不支持的降维方法: {method}。支持的方法: 'pca', 'tsne'")
    
    readme = f"使用{method.upper()}方法将数据从{X.shape[1]}维降维到{n_components}维"
    
    return reduced_X.tolist(), explained_variance, readme


def calculate_elbow_score(X, max_clusters=10, algorithm='kmeans'):
    """
    计算Elbow方法得分（用于确定最佳聚类数）
    
    输入参数:
        X: 特征数据（numpy数组或列表的列表）
        max_clusters: 最大聚类数，默认10
        algorithm: 聚类算法，'kmeans'，默认'kmeans'
    
    输出参数:
        n_clusters_list: 聚类数列表
        scores: 对应的得分列表（SSE或inertia）
        readme: 组件使用说明
    """
    from sklearn.cluster import KMeans
    
    X = np.array(X)
    if len(X.shape) == 1:
        X = X.reshape(-1, 1)
    
    # 处理字符串/对象数组
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            raise ValueError("Elbow计算失败：X 中包含非数值内容。")
    
    n_clusters_list = []
    scores = []
    
    for k in range(1, min(max_clusters + 1, X.shape[0] + 1)):
        kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
        kmeans.fit(X)
        n_clusters_list.append(k)
        scores.append(float(kmeans.inertia_))
    
    readme = f"Elbow方法：计算了1到{max_clusters}个聚类数的SSE（误差平方和）"
    
    return n_clusters_list, scores, readme


def calculate_silhouette_score(X, labels):
    """
    计算Silhouette得分（用于评估聚类质量）
    
    输入参数:
        X: 特征数据（numpy数组或列表的列表）
        labels: 聚类标签（numpy数组或列表）
    
    输出参数:
        score: Silhouette得分（-1到1之间）
        readme: 组件使用说明
    """
    from sklearn.metrics import silhouette_score
    
    X = np.array(X)
    labels = np.array(labels)
    
    if len(X.shape) == 1:
        X = X.reshape(-1, 1)
    
    # 处理字符串/对象数组
    if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
        try:
            X = X.astype(float)
        except Exception:
            raise ValueError("Silhouette计算失败：X 中包含非数值内容。")
    
    if len(np.unique(labels)) < 2:
        score = -1.0
        readme = "Silhouette得分：需要至少2个聚类才能计算"
    else:
        score = float(silhouette_score(X, labels))
        readme = f"Silhouette得分: {score:.4f}（越接近1表示聚类质量越好）"
    
    return score, readme
