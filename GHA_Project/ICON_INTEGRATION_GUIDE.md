# 图标集成指南

本文档说明如何将图标集成到SimpleML GHA插件中。

## 已完成的工作

### 1. 图标文件
- ✅ 所有24个图标PNG文件已生成在 `icons/` 目录
- ✅ 图标规格：24x24像素，透明背景，黑色文字，2-3个大写字母缩写

### 2. 代码文件
- ✅ `IconLoader.cs` - 图标加载辅助类
- ✅ `ComponentIconMap.cs` - 组件名称到图标文件名的映射
- ✅ 所有组件已更新，使用 `IconLoader.LoadComponentIcon()` 加载图标

### 3. 项目配置
- ✅ `SimpleML.csproj` 已更新，包含图标资源：
  ```xml
  <ItemGroup>
    <EmbeddedResource Include="Icons\*.png" />
  </ItemGroup>
  ```

## 下一步操作

### 1. 复制图标文件到GHA_Project/Icons目录

**重要**：需要手动将图标文件复制到 `GHA_Project/Icons/` 目录，或者运行以下Python脚本：

```python
import os
import shutil

src = r'D:\Helio\250928_机器学习课程\myML\icons'
dst = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\Icons'

os.makedirs(dst, exist_ok=True)
for f in os.listdir(src):
    if f.endswith('.png'):
        shutil.copy2(os.path.join(src, f), os.path.join(dst, f))
        print(f'已复制: {f}')
```

### 2. 确保所有组件文件包含必要的using语句

所有组件文件需要包含：
```csharp
using SimpleML.Core;
```

如果缺少，需要手动添加。

### 3. 构建GHA文件

使用以下方法之一构建：

#### 方法1：使用Visual Studio
1. 打开 `SimpleML.csproj`
2. 选择 Release 配置
3. 构建项目（Build -> Build Solution）
4. GHA文件将自动生成在 `bin/Release/SimpleML.gha`

#### 方法2：使用MSBuild命令行
```batch
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
msbuild SimpleML.csproj /p:Configuration=Release
```

#### 方法3：使用项目中的构建脚本
```batch
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
build.bat
```

## 图标映射表

| 组件类名 | 图标文件名 |
|---------|-----------|
| ReadCSVComponent | file_io_components.png |
| ReadExcelComponent | file_io_components.png |
| WriteCSVComponent | file_io_components.png |
| WriteExcelComponent | file_io_components.png |
| LoadDatasetComponent | dataset_loader.png |
| CalculateStatisticsComponent | statistics_components.png |
| CalculateCorrelationComponent | statistics_components.png |
| GetDataSummaryComponent | statistics_components.png |
| DescribeFeaturesComponent | statistics_components.png |
| CreateDatasetComponent | dataset_components.png |
| DeconstructDatasetComponent | dataset_components.png |
| SplitDatasetComponent | dataset_components.png |
| TrainClassifierComponent | train_components.png |
| TrainRegressorComponent | train_components.png |
| TrainClusterComponent | train_components.png |
| TrainRandomForestClassifierComponent | train_random_forest_classifier.png |
| TrainSVMClassifierComponent | train_svm_classifier.png |
| TrainKNNClassifierComponent | train_knn_classifier.png |
| TrainLogisticRegressionClassifierComponent | train_logistic_regression_classifier.png |
| TrainNaiveBayesClassifierComponent | train_naive_bayes_classifier.png |
| TrainDecisionTreeClassifierComponent | train_decision_tree_classifier.png |
| TrainRandomForestRegressorComponent | train_random_forest_regressor.png |
| TrainSVRComponent | train_svr.png |
| TrainLinearRegressionComponent | train_linear_regression.png |
| TrainRidgeRegressionComponent | train_ridge_regression.png |
| TrainLassoRegressionComponent | train_lasso_regression.png |
| TrainKNNRegressorComponent | train_knn_regressor.png |
| TrainKMeansComponent | train_kmeans.png |
| TrainDBSCANComponent | train_dbscan.png |
| TrainAgglomerativeClusteringComponent | train_agglomerative_clustering.png |
| PredictClassifierComponent | predict_components.png |
| PredictRegressorComponent | predict_components.png |
| PredictClusterComponent | predict_components.png |
| EvaluateClassificationComponent | evaluate_components.png |
| EvaluateRegressionComponent | evaluate_components.png |
| EvaluateClusteringComponent | evaluate_components.png |
| SaveModelComponent | model_io_components.png |
| LoadModelComponent | model_io_components.png |

## 验证图标加载

构建完成后，在Grasshopper中加载GHA文件，检查组件是否显示图标。如果图标未显示，检查：

1. 图标文件是否在 `GHA_Project/Icons/` 目录中
2. `.csproj` 文件是否正确包含 `<EmbeddedResource Include="Icons\*.png" />`
3. 组件代码是否正确使用 `IconLoader.LoadComponentIcon()`
4. 构建输出中是否有错误信息

## 故障排除

### 图标未显示
- 检查图标文件是否存在
- 检查资源是否正确嵌入到DLL中
- 检查组件代码中的图标加载逻辑

### 构建错误
- 确保所有组件文件都包含 `using SimpleML.Core;`
- 检查 `IconLoader.cs` 和 `ComponentIconMap.cs` 是否正确编译

### 资源加载失败
- 检查资源名称是否正确（应为 `SimpleML.Icons.{filename}`）
- 检查图标文件是否作为嵌入资源包含在项目中

## 完成状态

- [x] 图标文件生成
- [x] 图标加载类创建
- [x] 组件映射创建
- [x] 项目配置更新
- [x] 组件代码更新
- [ ] 图标文件复制到GHA_Project/Icons（需要手动完成）
- [ ] 构建GHA文件（需要手动完成）
- [ ] 测试图标显示（需要手动完成）
