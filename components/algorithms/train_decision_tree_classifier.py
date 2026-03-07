"""
决策树分类器参数配置组件
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


def train_decision_tree_classifier(criterion='gini', max_depth=None,
                                   min_samples_split=2, min_samples_leaf=1,
                                   min_weight_fraction_leaf=0.0, max_features=None,
                                   random_state=None, max_leaf_nodes=None,
                                   class_weight=None):
    """
    配置决策树分类器参数（不进行实际训练）
    
    输入参数:
        criterion: 划分标准，默认"gini"（可选：gini, entropy, log_loss）
        max_depth: 最大深度，默认None（0表示None）
        min_samples_split: 内部节点再划分所需最小样本数，默认2
        min_samples_leaf: 叶子节点所需最小样本数，默认1
        min_weight_fraction_leaf: 叶子节点所需最小权重比例，默认0.0
        max_features: 最大特征数，默认None（可选：sqrt, log2, None或整数）
        random_state: 随机种子，默认None
        max_leaf_nodes: 最大叶子节点数，默认None
        class_weight: 类别权重，默认None（可选：balanced）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 处理max_depth：0表示None
    if max_depth == 0:
        max_depth = None
    
    # 处理max_features
    if isinstance(max_features, str):
        if max_features.lower() == 'none':
            max_features = None
        elif max_features.isdigit():
            max_features = int(max_features)
    
    # 构建参数字典
    params = {
        'algorithm': 'decision_tree',
        'criterion': str(criterion),
        'min_samples_split': int(min_samples_split),
        'min_samples_leaf': int(min_samples_leaf),
        'min_weight_fraction_leaf': float(min_weight_fraction_leaf)
    }
    
    if max_depth is not None:
        params['max_depth'] = int(max_depth)
    
    if max_features is not None:
        params['max_features'] = max_features
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    if max_leaf_nodes is not None:
        params['max_leaf_nodes'] = int(max_leaf_nodes)
    
    if class_weight is not None:
        params['class_weight'] = str(class_weight)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""决策树分类器参数配置
==================
算法名称: Decision Tree Classifier
适用范围: 分类任务，可解释性强
参数配置:
  - 划分标准: {criterion}
  - 最大深度: {max_depth if max_depth is not None else '无限制'}
  - 最小分割样本数: {min_samples_split}
  - 最小叶子样本数: {min_samples_leaf}
  - 最小权重比例: {min_weight_fraction_leaf}
  - 最大特征数: {max_features if max_features is not None else 'None'}
  - 最大叶子节点数: {max_leaf_nodes if max_leaf_nodes is not None else 'None'}
  - 类别权重: {class_weight if class_weight else 'None'}

预期结果:
  - 适用于非线性分类
  - 可解释性强
  - 容易过拟合

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Classifier组件完成训练。
"""
    
    return algorithm_params, readme
