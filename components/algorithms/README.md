# 算法特定电池模块

本模块包含每个机器学习算法的独立训练函数，每个函数都包含该算法的所有特定参数。

## 快速开始

```python
import sys
sys.path.append(r'D:\Helio\250928_机器学习课程\myML')

# 导入算法函数
from components.algorithms import train_random_forest_regressor

# 使用
model, manager = train_random_forest_regressor(
    X_train, y_train,
    n_estimators=100,
    max_depth=15
)
```

## 可用算法

### 分类算法
- `train_random_forest_classifier` - 随机森林分类器
- `train_svm_classifier` - 支持向量机分类器
- `train_logistic_regression_classifier` - 逻辑回归分类器
- `train_knn_classifier` - K近邻分类器
- `train_decision_tree_classifier` - 决策树分类器
- `train_naive_bayes_classifier` - 朴素贝叶斯分类器

### 回归算法
- `train_random_forest_regressor` - 随机森林回归器
- `train_svr` - 支持向量回归器
- `train_linear_regression` - 线性回归器
- `train_ridge_regression` - 岭回归器
- `train_lasso_regression` - Lasso回归器
- `train_knn_regressor` - K近邻回归器

### 聚类算法
- `train_kmeans` - K-Means聚类
- `train_dbscan` - DBSCAN聚类
- `train_agglomerative_clustering` - 层次聚类

## 详细文档

请参考 `ALGORITHMS_GUIDE.md` 获取每个算法的详细参数说明和使用示例。
