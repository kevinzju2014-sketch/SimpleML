"""
线性回归器参数配置组件
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


def train_linear_regression(fit_intercept=True, normalize=False,
                            copy_X=True, n_jobs=None):
    """
    配置线性回归器参数（不进行实际训练）
    
    输入参数:
        fit_intercept: 是否拟合截距，默认True
        normalize: 是否标准化，默认False（已弃用，建议在Create Dataset中处理）
        copy_X: 是否复制X，默认True
        n_jobs: 并行任务数，默认None（-1表示使用所有CPU）
    
    输出参数:
        algorithm_params: 本次配置的参数字典（JSON格式），连接到通用训练组件的Algorithm输入
        readme: 算法说明
    """
    # 构建参数字典
    params = {
        'algorithm': 'linear_regression',
        'fit_intercept': bool(fit_intercept),
        'normalize': bool(normalize),
        'copy_X': bool(copy_X)
    }
    
    if n_jobs is not None:
        params['n_jobs'] = int(n_jobs)
    
    # 转换为JSON字符串
    algorithm_params = json.dumps(params, ensure_ascii=False)
    
    # 生成算法说明
    readme = f"""线性回归器参数配置
==================
算法名称: Linear Regression
适用范围: 回归任务，线性关系
参数配置:
  - 拟合截距: {fit_intercept}
  - 标准化: {normalize}（已弃用，建议在Create Dataset中处理）
  - 复制X: {copy_X}
  - 并行任务数: {n_jobs if n_jobs is not None else 'None'}

预期结果:
  - 适用于线性关系的数据
  - 训练速度快
  - 可解释性强

注意: 此组件只配置参数，不进行实际训练。
需要将Algorithm Params输出连接到Train Regressor组件完成训练。
"""
    
    return algorithm_params, readme
