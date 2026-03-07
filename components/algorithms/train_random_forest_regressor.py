"""
随机森林回归器参数配置组件
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


def train_random_forest_regressor(n_estimators=100, max_depth=None,
                                  min_samples_split=2, min_samples_leaf=1,
                                  max_features='sqrt', bootstrap=True,
                                  random_state=None, criterion='squared_error'):
    """
    配置随机森林回归器参数（不进行实际训练）
    
    输入参数:
        n_estimators: 树的数量，默认100
        max_depth: 最大深度，默认None（0表示None）
        min_samples_split: 内部节点再划分所需最小样本数，默认2
        min_samples_leaf: 叶子节点所需最小样本数，默认1
        max_features: 最大特征数，默认"sqrt"（可选：sqrt, log2, None或整数）
        bootstrap: 是否使用bootstrap采样，默认True
        random_state: 随机种子，默认None
        criterion: 划分标准，默认"squared_error"（可选：squared_error, absolute_error, friedman_mse, poisson）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 处理max_depth：0表示None
    if max_depth == 0:
        max_depth = None
    
    # 处理max_features
    if isinstance(max_features, str) and max_features.lower() == 'none':
        max_features = None
    elif isinstance(max_features, str) and max_features.isdigit():
        max_features = int(max_features)
    
    # 构建参数字典
    params = {
        'algorithm': 'random_forest',
        'n_estimators': int(n_estimators),
        'min_samples_split': int(min_samples_split),
        'min_samples_leaf': int(min_samples_leaf),
        'max_features': max_features,
        'bootstrap': bool(bootstrap),
        'criterion': str(criterion)
    }
    
    if max_depth is not None:
        params['max_depth'] = int(max_depth)
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""随机森林回归器参数配置
==================
算法名称: Random Forest Regressor
适用范围: 回归任务
参数配置:
  - 树的数量: {n_estimators}
  - 最大深度: {max_depth if max_depth is not None else '无限制'}
  - 最小分割样本数: {min_samples_split}
  - 最小叶子样本数: {min_samples_leaf}
  - 最大特征数: {max_features}
  - Bootstrap采样: {bootstrap}
  - 划分标准: {criterion}

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Regressor组件完成训练。
"""
    
    return algorithm_params, readme
