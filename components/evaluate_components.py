"""
模型评估组件
用于 Grasshopper Python 组件
"""

import sys
import os
import site
import json

# 添加项目路径
current_dir = os.path.dirname(os.path.abspath(__file__))
project_dir = os.path.dirname(current_dir)
if project_dir not in sys.path:
    sys.path.insert(0, project_dir)

# 确保Rhino Python的site-packages在路径中
# 跨平台引导 Rhino / 系统 site-packages
try:
    from core.env_bootstrap import bootstrap_python_paths
    bootstrap_python_paths(project_dir)
except Exception:
    pass

import numpy as np
from sklearn.metrics import (
    accuracy_score, precision_score, recall_score, f1_score,
    confusion_matrix, classification_report,
    mean_squared_error, mean_absolute_error, r2_score, explained_variance_score,
    silhouette_score, davies_bouldin_score, calinski_harabasz_score,
    adjusted_rand_score, normalized_mutual_info_score
)
from components.explain_components import (
    explain_classification_metrics,
    explain_regression_metrics,
    explain_clustering_metrics,
    verdict_classification,
    verdict_regression,
    verdict_clustering,
)


def evaluate_classification(model, X=None, y_true=None, y_pred=None, metrics='all'):
    """
    评估分类模型的性能
    
    输入参数:
        model: 训练好的分类模型对象
        X: 测试特征数据（Tree结构，可选，如果提供了y_pred则不需要）
        y_true: 真实标签数据（Tree结构或numpy数组）
        y_pred: 预测结果（Tree结构或numpy数组，可选，如果提供了X则自动预测）
        metrics: 要计算的评估指标，默认"all"（可选：accuracy, precision, recall, f1, confusion_matrix等，或"all"）
    
    输出参数:
        metrics_json: 评估指标字典（JSON格式字符串）
        report: 详细的评估报告（字符串）
        confusion_matrix_tree: 混淆矩阵（Tree结构，如果计算）
        readme: 组件使用说明
    """
    y_true = np.array(y_true).ravel() if y_true is not None else None
    
    # 如果提供了y_pred，直接使用；否则使用X进行预测
    if y_pred is not None:
        y_pred = np.array(y_pred).ravel()
    elif X is not None:
        X = np.array(X)
        y_pred = model.predict(X)
    else:
        raise ValueError("必须提供X或y_pred参数")
    
    # 确定要计算的指标
    if metrics == 'all' or metrics is None:
        metrics_list = ['accuracy', 'precision', 'recall', 'f1', 'confusion_matrix']
    else:
        if isinstance(metrics, str):
            metrics_list = [m.strip() for m in metrics.split(',')]
        else:
            metrics_list = list(metrics)
    
    # 计算指标
    metrics_dict = {}
    
    if 'accuracy' in metrics_list or metrics == 'all':
        metrics_dict['accuracy'] = float(accuracy_score(y_true, y_pred))
    
    if 'precision' in metrics_list or metrics == 'all':
        try:
            metrics_dict['precision'] = float(precision_score(y_true, y_pred, average='weighted', zero_division=0))
        except:
            metrics_dict['precision'] = None
    
    if 'recall' in metrics_list or metrics == 'all':
        try:
            metrics_dict['recall'] = float(recall_score(y_true, y_pred, average='weighted', zero_division=0))
        except:
            metrics_dict['recall'] = None
    
    if 'f1' in metrics_list or metrics == 'all':
        try:
            metrics_dict['f1_score'] = float(f1_score(y_true, y_pred, average='weighted', zero_division=0))
        except:
            metrics_dict['f1_score'] = None
    
    # 混淆矩阵
    confusion_matrix_tree = None
    if 'confusion_matrix' in metrics_list or metrics == 'all':
        cm = confusion_matrix(y_true, y_pred)
        confusion_matrix_tree = cm.tolist()
        metrics_dict['confusion_matrix'] = confusion_matrix_tree
    
    # 生成详细报告
    try:
        report_base = classification_report(y_true, y_pred)
        # 添加详细说明
        explanation = """
===================================================================
指标说明:
===================================================================

• Precision (精确率/查准率):
  含义: 在所有被模型预测为正例的样本中，实际为正例的比例
  通俗理解: "模型说对的，有多少真的对了"
  数值范围: 0-1，越高越好（>0.7通常认为较好）
  使用场景: 当误报成本高时，需要高精确率

• Recall (召回率/查全率):
  含义: 在所有实际为正例的样本中，被模型正确预测为正例的比例
  通俗理解: "实际为正的，模型找到了多少"
  数值范围: 0-1，越高越好（>0.7通常认为较好）
  使用场景: 当漏报成本高时，需要高召回率

• F1-Score (F1分数):
  含义: 精确率和召回率的调和平均数，综合评估模型性能
  通俗理解: "精确率和召回率的平衡值"
  数值范围: 0-1，越高越好（>0.7通常认为较好）
  使用场景: 需要平衡精确率和召回率时

• Support (支持数):
  含义: 每个类别在测试集中实际出现的样本数量
  通俗理解: "测试集中每个类别有多少个样本"
  用途: 了解测试集的类别分布，判断数据集是否类别不平衡

• Accuracy (准确率):
  含义: 所有预测中，预测正确的比例
  注意: 当类别不平衡时，准确率可能误导

• Macro Avg (宏平均):
  含义: 所有类别指标的简单平均值，每个类别权重相同
  使用场景: 类别重要性相同，或类别样本数量差异很大时

• Weighted Avg (加权平均):
  含义: 根据每个类别的样本数量加权平均，样本多的类别权重更大
  使用场景: 关注整体性能，样本多的类别更重要时

===================================================================
如何解读报告:
===================================================================

1. 看整体性能: 查看 accuracy 和 macro avg / weighted avg 的 F1-score
2. 看各类别性能: 检查每个类别的 precision、recall、f1-score
3. 看类别平衡: 比较各类别的 support，如果差异很大，关注 macro avg
4. 根据需求选择指标:
   - 需要高精确率: 关注 precision（减少误报）
   - 需要高召回率: 关注 recall（减少漏报）
   - 需要平衡: 关注 f1-score

===================================================================
"""
        report = (
            report_base
            + explanation
            + "\n\n"
            + explain_classification_metrics(metrics_dict)
            + "\n\n"
            + verdict_classification(metrics_dict)
        )
    except Exception:
        report = (
            f"分类报告生成失败\n准确率: {metrics_dict.get('accuracy', 'N/A')}\n\n"
            + explain_classification_metrics(metrics_dict)
            + "\n\n"
            + verdict_classification(metrics_dict)
        )
    
    # 转换为JSON字符串
    metrics_json = json.dumps(metrics_dict, ensure_ascii=False, indent=2)
    
    readme = "分类模型评估完成（含通俗解读与结论）"
    
    return metrics_json, report, confusion_matrix_tree, readme


def evaluate_regression(model, X=None, y_true=None, y_pred=None, metrics='all'):
    """
    评估回归模型的性能
    
    输入参数:
        model: 训练好的回归模型对象
        X: 测试特征数据（Tree结构，可选，如果提供了y_pred则不需要）
        y_true: 真实标签数据（Tree结构或numpy数组）
        y_pred: 预测结果（Tree结构或numpy数组，可选，如果提供了X则自动预测）
        metrics: 要计算的评估指标，默认"all"（可选：mse, rmse, mae, r2, explained_variance等，或"all"）
    
    输出参数:
        metrics_json: 评估指标字典（JSON格式字符串）
        report: 详细的评估报告（字符串）
        readme: 组件使用说明
    """
    y_true = np.array(y_true).ravel() if y_true is not None else None
    
    # 如果提供了y_pred，直接使用；否则使用X进行预测
    if y_pred is not None:
        y_pred = np.array(y_pred).ravel()
    elif X is not None:
        X = np.array(X)
        y_pred = model.predict(X)
    else:
        raise ValueError("必须提供X或y_pred参数")
    
    # 确定要计算的指标
    if metrics == 'all' or metrics is None:
        metrics_list = ['mse', 'rmse', 'mae', 'r2', 'explained_variance']
    else:
        if isinstance(metrics, str):
            metrics_list = [m.strip() for m in metrics.split(',')]
        else:
            metrics_list = list(metrics)
    
    # 计算指标
    metrics_dict = {}
    
    if 'mse' in metrics_list or metrics == 'all':
        metrics_dict['mse'] = float(mean_squared_error(y_true, y_pred))
    
    if 'rmse' in metrics_list or metrics == 'all':
        mse = mean_squared_error(y_true, y_pred)
        metrics_dict['rmse'] = float(np.sqrt(mse))
    
    if 'mae' in metrics_list or metrics == 'all':
        metrics_dict['mae'] = float(mean_absolute_error(y_true, y_pred))
    
    if 'r2' in metrics_list or metrics == 'all':
        metrics_dict['r2_score'] = float(r2_score(y_true, y_pred))
    
    if 'explained_variance' in metrics_list or metrics == 'all':
        metrics_dict['explained_variance'] = float(explained_variance_score(y_true, y_pred))
    
    # 生成详细报告
    report_base = f"""回归模型评估报告
==================
均方误差 (MSE): {metrics_dict.get('mse', 'N/A'):.4f}
均方根误差 (RMSE): {metrics_dict.get('rmse', 'N/A'):.4f}
平均绝对误差 (MAE): {metrics_dict.get('mae', 'N/A'):.4f}
R² 分数: {metrics_dict.get('r2_score', 'N/A'):.4f}
解释方差: {metrics_dict.get('explained_variance', 'N/A'):.4f}
"""
    # 添加详细说明
    explanation = """
===================================================================
指标说明:
===================================================================

• MSE (均方误差 - Mean Squared Error):
  含义: 预测值与真实值差的平方的平均值
  公式: MSE = Σ(预测值 - 真实值)² / n
  特点: 对大误差敏感（平方放大了大误差的影响）
  数值范围: ≥ 0，越小越好
  单位: 与目标变量的平方单位相同

• RMSE (均方根误差 - Root Mean Squared Error):
  含义: MSE的平方根，与目标变量单位相同
  公式: RMSE = √MSE
  特点: 比MSE更直观，单位与目标变量一致
  数值范围: ≥ 0，越小越好
  单位: 与目标变量单位相同
  使用场景: 最常用的回归评估指标之一

• MAE (平均绝对误差 - Mean Absolute Error):
  含义: 预测值与真实值差的绝对值的平均值
  公式: MAE = Σ|预测值 - 真实值| / n
  特点: 对所有误差一视同仁，不受异常值影响
  数值范围: ≥ 0，越小越好
  单位: 与目标变量单位相同
  使用场景: 当不希望大误差被过度放大时

• R² (决定系数 - R-squared):
  含义: 模型解释的方差占总方差的比例
  公式: R² = 1 - (SS_res / SS_tot)
  特点: 无量纲，不受目标变量单位影响
  数值范围: -∞ 到 1
  - R² = 1: 完美拟合（模型完美预测）
  - R² = 0: 模型性能等同于简单平均值
  - R² < 0: 模型性能比简单平均值还差
  使用场景: 最常用的回归模型评估指标
  参考值: > 0.7 通常认为较好，> 0.9 非常好

• Explained Variance (解释方差):
  含义: 模型能够解释的目标变量方差比例
  特点: 与R²类似，但计算方法略有不同
  数值范围: 0 到 1，越接近1越好
  使用场景: 评估模型的解释能力

===================================================================
如何解读报告:
===================================================================

1. 看整体拟合度:
   - R²值越接近1，模型拟合越好
   - R² > 0.7 通常认为较好
   - R² > 0.9 非常好

2. 看预测误差:
   - RMSE和MAE越小，预测误差越小
   - RMSE对大误差敏感，MAE对所有误差一视同仁
   - 如果RMSE >> MAE，说明存在较大的预测误差

3. 比较不同模型:
   - 比较R²值：越高越好
   - 比较RMSE/MAE：越低越好
   - 注意单位一致性

4. 根据应用场景选择指标:
   - 关注整体拟合度: 使用R²
   - 关注预测误差大小: 使用RMSE或MAE
   - 关注异常值影响: 使用MAE（不受异常值影响）

===================================================================
"""
    report = (
        report_base
        + explanation
        + "\n\n"
        + explain_regression_metrics(metrics_dict)
        + "\n\n"
        + verdict_regression(metrics_dict)
    )
    
    # 转换为JSON字符串
    metrics_json = json.dumps(metrics_dict, ensure_ascii=False, indent=2)
    
    readme = "回归模型评估完成（含通俗解读与结论）"
    
    return metrics_json, report, readme


def evaluate_clustering(model, X=None, y_true=None, y_pred=None, metrics='all'):
    """
    评估聚类模型的性能
    
    输入参数:
        model: 训练好的聚类模型对象
        X: 测试特征数据（Tree结构，可选，如果提供了y_pred则不需要）
        y_true: 真实标签数据（可选，用于有监督评估指标）
        y_pred: 预测结果（Tree结构或numpy数组，可选，如果提供了X则自动预测）
        metrics: 要计算的评估指标，默认"all"（可选：silhouette_score, davies_bouldin_score, calinski_harabasz_score等，或"all"）
    
    输出参数:
        metrics_json: 评估指标字典（JSON格式字符串）
        report: 详细的评估报告（字符串）
        readme: 组件使用说明
    """
    y_true = np.array(y_true).ravel() if y_true is not None else None
    
    from core.model_bundle import unwrap_model
    inner = unwrap_model(model)

    # 如果提供了y_pred，直接使用；否则使用X进行预测
    if y_pred is not None:
        labels = np.array(y_pred).ravel()
    elif X is not None:
        X = np.array(X)
        # 获取聚类标签
        if hasattr(model, 'predict'):
            labels = model.predict(X)
        elif hasattr(inner, 'labels_'):
            labels = inner.labels_
        else:
            labels = model.fit_predict(X) if hasattr(model, 'fit_predict') else inner.fit_predict(X)
    else:
        raise ValueError("必须提供X或y_pred参数")

    if X is not None:
        X = np.array(X)
    
    # 确定要计算的指标
    if metrics == 'all' or metrics is None:
        metrics_list = ['silhouette_score', 'davies_bouldin_score', 'calinski_harabasz_score']
    else:
        if isinstance(metrics, str):
            metrics_list = [m.strip() for m in metrics.split(',')]
        else:
            metrics_list = list(metrics)
    
    # 计算指标
    metrics_dict = {}
    n_clusters = len(np.unique(labels[labels >= 0]))  # 排除噪声点（-1）
    metrics_dict['n_clusters'] = int(n_clusters)
    
    if X is not None and ('silhouette_score' in metrics_list or metrics == 'all'):
        try:
            if n_clusters > 1:
                metrics_dict['silhouette_score'] = float(silhouette_score(X, labels))
            else:
                metrics_dict['silhouette_score'] = None
        except Exception:
            metrics_dict['silhouette_score'] = None
    
    if X is not None and ('davies_bouldin_score' in metrics_list or metrics == 'all'):
        try:
            if n_clusters > 1:
                metrics_dict['davies_bouldin_score'] = float(davies_bouldin_score(X, labels))
            else:
                metrics_dict['davies_bouldin_score'] = None
        except Exception:
            metrics_dict['davies_bouldin_score'] = None
    
    if X is not None and ('calinski_harabasz_score' in metrics_list or metrics == 'all'):
        try:
            if n_clusters > 1:
                metrics_dict['calinski_harabasz_score'] = float(calinski_harabasz_score(X, labels))
            else:
                metrics_dict['calinski_harabasz_score'] = None
        except Exception:
            metrics_dict['calinski_harabasz_score'] = None
    
    # 有真实标签时计算监督指标
    if y_true is not None:
        y_true = np.array(y_true).ravel()
        try:
            metrics_dict['adjusted_rand_score'] = float(adjusted_rand_score(y_true, labels))
        except Exception:
            metrics_dict['adjusted_rand_score'] = None
        try:
            metrics_dict['normalized_mutual_info'] = float(normalized_mutual_info_score(y_true, labels))
        except Exception:
            metrics_dict['normalized_mutual_info'] = None
    
    # 生成详细报告
    report = f"""聚类模型评估报告
==================
聚类数量: {n_clusters}
轮廓系数: {metrics_dict.get('silhouette_score', 'N/A')}
Davies-Bouldin指数: {metrics_dict.get('davies_bouldin_score', 'N/A')}
Calinski-Harabasz指数: {metrics_dict.get('calinski_harabasz_score', 'N/A')}
调整兰德指数(ARI): {metrics_dict.get('adjusted_rand_score', 'N/A')}
归一化互信息(NMI): {metrics_dict.get('normalized_mutual_info', 'N/A')}

{explain_clustering_metrics(metrics_dict)}

{verdict_clustering(metrics_dict)}
"""
    
    # 转换为JSON字符串
    metrics_json = json.dumps(metrics_dict, ensure_ascii=False, indent=2)
    
    readme = "聚类模型评估完成（含通俗解读、结论与ARI/NMI）"
    
    return metrics_json, report, readme
