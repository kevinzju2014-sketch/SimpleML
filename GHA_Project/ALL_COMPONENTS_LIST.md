# SimpleML 所有组件完整列表

## 组件统计

- **总计**: 42个组件
- **已创建**: 42个组件 ✅
- **完成度**: 100%

## 组件分类列表

### 1. Data Input (数据输入) - 4个 ✅

1. ✅ **ReadCSVComponent** - 读取CSV文件
2. ✅ **ReadExcelComponent** - 读取Excel文件
3. ✅ **ExtractFeaturesAndTargetComponent** - 提取特征和目标
4. ✅ **GetDataInfoComponent** - 获取数据信息

### 2. Data Analysis (数据分析) - 4个 ✅

1. ✅ **CalculateStatisticsComponent** - 计算统计信息
2. ✅ **CalculateCorrelationComponent** - 计算相关性
3. ✅ **GetDataSummaryComponent** - 获取数据摘要
4. ✅ **DescribeFeaturesComponent** - 描述特征

### 3. Data Preprocessing (数据预处理) - 4个 ✅

1. ✅ **PrepareDataComponent** - 准备数据
2. ✅ **SplitDataComponent** - 分割数据
3. ✅ **NormalizeDataComponent** - 标准化数据
4. ✅ **HandleMissingDataComponent** - 处理缺失值

### 4. Dataset Management (数据集管理) - 4个 ✅

1. ✅ **CreateDatasetComponent** - 创建数据集
2. ✅ **DeconstructDatasetComponent** - 解构数据集
3. ✅ **GetDatasetInfoComponent** - 获取数据集信息
4. ✅ **SplitDatasetComponent** - 分割数据集

### 5. Model Training - 通用 (模型训练通用) - 3个 ✅

1. ✅ **TrainClassifierComponent** - 训练分类器（通用）
2. ✅ **TrainRegressorComponent** - 训练回归器（通用）
3. ✅ **TrainClusterComponent** - 训练聚类模型（通用）

### 6. Model Training - 算法特定 (算法特定训练) - 15个 ✅

#### Classification (分类) - 6个
1. ✅ **TrainRandomForestClassifierComponent** - 随机森林分类器
2. ✅ **TrainSVMClassifierComponent** - SVM分类器
3. ✅ **TrainLogisticRegressionClassifierComponent** - 逻辑回归分类器
4. ✅ **TrainKNNClassifierComponent** - KNN分类器
5. ✅ **TrainDecisionTreeClassifierComponent** - 决策树分类器
6. ✅ **TrainNaiveBayesClassifierComponent** - 朴素贝叶斯分类器

#### Regression (回归) - 6个
1. ✅ **TrainRandomForestRegressorComponent** - 随机森林回归器
2. ✅ **TrainSVRComponent** - 支持向量回归器
3. ✅ **TrainLinearRegressionComponent** - 线性回归器
4. ✅ **TrainRidgeRegressionComponent** - 岭回归器
5. ✅ **TrainLassoRegressionComponent** - Lasso回归器
6. ✅ **TrainKNNRegressorComponent** - KNN回归器

#### Clustering (聚类) - 3个
1. ✅ **TrainKMeansComponent** - K-Means聚类
2. ✅ **TrainDBSCANComponent** - DBSCAN聚类
3. ✅ **TrainAgglomerativeClusteringComponent** - 层次聚类

### 7. Model Prediction (模型预测) - 2个 ✅

1. ✅ **PredictComponent** - 预测
2. ✅ **PredictProbaComponent** - 预测概率

### 8. Model Evaluation (模型评估) - 3个 ✅

1. ✅ **EvaluateClassificationComponent** - 评估分类模型
2. ✅ **EvaluateRegressionComponent** - 评估回归模型
3. ✅ **EvaluateClusteringComponent** - 评估聚类模型

### 9. Model Management (模型管理) - 5个 ✅

1. ✅ **SaveModelComponent** - 保存模型
2. ✅ **SaveModelWithManagerComponent** - 使用管理器保存模型
3. ✅ **LoadModelComponent** - 加载模型
4. ✅ **LoadModelForPredictionComponent** - 加载模型用于预测
5. ✅ **ListModelsComponent** - 列出模型文件

## 文件位置

所有组件文件位于：
```
GHA_Project/Components/
```

## 命名空间

组件按分类组织在不同的命名空间中：

- `SimpleML.Components.DataInput`
- `SimpleML.Components.DataAnalysis`
- `SimpleML.Components.DataPreprocessing`
- `SimpleML.Components.DatasetManagement`
- `SimpleML.Components.ModelTraining`
- `SimpleML.Components.ModelPrediction`
- `SimpleML.Components.ModelEvaluation`
- `SimpleML.Components.ModelManagement`

## 构建和安装

1. **构建项目**：
   - 在 Visual Studio 中：`生成` → `生成解决方案`
   - 或运行 `build_simple.bat`

2. **安装GHA文件**：
   - 复制 `bin\Release\SimpleML.gha` 到 `%APPDATA%\Grasshopper\Libraries\`

3. **安装Python代码**：
   - 复制 `myML` 文件夹到 `%APPDATA%\Grasshopper\UserObjects\SimpleML\myML`

4. **重启Grasshopper**

## 组件使用

所有组件都：
- 自动查找myML路径（环境变量、默认位置、相对路径）
- 提供清晰的错误消息
- 支持Dataset对象和单独的X/y输入
- 输出JSON格式的数据（便于在Grasshopper中使用）

## 注意事项

1. **每个组件都有唯一的GUID**
2. **所有组件都继承自GH_Component**
3. **使用PythonScriptExecutor执行Python代码**
4. **支持Dataset对象的序列化/反序列化**

---

**创建完成日期**: 2026年1月26日  
**版本**: 1.0.0  
**状态**: ✅ 全部完成
