# 图标文件重命名指南

根据 `ComponentIconMap.cs` 中的映射关系，需要将图标文件重命名为以下名称：

## 需要的图标文件列表

### 共享图标（多个组件使用同一个图标）

| 目标文件名 | 使用的组件 | 可用的源文件 |
|-----------|-----------|-------------|
| `file_io_components.png` | ReadCSV, ReadExcel, WriteCSV, WriteExcel | `read_csv.png` |
| `statistics_components.png` | CalculateStatistics, CalculateCorrelation, GetDataSummary, DescribeFeatures | `calculate_statistics.png` |
| `dataset_components.png` | CreateDataset, DeconstructDataset, SplitDataset | `create_dataset.png` |
| `train_components.png` | TrainClassifier, TrainRegressor, TrainCluster | `train_classifier.png` |
| `predict_components.png` | PredictClassifier, PredictRegressor, PredictCluster | `predict_classifier.png` |
| `evaluate_components.png` | EvaluateClassification, EvaluateRegression, EvaluateClustering | `evaluate_classification.png` |
| `model_io_components.png` | SaveModel, LoadModel | `save_model.png` |
| `dataset_loader.png` | LoadDataset | `load_dataset.png` |

### 算法特定图标（需要添加 `train_` 前缀）

| 目标文件名 | 源文件名 |
|-----------|---------|
| `train_random_forest_classifier.png` | `random_forest_classifier.png` |
| `train_svm_classifier.png` | `support_vector_machine_classifier.png` |
| `train_knn_classifier.png` | `k_nearest_neighbors_classifier.png` |
| `train_logistic_regression_classifier.png` | `logistic_regression.png` |
| `train_naive_bayes_classifier.png` | `naive_bayes_classifier.png` |
| `train_decision_tree_classifier.png` | `decision_tree_classifier.png` |
| `train_random_forest_regressor.png` | `random_forest_regressor.png` |
| `train_svr.png` | `support_vector_regression.png` |
| `train_linear_regression.png` | `linear_regression.png` |
| `train_ridge_regression.png` | `ridge_regression.png` |
| `train_lasso_regression.png` | `lasso_regression.png` |
| `train_knn_regressor.png` | `k_nearest_neighbors_regressor.png` |
| `train_kmeans.png` | `k_means.png` |
| `train_dbscan.png` | `density_baised_spatial_clistering_of_applications_with_noise.png` |
| `train_agglomerative_clustering.png` | `agglomerative_clustering.png` |

## 重命名步骤

### 方法1：使用Python脚本（推荐）

在 `icons` 目录下运行以下Python代码：

```python
import os
import shutil

d = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'
os.chdir(d)

# 获取当前文件（大小写不敏感）
files = {f.lower(): f for f in os.listdir('.') if f.endswith('.png')}

# 共享图标映射
shared_mappings = [
    ('read_csv.png', 'file_io_components.png'),
    ('calculate_statistics.png', 'statistics_components.png'),
    ('create_dataset.png', 'dataset_components.png'),
    ('train_classifier.png', 'train_components.png'),
    ('predict_classifier.png', 'predict_components.png'),
    ('evaluate_classification.png', 'evaluate_components.png'),
    ('save_model.png', 'model_io_components.png'),
    ('load_dataset.png', 'dataset_loader.png'),
]

# 算法特定图标映射
algorithm_mappings = [
    ('random_forest_classifier.png', 'train_random_forest_classifier.png'),
    ('support_vector_machine_classifier.png', 'train_svm_classifier.png'),
    ('k_nearest_neighbors_classifier.png', 'train_knn_classifier.png'),
    ('logistic_regression.png', 'train_logistic_regression_classifier.png'),
    ('naive_bayes_classifier.png', 'train_naive_bayes_classifier.png'),
    ('decision_tree_classifier.png', 'train_decision_tree_classifier.png'),
    ('random_forest_regressor.png', 'train_random_forest_regressor.png'),
    ('support_vector_regression.png', 'train_svr.png'),
    ('linear_regression.png', 'train_linear_regression.png'),
    ('ridge_regression.png', 'train_ridge_regression.png'),
    ('lasso_regression.png', 'train_lasso_regression.png'),
    ('k_nearest_neighbors_regressor.png', 'train_knn_regressor.png'),
    ('k_means.png', 'train_kmeans.png'),
    ('density_baised_spatial_clistering_of_applications_with_noise.png', 'train_dbscan.png'),
    ('agglomerative_clustering.png', 'train_agglomerative_clustering.png'),
]

# 执行复制
all_mappings = shared_mappings + algorithm_mappings
created = 0
for source, target in all_mappings:
    if source.lower() in files and not os.path.exists(target):
        shutil.copy2(files[source.lower()], target)
        print(f'✓ 创建 {target} (从 {files[source.lower()]})')
        created += 1
    elif os.path.exists(target):
        print(f'✓ {target} 已存在')
    else:
        print(f'⚠ 源文件不存在: {source}')

print(f'\n完成！创建了 {created} 个文件')
```

### 方法2：手动复制

在Windows资源管理器中，进入 `icons` 目录，手动复制文件并重命名。

## 验证

运行以下代码验证所有必需文件是否存在：

```python
import os

d = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'
required = [
    'file_io_components.png',
    'statistics_components.png',
    'dataset_components.png',
    'train_components.png',
    'predict_components.png',
    'evaluate_components.png',
    'model_io_components.png',
    'dataset_loader.png',
    'train_random_forest_classifier.png',
    'train_svm_classifier.png',
    'train_knn_classifier.png',
    'train_logistic_regression_classifier.png',
    'train_naive_bayes_classifier.png',
    'train_decision_tree_classifier.png',
    'train_random_forest_regressor.png',
    'train_svr.png',
    'train_linear_regression.png',
    'train_ridge_regression.png',
    'train_lasso_regression.png',
    'train_knn_regressor.png',
    'train_kmeans.png',
    'train_dbscan.png',
    'train_agglomerative_clustering.png',
]

files = {f.lower(): f for f in os.listdir(d) if f.endswith('.png')}
missing = [r for r in required if r.lower() not in files]

if missing:
    print(f'缺失 {len(missing)} 个文件:')
    for m in missing:
        print(f'  - {m}')
else:
    print('✓ 所有必需文件都存在！')
```

## 注意事项

1. **共享图标**：多个组件使用同一个图标文件，只需要一个文件即可
2. **算法特定图标**：每个算法组件需要独立的图标文件，文件名以 `train_` 开头
3. **文件保留**：原始文件会被保留，新文件是通过复制创建的
4. **大小写**：文件名匹配是大小写不敏感的
