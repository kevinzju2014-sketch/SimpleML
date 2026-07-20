"""
Lasso回归器参数配置组件
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


def train_lasso_regression(alpha=1.0, fit_intercept=True, normalize=False,
                          max_iter=1000, tol=1e-4, selection='cyclic',
                          random_state=None, warm_start=False):
    """
    配置Lasso回归器参数（不进行实际训练）
    
    输入参数:
        alpha: 正则化强度，默认1.0
        fit_intercept: 是否拟合截距，默认True
        normalize: 是否标准化，默认False（已弃用）
        max_iter: 最大迭代次数，默认1000
        tol: 停止训练的容差，默认1e-4
        selection: 变量选择策略，默认"cyclic"（可选：cyclic, random）
        random_state: 随机种子，默认None（用于random选择）
        warm_start: 是否使用热启动，默认False
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典（normalize 已弃用，默认不写入）
    params = {
        'algorithm': 'lasso',
        'alpha': float(alpha),
        'fit_intercept': bool(fit_intercept),
        'max_iter': int(max_iter),
        'tol': float(tol),
        'selection': str(selection),
        'warm_start': bool(warm_start)
    }
    if normalize:
        params['normalize'] = True
    
    if random_state is not None:
        params['random_state'] = int(random_state)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""Lasso回归器参数配置
==================
算法名称: Lasso Regression
适用范围: 回归任务，特征选择
参数配置:
  - 正则化强度: {alpha}
  - 拟合截距: {fit_intercept}
  - 标准化: {normalize}（已弃用）
  - 最大迭代次数: {max_iter}
  - 容差: {tol}
  - 选择策略: {selection}
  - 随机种子: {random_state if random_state is not None else 'None'}
  - 热启动: {warm_start}

预期结果:
  - 适用于特征选择
  - 可以将不重要的特征系数变为0
  - 产生稀疏模型

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Regressor组件完成训练。
"""
    
    return algorithm_params, readme
