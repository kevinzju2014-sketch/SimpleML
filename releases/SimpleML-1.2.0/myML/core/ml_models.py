"""
机器学习模型管理器
提供各种机器学习算法的封装
"""

import numpy as np
from sklearn.ensemble import RandomForestClassifier, RandomForestRegressor
from sklearn.svm import SVC, SVR
from sklearn.linear_model import LogisticRegression, LinearRegression, Ridge, Lasso
from sklearn.neighbors import KNeighborsClassifier, KNeighborsRegressor
from sklearn.tree import DecisionTreeClassifier, DecisionTreeRegressor
from sklearn.naive_bayes import GaussianNB, MultinomialNB, BernoulliNB, ComplementNB, CategoricalNB
from sklearn.cluster import KMeans, DBSCAN, AgglomerativeClustering
from sklearn.model_selection import train_test_split
from sklearn.metrics import (
    accuracy_score, precision_score, recall_score, f1_score,
    r2_score, mean_squared_error, mean_absolute_error,
    confusion_matrix, classification_report
)


class MLModelManager:
    """机器学习模型管理器"""
    
    # 分类算法映射
    CLASSIFICATION_ALGORITHMS = {
        'logistic_regression': LogisticRegression,
        'svm': SVC,
        'random_forest': RandomForestClassifier,
        'knn': KNeighborsClassifier,
        'decision_tree': DecisionTreeClassifier,
        'naive_bayes': GaussianNB
    }
    
    # 回归算法映射
    REGRESSION_ALGORITHMS = {
        'linear_regression': LinearRegression,
        'random_forest': RandomForestRegressor,
        'svr': SVR,
        'ridge': Ridge,
        'lasso': Lasso,
        'knn': KNeighborsRegressor
    }
    
    # 聚类算法映射
    CLUSTERING_ALGORITHMS = {
        'kmeans': KMeans,
        'dbscan': DBSCAN,
        'agglomerative': AgglomerativeClustering
    }
    
    def __init__(self):
        self.model = None
        self.model_type = None  # 'classification', 'regression', 'clustering'
        self.algorithm = None
    
    def train_classifier(self, X, y, algorithm='random_forest', **kwargs):
        """
        训练分类模型
        
        参数:
            X: 特征数据 (numpy array 或 list)
            y: 标签数据 (numpy array 或 list)
            algorithm: 算法名称
            **kwargs: 算法特定参数
        
        返回:
            训练好的模型
        """
        X = np.array(X)
        y = np.array(y).ravel()
        
        if algorithm not in self.CLASSIFICATION_ALGORITHMS:
            raise ValueError(f"不支持的分类算法: {algorithm}")
        
        # 先处理朴素贝叶斯的type参数（必须在选择model_class之前）
        nb_type = None
        if 'type' in kwargs:
            nb_type = kwargs.pop('type')
        
        # 处理朴素贝叶斯的type参数
        if algorithm == 'naive_bayes':
            if nb_type is not None:
                # 如果提供了type参数，使用指定的朴素贝叶斯类型
                nb_classes = {
                    'GaussianNB': GaussianNB,
                    'MultinomialNB': MultinomialNB,
                    'BernoulliNB': BernoulliNB,
                    'ComplementNB': ComplementNB,
                    'CategoricalNB': CategoricalNB
                }
                if nb_type not in nb_classes:
                    raise ValueError(f"不支持的朴素贝叶斯类型: {nb_type}")
                model_class = nb_classes[nb_type]
                
                # 根据朴素贝叶斯类型过滤不相关的参数
                # GaussianNB 只接受 var_smoothing
                if nb_type == 'GaussianNB':
                    filtered_kwargs = {}
                    if 'var_smoothing' in kwargs:
                        filtered_kwargs['var_smoothing'] = kwargs['var_smoothing']
                    kwargs = filtered_kwargs
                # MultinomialNB, BernoulliNB, ComplementNB 接受 alpha, fit_prior, class_prior
                elif nb_type in ['MultinomialNB', 'BernoulliNB', 'ComplementNB']:
                    filtered_kwargs = {}
                    if 'alpha' in kwargs:
                        filtered_kwargs['alpha'] = kwargs['alpha']
                    if 'fit_prior' in kwargs:
                        filtered_kwargs['fit_prior'] = kwargs['fit_prior']
                    if 'class_prior' in kwargs:
                        filtered_kwargs['class_prior'] = kwargs['class_prior']
                    kwargs = filtered_kwargs
                # CategoricalNB 接受 alpha, fit_prior, class_prior, min_categories
                elif nb_type == 'CategoricalNB':
                    filtered_kwargs = {}
                    if 'alpha' in kwargs:
                        filtered_kwargs['alpha'] = kwargs['alpha']
                    if 'fit_prior' in kwargs:
                        filtered_kwargs['fit_prior'] = kwargs['fit_prior']
                    if 'class_prior' in kwargs:
                        filtered_kwargs['class_prior'] = kwargs['class_prior']
                    if 'min_categories' in kwargs:
                        filtered_kwargs['min_categories'] = kwargs['min_categories']
                    kwargs = filtered_kwargs
            else:
                # 如果没有提供type参数，使用默认的GaussianNB
                model_class = self.CLASSIFICATION_ALGORITHMS[algorithm]
                # GaussianNB 只接受 var_smoothing，过滤其他参数
                filtered_kwargs = {}
                if 'var_smoothing' in kwargs:
                    filtered_kwargs['var_smoothing'] = kwargs['var_smoothing']
                kwargs = filtered_kwargs
        else:
            # 对于非朴素贝叶斯算法，确保type参数已被移除（如果之前没有移除）
            if 'type' in kwargs:
                kwargs.pop('type')
            model_class = self.CLASSIFICATION_ALGORITHMS[algorithm]
        
        # 处理algorithm_type参数（用于KNN等算法，避免与我们的algorithm键冲突）
        if 'algorithm_type' in kwargs:
            kwargs['algorithm'] = kwargs.pop('algorithm_type')
        
        self.model = model_class(**kwargs)
        self.model.fit(X, y)
        self.model_type = 'classification'
        self.algorithm = algorithm
        
        return self.model
    
    def train_regressor(self, X, y, algorithm='random_forest', **kwargs):
        """
        训练回归模型
        
        参数:
            X: 特征数据
            y: 目标值
            algorithm: 算法名称
            **kwargs: 算法特定参数
        
        返回:
            训练好的模型
        """
        # 回归任务必须是数值型
        X = np.array(X)
        y = np.array(y).ravel()

        # 尽量把字符串/对象数组转成 float，避免 KNN 等模型在预测时对字符串做均值导致崩溃
        if hasattr(X, 'dtype') and X.dtype.kind in ('U', 'S', 'O'):
            try:
                X = X.astype(float)
            except Exception:
                raise ValueError("回归模型训练失败：X 中包含非数值内容（例如文本）。请确保特征列都是数值。")
        if hasattr(y, 'dtype') and y.dtype.kind in ('U', 'S', 'O'):
            try:
                y = y.astype(float)
            except Exception:
                raise ValueError("回归模型训练失败：y（目标值）必须是数值，但检测到文本/非数值内容。请检查 Dataset 的标签列。")
        
        # 算法名称映射（将组件输出的名称映射到内部使用的名称）
        algorithm_mapping = {
            'knn_regressor': 'knn',
            'lasso_regression': 'lasso',
            'ridge_regression': 'ridge',
            'random_forest_regressor': 'random_forest',
            'linear_regression': 'linear_regression'  # 保持不变
        }
        
        # 如果算法名称在映射中，使用映射后的名称
        if algorithm in algorithm_mapping:
            algorithm = algorithm_mapping[algorithm]
        
        if algorithm not in self.REGRESSION_ALGORITHMS:
            raise ValueError(f"不支持的回归算法: {algorithm}")
        
        model_class = self.REGRESSION_ALGORITHMS[algorithm]
        
        # 处理algorithm_type参数（用于KNN等算法，避免与我们的algorithm键冲突）
        if 'algorithm_type' in kwargs:
            kwargs['algorithm'] = kwargs.pop('algorithm_type')
        
        self.model = model_class(**kwargs)
        self.model.fit(X, y)
        self.model_type = 'regression'
        self.algorithm = algorithm
        
        return self.model
    
    def train_cluster(self, X, algorithm='kmeans', n_clusters=3, **kwargs):
        """
        训练聚类模型
        
        参数:
            X: 特征数据
            algorithm: 算法名称
            n_clusters: 聚类数量（某些算法需要，如果kwargs中已有则使用kwargs中的）
            **kwargs: 算法特定参数
        
        返回:
            训练好的模型
        """
        X = np.array(X)
        
        if algorithm not in self.CLUSTERING_ALGORITHMS:
            raise ValueError(f"不支持的聚类算法: {algorithm}")
        
        model_class = self.CLUSTERING_ALGORITHMS[algorithm]
        
        # 处理algorithm_type参数（用于K-Means、DBSCAN等算法）
        if 'algorithm_type' in kwargs:
            kwargs['algorithm'] = kwargs.pop('algorithm_type')
        
        # 为需要 n_clusters 的算法设置参数（如果kwargs中没有）
        if algorithm in ['kmeans', 'agglomerative']:
            if 'n_clusters' not in kwargs:
                kwargs['n_clusters'] = n_clusters
        
        self.model = model_class(**kwargs)
        self.model.fit(X)
        self.model_type = 'clustering'
        self.algorithm = algorithm
        
        return self.model
    
    def predict(self, X):
        """
        使用训练好的模型进行预测
        
        参数:
            X: 特征数据
        
        返回:
            预测结果
        """
        if self.model is None:
            raise ValueError("模型尚未训练，请先训练模型")
        
        X = np.array(X)
        
        if self.model_type == 'clustering':
            return self.model.predict(X)
        else:
            return self.model.predict(X)
    
    def predict_proba(self, X):
        """
        预测概率（仅分类模型）
        
        参数:
            X: 特征数据
        
        返回:
            预测概率
        """
        if self.model is None:
            raise ValueError("模型尚未训练，请先训练模型")
        
        if self.model_type != 'classification':
            raise ValueError("只有分类模型支持概率预测")
        
        if not hasattr(self.model, 'predict_proba'):
            raise ValueError(f"算法 {self.algorithm} 不支持概率预测")
        
        X = np.array(X)
        return self.model.predict_proba(X)
    
    def evaluate_classification(self, X, y):
        """
        评估分类模型
        
        返回:
            评估指标字典
        """
        if self.model is None or self.model_type != 'classification':
            raise ValueError("需要训练好的分类模型")
        
        y_pred = self.predict(X)
        y = np.array(y).ravel()
        
        metrics = {
            'accuracy': accuracy_score(y, y_pred),
            'precision': precision_score(y, y_pred, average='weighted', zero_division=0),
            'recall': recall_score(y, y_pred, average='weighted', zero_division=0),
            'f1_score': f1_score(y, y_pred, average='weighted', zero_division=0),
            'confusion_matrix': confusion_matrix(y, y_pred).tolist()
        }
        
        return metrics
    
    def evaluate_regression(self, X, y):
        """
        评估回归模型
        
        返回:
            评估指标字典
        """
        if self.model is None or self.model_type != 'regression':
            raise ValueError("需要训练好的回归模型")
        
        y_pred = self.predict(X)
        y = np.array(y).ravel()
        
        metrics = {
            'r2_score': r2_score(y, y_pred),
            'mse': mean_squared_error(y, y_pred),
            'rmse': np.sqrt(mean_squared_error(y, y_pred)),
            'mae': mean_absolute_error(y, y_pred)
        }
        
        return metrics
    
    def evaluate_clustering(self, X):
        """
        评估聚类模型
        
        返回:
            聚类结果和标签
        """
        if self.model is None or self.model_type != 'clustering':
            raise ValueError("需要训练好的聚类模型")
        
        labels = self.predict(X)
        
        return {
            'labels': labels.tolist(),
            'n_clusters': len(np.unique(labels))
        }
    
    def get_model(self):
        """获取当前模型"""
        return self.model
    
    def get_available_algorithms(self, model_type):
        """
        获取可用的算法列表
        
        参数:
            model_type: 'classification', 'regression', 或 'clustering'
        """
        if model_type == 'classification':
            return list(self.CLASSIFICATION_ALGORITHMS.keys())
        elif model_type == 'regression':
            return list(self.REGRESSION_ALGORITHMS.keys())
        elif model_type == 'clustering':
            return list(self.CLUSTERING_ALGORITHMS.keys())
        else:
            raise ValueError(f"不支持的类型: {model_type}")
