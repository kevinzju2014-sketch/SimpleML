# 图标文件设置说明

## 问题
您绘制的图标文件需要重命名为 `ComponentIconMap.cs` 中期望的名称，才能正确集成到GHA插件中。

## 快速解决方案

### 方法1：双击运行Python脚本（推荐）

1. 进入 `GHA_Project` 目录
2. 双击运行 `copy_icons.py`
3. 脚本会自动创建所有必需的图标文件

### 方法2：手动复制文件

在 `icons` 目录中，需要创建以下文件：

#### 共享图标（8个）
- 复制 `read_csv.png` → `file_io_components.png`
- 复制 `calculate_statistics.png` → `statistics_components.png`
- 复制 `create_dataset.png` → `dataset_components.png`
- 复制 `train_classifier.png` → `train_components.png`
- 复制 `predict_classifier.png` → `predict_components.png`
- 复制 `evaluate_classification.png` → `evaluate_components.png`
- 复制 `save_model.png` → `model_io_components.png`
- 复制 `load_dataset.png` → `dataset_loader.png`

#### 算法特定图标（15个）
- 复制 `random_forest_classifier.png` → `train_random_forest_classifier.png`
- 复制 `support_vector_machine_classifier.png` → `train_svm_classifier.png`
- 复制 `k_nearest_neighbors_classifier.png` → `train_knn_classifier.png`
- 复制 `logistic_regression.png` → `train_logistic_regression_classifier.png`
- 复制 `naive_bayes_classifier.png` → `train_naive_bayes_classifier.png`
- 复制 `decision_tree_classifier.png` → `train_decision_tree_classifier.png`
- 复制 `random_forest_regressor.png` → `train_random_forest_regressor.png`
- 复制 `support_vector_regression.png` → `train_svr.png`
- 复制 `linear_regression.png` → `train_linear_regression.png`
- 复制 `ridge_regression.png` → `train_ridge_regression.png`
- 复制 `lasso_regression.png` → `train_lasso_regression.png`
- 复制 `k_nearest_neighbors_regressor.png` → `train_knn_regressor.png`
- 复制 `k_means.png` → `train_kmeans.png`
- 复制 `density_baised_spatial_clistering_of_applications_with_noise.png` → `train_dbscan.png`
- 复制 `agglomerative_clustering.png` → `train_agglomerative_clustering.png`

## 验证

运行 `verify_icons.py` 来验证所有必需文件是否存在。

## 已修正的问题

1. ✅ `IconLoader.cs` 中的资源名称已修正为 `SimpleML.icons.{iconName}`（小写icons）
2. ✅ `.csproj` 文件已配置图标资源嵌入
3. ✅ 所有组件代码已更新为使用 `IconLoader.LoadComponentIcon()`

## 下一步

完成图标文件设置后：
1. 使用 Visual Studio 或 MSBuild 构建项目
2. GHA文件将自动生成在 `bin\Release\SimpleML.gha`
3. 安装到Grasshopper并测试图标显示
