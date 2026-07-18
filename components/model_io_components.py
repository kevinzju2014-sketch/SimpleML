"""
模型保存和加载组件
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
# 跨平台引导 Rhino / 系统 site-packages
try:
    from core.env_bootstrap import bootstrap_python_paths
    bootstrap_python_paths(project_dir)
except Exception:
    pass

from core.model_io import ModelIO
from core.ml_models import MLModelManager


def save_model(model, filepath, model_type=None, algorithm=None, metrics=None):
    """
    保存模型组件
    
    输入:
        model: 要保存的模型对象
        filepath: 保存路径（字符串，可以包含 .pkl 扩展名）
        model_type: 模型类型（可选，'classification', 'regression', 'clustering'）
        algorithm: 算法名称（可选）
        metrics: 评估指标（可选，字典）
    
    输出:
        saved_path: 保存的文件路径
    """
    # 自动提取模型信息（如果未提供）
    metadata = {}
    
    # 如果提供了 model_type 和 algorithm，直接使用
    if model_type:
        metadata['model_type'] = model_type
    else:
        # 自动检测模型类型
        model_class_name = type(model).__name__
        if 'Classifier' in model_class_name or 'NB' in model_class_name:
            metadata['model_type'] = 'classification'
        elif 'Regressor' in model_class_name or 'Regression' in model_class_name:
            metadata['model_type'] = 'regression'
        elif 'Cluster' in model_class_name or 'KMeans' in model_class_name or 'DBSCAN' in model_class_name:
            metadata['model_type'] = 'clustering'
        else:
            # 通过方法判断
            if hasattr(model, 'predict_proba'):
                metadata['model_type'] = 'classification'
            elif hasattr(model, 'predict'):
                # 可能是回归或聚类，进一步判断
                if hasattr(model, 'n_clusters') or hasattr(model, 'labels_'):
                    metadata['model_type'] = 'clustering'
                else:
                    metadata['model_type'] = 'regression'
    
    # 如果提供了 algorithm，直接使用
    if algorithm:
        metadata['algorithm'] = algorithm
    else:
        # 自动检测算法名称
        model_class_name = type(model).__name__
        # 映射常见的模型类名到算法名称
        algorithm_map = {
            'LogisticRegression': 'logistic_regression',
            'SVC': 'svm',
            'SVR': 'svr',
            'RandomForestClassifier': 'random_forest',
            'RandomForestRegressor': 'random_forest',
            'KNeighborsClassifier': 'knn',
            'KNeighborsRegressor': 'knn',
            'DecisionTreeClassifier': 'decision_tree',
            'DecisionTreeRegressor': 'decision_tree',
            'GaussianNB': 'naive_bayes',
            'MultinomialNB': 'naive_bayes',
            'BernoulliNB': 'naive_bayes',
            'ComplementNB': 'naive_bayes',
            'CategoricalNB': 'naive_bayes',
            'LinearRegression': 'linear_regression',
            'Ridge': 'ridge',
            'Lasso': 'lasso',
            'KMeans': 'kmeans',
            'DBSCAN': 'dbscan',
            'AgglomerativeClustering': 'agglomerative'
        }
        metadata['algorithm'] = algorithm_map.get(model_class_name, model_class_name.lower())
    
    # 提取特征数量（如果模型有该属性）
    if hasattr(model, 'n_features_in_'):
        metadata['n_features'] = int(model.n_features_in_)
    
    # 提取类别数量（分类模型）
    if metadata.get('model_type') == 'classification' and hasattr(model, 'classes_'):
        try:
            metadata['n_classes'] = len(model.classes_)
        except:
            pass
    
    # 提取聚类数量（聚类模型）
    if metadata.get('model_type') == 'clustering' and hasattr(model, 'n_clusters'):
        try:
            metadata['n_clusters'] = int(model.n_clusters)
        except:
            pass
    
    # 添加评估指标（如果提供）
    if metrics:
        metadata['metrics'] = metrics
    
    # 保存模型
    return ModelIO.save_model(model, filepath, metadata)


def save_model_with_manager(model_manager, filepath, metrics=None):
    """
    使用模型管理器保存模型组件
    
    输入:
        model_manager: MLModelManager 对象
        filepath: 保存路径
        metrics: 评估指标（可选）
    
    输出:
        saved_path: 保存的文件路径
    """
    model = model_manager.get_model()
    model_type = model_manager.model_type
    algorithm = model_manager.algorithm
    
    return save_model(model, filepath, model_type, algorithm, metrics)


def load_model(filepath):
    """
    加载模型组件
    
    输入:
        filepath: 模型文件路径（字符串）
    
    输出:
        model: 加载的模型对象
        metadata: 模型元数据字典
    """
    return ModelIO.load_model(filepath)


def load_model_as_manager(filepath):
    """
    加载模型并创建模型管理器对象
    
    输入:
        filepath: 模型文件路径
    
    输出:
        model_manager: MLModelManager 对象（包含加载的模型）
        metadata: 模型元数据
    """
    model, metadata = ModelIO.load_model(filepath)
    
    manager = MLModelManager()
    manager.model = model
    manager.model_type = metadata.get('model_type')
    manager.algorithm = metadata.get('algorithm')
    
    return manager, metadata


def list_models(directory='.'):
    """
    列出目录中的模型文件组件
    
    输入:
        directory: 要搜索的目录路径（字符串，默认为当前目录）
    
    输出:
        model_list: 模型文件信息列表
    """
    return ModelIO.list_models(directory)


def load_model_for_prediction(filepath):
    """
    加载模型用于预测组件（推荐使用）
    
    输入:
        filepath: 模型文件路径（字符串）
    
    输出:
        model_manager: MLModelManager 对象（可直接用于predict_components）
        model_info: 模型信息字典
            - model_type: 模型类型（'classification', 'regression', 'clustering'）
            - algorithm: 算法名称
            - metrics: 评估指标（如果有）
            - saved_at: 保存时间
            - filepath: 文件路径
    
    使用示例:
        # 加载模型
        manager, info = load_model_for_prediction("model.pkl")
        
        # 直接用于预测
        predictions = predict(manager, X)
    """
    model, metadata = ModelIO.load_model(filepath)
    
    manager = MLModelManager()
    manager.model = model
    manager.model_type = metadata.get('model_type')
    manager.algorithm = metadata.get('algorithm')
    
    # 构建模型信息字典
    model_info = {
        'model_type': metadata.get('model_type', 'unknown'),
        'algorithm': metadata.get('algorithm', 'unknown'),
        'metrics': metadata.get('metrics'),
        'saved_at': metadata.get('saved_at'),
        'filepath': filepath
    }
    
    return manager, model_info
