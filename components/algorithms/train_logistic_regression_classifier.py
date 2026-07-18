"""
逻辑回归分类器参数配置组件
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


def train_logistic_regression_classifier(penalty='l2', C=1.0, solver='lbfgs',
                                        max_iter=100, tol=1e-4, multi_class='auto',
                                        random_state=None, l1_ratio=None,
                                        class_weight=None):
    """
    配置逻辑回归分类器参数（不进行实际训练）
    
    输入参数:
        penalty: 正则化类型，默认"l2"（可选：l1, l2, elasticnet, None）
        C: 惩罚系数的倒数，默认1.0（值越小，正则化越强）
        solver: 优化算法，默认"lbfgs"（可选：lbfgs, liblinear, newton-cg, sag, saga）
        max_iter: 最大迭代次数，默认100
        tol: 停止训练的容差，默认1e-4
        multi_class: 多分类策略，默认"auto"（可选：ovr, multinomial, auto）
        random_state: 随机种子，默认None
        l1_ratio: Elastic-Net混合参数，默认None（仅用于elasticnet惩罚）
        class_weight: 类别权重，默认None（可选：balanced）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    params = {
        'algorithm': 'logistic_regression',
        'penalty': str(penalty) if penalty is not None else None,
        'C': float(C),
        'solver': str(solver),
        'max_iter': int(max_iter),
        'tol': float(tol),
        'multi_class': str(multi_class)
    }
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    if l1_ratio is not None:
        params['l1_ratio'] = float(l1_ratio)
    
    if class_weight is not None:
        params['class_weight'] = str(class_weight)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""逻辑回归分类器参数配置
==================
算法名称: Logistic Regression Classifier
适用范围: 分类任务，线性分类
参数配置:
  - 正则化类型: {penalty}
  - 惩罚系数倒数: {C}
  - 优化算法: {solver}
  - 最大迭代次数: {max_iter}
  - 容差: {tol}
  - 多分类策略: {multi_class}
  - 随机种子: {random_state if random_state is not None else 'None'}
  - L1比例: {l1_ratio if l1_ratio is not None else 'None'}
  - 类别权重: {class_weight if class_weight else 'None'}

预期结果:
  - 适用于线性分类问题
  - 训练速度快
  - 可解释性强

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Classifier组件完成训练。
"""
    
    return algorithm_params, readme
