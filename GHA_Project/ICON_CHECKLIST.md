# 图标文件检查清单

## 当前图标文件状态

根据 `ComponentIconMap.cs` 的映射关系，需要以下23个图标文件：

### 必需的文件列表

#### 共享图标（8个）
1. `file_io_components.png` - 用于 ReadCSV, ReadExcel, WriteCSV, WriteExcel
2. `statistics_components.png` - 用于 CalculateStatistics, CalculateCorrelation, GetDataSummary, DescribeFeatures
3. `dataset_components.png` - 用于 CreateDataset, DeconstructDataset, SplitDataset
4. `train_components.png` - 用于 TrainClassifier, TrainRegressor, TrainCluster
5. `predict_components.png` - 用于 PredictClassifier, PredictRegressor, PredictCluster
6. `evaluate_components.png` - 用于 EvaluateClassification, EvaluateRegression, EvaluateClustering
7. `model_io_components.png` - 用于 SaveModel, LoadModel
8. `dataset_loader.png` - 用于 LoadDataset

#### 算法特定图标（15个）
9. `train_random_forest_classifier.png`
10. `train_svm_classifier.png`
11. `train_knn_classifier.png`
12. `train_logistic_regression_classifier.png`
13. `train_naive_bayes_classifier.png`
14. `train_decision_tree_classifier.png`
15. `train_random_forest_regressor.png`
16. `train_svr.png`
17. `train_linear_regression.png`
18. `train_ridge_regression.png`
19. `train_lasso_regression.png`
20. `train_knn_regressor.png`
21. `train_kmeans.png`
22. `train_dbscan.png`
23. `train_agglomerative_clustering.png`

## 重命名映射表

### 从当前文件名创建共享图标

| 当前文件名 | → | 目标文件名 |
|-----------|---|-----------|
| `read_csv.png` | → | `file_io_components.png` |
| `calculate_statistics.png` | → | `statistics_components.png` |
| `create_dataset.png` | → | `dataset_components.png` |
| `train_classifier.png` | → | `train_components.png` |
| `predict_classifier.png` | → | `predict_components.png` |
| `evaluate_classification.png` | → | `evaluate_components.png` |
| `save_model.png` | → | `model_io_components.png` |
| `load_dataset.png` | → | `dataset_loader.png` |

### 从当前文件名创建算法特定图标（添加train_前缀）

| 当前文件名 | → | 目标文件名 |
|-----------|---|-----------|
| `random_forest_classifier.png` | → | `train_random_forest_classifier.png` |
| `support_vector_machine_classifier.png` | → | `train_svm_classifier.png` |
| `k_nearest_neighbors_classifier.png` | → | `train_knn_classifier.png` |
| `logistic_regression.png` | → | `train_logistic_regression_classifier.png` |
| `naive_bayes_classifier.png` | → | `train_naive_bayes_classifier.png` |
| `decision_tree_classifier.png` | → | `train_decision_tree_classifier.png` |
| `random_forest_regressor.png` | → | `train_random_forest_regressor.png` |
| `support_vector_regression.png` | → | `train_svr.png` |
| `linear_regression.png` | → | `train_linear_regression.png` |
| `ridge_regression.png` | → | `train_ridge_regression.png` |
| `lasso_regression.png` | → | `train_lasso_regression.png` |
| `k_nearest_neighbors_regressor.png` | → | `train_knn_regressor.png` |
| `k_means.png` | → | `train_kmeans.png` |
| `density_baised_spatial_clistering_of_applications_with_noise.png` | → | `train_dbscan.png` |
| `agglomerative_clustering.png` | → | `train_agglomerative_clustering.png` |

## 快速操作指南

### 方法1：使用Python脚本（推荐）

在 `GHA_Project` 目录下运行：
```bash
python prepare_icons.py
```

### 方法2：手动操作

1. 进入 `icons` 目录
2. 复制文件并重命名（参考上表）
3. 确保所有23个必需文件都存在

### 方法3：使用Windows资源管理器

1. 打开 `icons` 文件夹
2. 复制源文件
3. 粘贴并重命名为目标文件名

## 验证

运行以下Python代码验证：

```python
import os
d = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'
required = ['file_io_components.png', 'statistics_components.png', 'dataset_components.png', 'train_components.png', 'predict_components.png', 'evaluate_components.png', 'model_io_components.png', 'dataset_loader.png', 'train_random_forest_classifier.png', 'train_svm_classifier.png', 'train_knn_classifier.png', 'train_logistic_regression_classifier.png', 'train_naive_bayes_classifier.png', 'train_decision_tree_classifier.png', 'train_random_forest_regressor.png', 'train_svr.png', 'train_linear_regression.png', 'train_ridge_regression.png', 'train_lasso_regression.png', 'train_knn_regressor.png', 'train_kmeans.png', 'train_dbscan.png', 'train_agglomerative_clustering.png']
files = {f.lower(): f for f in os.listdir(d) if f.endswith('.png')}
missing = [r for r in required if r.lower() not in files]
print('缺失文件:' if missing else '✓ 所有文件都存在！')
[print(f'  - {m}') for m in missing]
```

## 准备生成GHA文件

完成图标文件重命名后，可以：

1. **验证图标文件**：确保所有23个必需文件都存在
2. **构建项目**：使用 Visual Studio 或 MSBuild 构建项目
3. **生成GHA文件**：构建完成后会自动生成 `SimpleML.gha` 文件
