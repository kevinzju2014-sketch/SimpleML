"""
朴素贝叶斯分类器参数配置组件
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


def train_naive_bayes_classifier(type='GaussianNB', var_smoothing=1e-9,
                                 alpha=1.0, fit_prior=True, class_prior=None):
    """
    配置朴素贝叶斯分类器参数（不进行实际训练）
    
    输入参数:
        type: 贝叶斯类型，默认"GaussianNB"（可选：GaussianNB, MultinomialNB, BernoulliNB, ComplementNB, CategoricalNB）
        var_smoothing: 方差平滑参数，默认1e-9（仅用于GaussianNB）
        alpha: 平滑参数，默认1.0（用于MultinomialNB和BernoulliNB）
        fit_prior: 是否学习类别先验概率，默认True（用于MultinomialNB和BernoulliNB）
        class_prior: 类别先验概率，默认None（可选：None或数组）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    params = {
        'algorithm': 'naive_bayes',
        'type': str(type),
        'var_smoothing': float(var_smoothing),
        'alpha': float(alpha),
        'fit_prior': bool(fit_prior)
    }
    
    if class_prior is not None:
        # 如果是字符串，尝试解析；如果是列表，直接使用
        if isinstance(class_prior, str):
            try:
                class_prior = json.loads(class_prior)
            except:
                pass
        params['class_prior'] = class_prior
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""朴素贝叶斯分类器参数配置
==================
算法名称: Naive Bayes Classifier
算法类型: {type}
适用范围: 分类任务，文本分类
参数配置:
  - 类型: {type}
  - 方差平滑: {var_smoothing}
  - 平滑参数: {alpha}
  - 学习先验概率: {fit_prior}
  - 类别先验: {class_prior if class_prior is not None else 'None'}

预期结果:
  - 适用于文本分类
  - 训练速度快
  - 对特征独立性假设敏感

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Classifier组件完成训练。
"""
    
    return algorithm_params, readme
