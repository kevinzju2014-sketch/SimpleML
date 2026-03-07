"""
SVM分类器参数配置组件
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


def train_svm_classifier(kernel='rbf', C=1.0, gamma='scale', degree=3,
                         coef0=0.0, probability=False, random_state=None,
                         class_weight=None, tol=1e-3):
    """
    配置SVM分类器参数（不进行实际训练）
    
    输入参数:
        kernel: 核函数类型，默认"rbf"（可选：linear, poly, sigmoid, rbf, precomputed）
        C: 惩罚系数，默认1.0
        gamma: 核函数系数，默认"scale"（可选：scale, auto或浮点数）
        degree: 多项式核的度数，默认3（仅用于poly核）
        coef0: 核函数中的独立项，默认0.0（用于poly和sigmoid核）
        probability: 是否启用概率估计，默认False
        random_state: 随机种子，默认None
        class_weight: 类别权重，默认None（可选：balanced）
        tol: 停止训练的容差，默认1e-3
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 处理gamma：如果是字符串，保持原样；如果是数字，转换为浮点数
    if isinstance(gamma, str):
        if gamma.lower() not in ['scale', 'auto']:
            try:
                gamma = float(gamma)
            except:
                pass
    
    # 构建参数字典
    params = {
        'algorithm': 'svm',
        'kernel': str(kernel),
        'C': float(C),
        'degree': int(degree),
        'coef0': float(coef0),
        'probability': bool(probability),
        'tol': float(tol)
    }
    
    # gamma可以是字符串或浮点数
    params['gamma'] = gamma
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    if class_weight is not None:
        params['class_weight'] = str(class_weight)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""SVM分类器参数配置
==================
算法名称: Support Vector Machine Classifier
适用范围: 分类任务，支持多分类
参数配置:
  - 核函数类型: {kernel}
  - 惩罚系数: {C}
  - 核函数系数: {gamma}
  - 多项式度数: {degree}
  - 独立项: {coef0}
  - 概率估计: {probability}
  - 容差: {tol}
  - 类别权重: {class_weight if class_weight else 'None'}

预期结果:
  - 适用于非线性分类问题
  - 对高维数据效果好
  - 需要数据标准化

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Classifier组件完成训练。
"""
    
    return algorithm_params, readme
