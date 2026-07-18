"""
随机森林分类器参数配置组件
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


def train_random_forest_classifier(n_estimators=100, max_depth=None, 
                                   min_samples_split=2, min_samples_leaf=1,
                                   max_features='sqrt', bootstrap=True,
                                   random_state=None, class_weight=None,
                                   criterion='gini'):
    """
    配置随机森林分类器参数（不进行实际训练）
    
    输入参数:
        n_estimators: 树的数量，默认100
        max_depth: 最大深度，默认None（0表示None）
        min_samples_split: 内部节点再划分所需最小样本数，默认2
        min_samples_leaf: 叶子节点所需最小样本数，默认1
        max_features: 最大特征数，默认"sqrt"（可选：sqrt, log2, None或整数）
        bootstrap: 是否使用bootstrap采样，默认True
        random_state: 随机种子，默认None
        class_weight: 类别权重，默认None（可选：balanced, balanced_subsample）
        criterion: 划分标准，默认"gini"（可选：gini, entropy, log_loss）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明（包含算法名称、适用范围、参数范围、预期结果）
    """
    # 处理max_depth：0表示None
    if max_depth == 0:
        max_depth = None
    
    # 处理max_features：如果是字符串"None"，转换为None
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
    
    if class_weight is not None:
        params['class_weight'] = str(class_weight)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""随机森林分类器参数配置
==================
算法名称: Random Forest Classifier
适用范围: 分类任务，支持多分类
参数配置:
  - 树的数量: {n_estimators}
  - 最大深度: {max_depth if max_depth is not None else '无限制'}
  - 最小分割样本数: {min_samples_split}
  - 最小叶子样本数: {min_samples_leaf}
  - 最大特征数: {max_features}
  - Bootstrap采样: {bootstrap}
  - 划分标准: {criterion}
  - 类别权重: {class_weight if class_weight else 'None'}

预期结果:
  - 适用于非线性分类问题
  - 对过拟合有较好的抵抗能力
  - 可以处理特征重要性
  - 训练时间相对较长

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Classifier组件完成训练。
"""
    
    return algorithm_params, readme
