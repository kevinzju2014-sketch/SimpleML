# 图标文件映射关系报告

## 已更新的映射关系

`ComponentIconMap.cs` 已更新，现在每个组件都使用对应的图标文件名（按命令名称）。

## 图标文件检查

### ✅ 已映射的图标文件（38个）

#### 数据输入组件（5个）
- `read_csv.png` → ReadCSVComponent
- `read_excel.png` → ReadExcelComponent
- `write_csv.png` → WriteCSVComponent
- `write_excel.png` → WriteExcelComponent
- `load_dataset.png` → LoadDatasetComponent

#### 数据分析组件（4个）
- `calculate_statistics.png` → CalculateStatisticsComponent
- `calculate correlation.png` → CalculateCorrelationComponent ⚠️ **注意：文件名中有空格**
- `get_data_summary.png` → GetDataSummaryComponent
- `describe_features.png` → DescribeFeaturesComponent

#### 数据集组件（3个）
- `create_dataset.png` → CreateDatasetComponent
- `deconstruct_dataset.png` → DeconstructDatasetComponent
- `split_data.png` → SplitDatasetComponent

#### 训练组件（3个）
- `train_classifier.png` → TrainClassifierComponent
- `train_regressor.png` → TrainRegressorComponent
- `train_cluster.png` → TrainClusterComponent

#### 算法特定训练组件 - 分类（6个）
- `random_forest_classifier.png` → TrainRandomForestClassifierComponent
- `support_vector_machine_classifier.png` → TrainSVMClassifierComponent
- `k_nearest_neighbors_classifier.png` → TrainKNNClassifierComponent
- `logistic_regression.png` → TrainLogisticRegressionClassifierComponent
- `naive_bayes_classifier.png` → TrainNaiveBayesClassifierComponent
- `decision_tree_classifier.png` → TrainDecisionTreeClassifierComponent

#### 算法特定训练组件 - 回归（6个）
- `random_forest_regressor.png` → TrainRandomForestRegressorComponent
- `support_vector_regression.png` → TrainSVRComponent
- `linear_regression.png` → TrainLinearRegressionComponent
- `ridge_regression.png` → TrainRidgeRegressionComponent
- `lasso_regression.png` → TrainLassoRegressionComponent
- `k_nearest_neighbors_regressor.png` → TrainKNNRegressorComponent

#### 算法特定训练组件 - 聚类（3个）
- `k_means.png` → TrainKMeansComponent
- `density_baised_spatial_clistering_of_applications_with_noise.png` → TrainDBSCANComponent
- `agglomerative_clustering.png` → TrainAgglomerativeClusteringComponent

#### 预测组件（3个）
- `predict_classifier.png` → PredictClassifierComponent
- `predict_regressor.png` → PredictRegressorComponent
- `predict_cluster.png` → PredictClusterComponent

#### 评估组件（3个）
- `evaluate_classification.png` → EvaluateClassificationComponent
- `evaluate_regression.png` → EvaluateRegressionComponent
- `evaluate_clustering.png` → EvaluateClusteringComponent

#### 模型IO组件（2个）
- `save_model.png` → SaveModelComponent
- `load_model.png` → LoadModelComponent

### ⚠️ 需要注意的问题

1. **`calculate correlation.png`** - 文件名中包含空格，在代码中已正确处理，但建议重命名为 `calculate_correlation.png` 以避免潜在问题。

2. **`DataComponents`** - 这个组件在映射中使用了 `data_components.png`，但该图标文件不存在。如果这个组件实际存在，需要创建对应的图标文件。

3. **`AboutComponent`** 和 **`InstallationGuideComponent`** - 这两个组件没有在 `ComponentIconMap.cs` 中映射。如果需要为它们添加图标，请：
   - 创建对应的图标文件
   - 在 `ComponentIconMap.cs` 中添加映射

## 需要手动处理的组件

### 没有图标文件的组件

1. **DataComponents** - 需要 `data_components.png` 图标文件
2. **AboutComponent** - 如果需要图标，请创建 `about.png` 并在 `ComponentIconMap.cs` 中添加映射
3. **InstallationGuideComponent** - 如果需要图标，请创建 `installation_guide.png` 并在 `ComponentIconMap.cs` 中添加映射

## 建议的操作

1. **重命名 `calculate correlation.png`**：
   ```bash
   # 重命名为 calculate_correlation.png
   # 然后更新 ComponentIconMap.cs 中的映射
   ```

2. **为 DataComponents 创建图标**（如果该组件存在且需要图标）

3. **为 AboutComponent 和 InstallationGuideComponent 添加图标**（如果需要）

## 验证

运行以下代码验证所有映射是否正确：

```python
import os
from pathlib import Path

icons_dir = Path(r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons')
icon_files = {f.name.lower(): f.name for f in icons_dir.glob('*.png')}

required_icons = [
    'read_csv.png',
    'read_excel.png',
    'write_csv.png',
    'write_excel.png',
    'load_dataset.png',
    'calculate_statistics.png',
    'calculate correlation.png',  # 注意空格
    'get_data_summary.png',
    'describe_features.png',
    'create_dataset.png',
    'deconstruct_dataset.png',
    'split_data.png',
    'train_classifier.png',
    'train_regressor.png',
    'train_cluster.png',
    'random_forest_classifier.png',
    'support_vector_machine_classifier.png',
    'k_nearest_neighbors_classifier.png',
    'logistic_regression.png',
    'naive_bayes_classifier.png',
    'decision_tree_classifier.png',
    'random_forest_regressor.png',
    'support_vector_regression.png',
    'linear_regression.png',
    'ridge_regression.png',
    'lasso_regression.png',
    'k_nearest_neighbors_regressor.png',
    'k_means.png',
    'density_baised_spatial_clistering_of_applications_with_noise.png',
    'agglomerative_clustering.png',
    'predict_classifier.png',
    'predict_regressor.png',
    'predict_cluster.png',
    'evaluate_classification.png',
    'evaluate_regression.png',
    'evaluate_clustering.png',
    'save_model.png',
    'load_model.png',
]

missing = [icon for icon in required_icons if icon.lower() not in icon_files]
if missing:
    print('缺失的图标文件:')
    for icon in missing:
        print(f'  - {icon}')
else:
    print('✓ 所有必需的图标文件都存在！')
```
