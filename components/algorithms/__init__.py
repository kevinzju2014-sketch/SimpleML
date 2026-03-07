"""
算法特定训练组件模块
每个文件对应一个特定的机器学习算法，包含该算法的所有参数
"""

# 分类算法
from .train_random_forest_classifier import train_random_forest_classifier
from .train_svm_classifier import train_svm_classifier
from .train_logistic_regression_classifier import train_logistic_regression_classifier
from .train_knn_classifier import train_knn_classifier
from .train_decision_tree_classifier import train_decision_tree_classifier
from .train_naive_bayes_classifier import train_naive_bayes_classifier

# 回归算法
from .train_random_forest_regressor import train_random_forest_regressor
from .train_svr import train_svr
from .train_linear_regression import train_linear_regression
from .train_ridge_regression import train_ridge_regression
from .train_lasso_regression import train_lasso_regression
from .train_knn_regressor import train_knn_regressor

# 聚类算法
from .train_kmeans import train_kmeans
from .train_dbscan import train_dbscan
from .train_agglomerative_clustering import train_agglomerative_clustering

__all__ = [
    # 分类算法
    'train_random_forest_classifier',
    'train_svm_classifier',
    'train_logistic_regression_classifier',
    'train_knn_classifier',
    'train_decision_tree_classifier',
    'train_naive_bayes_classifier',
    # 回归算法
    'train_random_forest_regressor',
    'train_svr',
    'train_linear_regression',
    'train_ridge_regression',
    'train_lasso_regression',
    'train_knn_regressor',
    # 聚类算法
    'train_kmeans',
    'train_dbscan',
    'train_agglomerative_clustering'
]
