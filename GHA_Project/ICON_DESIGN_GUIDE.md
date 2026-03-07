# SimpleML 图标设计指南

本文档提供了SimpleML插件所有组件的图标设计清单和设计规范。

---

## 目录

1. [设计规范](#设计规范)
2. [图标清单](#图标清单)
3. [设计建议](#设计建议)
4. [文件命名规范](#文件命名规范)

---

## 设计规范

### 1. 技术规格

#### 尺寸要求
- **主要尺寸**: 24×24 像素（Grasshopper标准图标尺寸）
- **推荐尺寸**: 48×48 像素（用于高DPI显示）
- **格式**: PNG（支持透明背景）
- **颜色模式**: RGB
- **位深度**: 32位（包含Alpha通道）

#### 视觉规范
- **背景**: 透明背景
- **线条粗细**: 最小2像素（24×24尺寸下）
- **圆角**: 可选，建议2-3像素圆角
- **边距**: 图标内容距离边缘至少2像素
- **对比度**: 确保在浅色和深色背景下都清晰可见

### 2. 颜色规范

#### 分类颜色方案

根据机器学习任务类型，使用以下颜色主题：

| 任务类型 | 主色调 | 辅助色 | 说明 |
|------|--------|--------|------|
| **分类任务** | 黄色 (#FFC107) | 浅黄 (#FFE082) | 分类相关组件（分类器、分类预测、分类评估） |
| **回归任务** | 橙色 (#FF9800) | 浅橙 (#FFB74D) | 回归相关组件（回归器、回归预测、回归评估） |
| **聚类任务** | 绿色 (#4CAF50) | 浅绿 (#81C784) | 聚类相关组件（聚类器、聚类预测、聚类评估） |
| **其他组件** | 灰色 (#757575) | 浅灰 (#BDBDBD) | 数据输入、数据分析、数据集管理、模型管理等非任务特定组件 |

#### 颜色使用原则
- **分类任务**: 使用黄色主题，包括所有分类相关的训练、预测、评估组件
- **回归任务**: 使用橙色主题，包括所有回归相关的训练、预测、评估组件
- **聚类任务**: 使用绿色主题，包括所有聚类相关的训练、预测、评估组件
- **其他组件**: 使用灰色或黑白，不使用彩色，包括：
  - 数据输入组件（Read CSV/Excel、Write CSV/Excel、Load Dataset）
  - 数据分析组件（Calculate Statistics、Calculate Correlation等）
  - 数据集管理组件（Create Dataset、Deconstruct Dataset、Split Dataset）
  - 模型管理组件（Save Model、Load Model）
- **中性色**: 黑色 (#000000) 或深灰 (#333333) 用于线条和文字
- **强调色**: 红色 (#FF4D4F) 仅用于警告或重要操作（如删除、错误）

### 3. 设计风格

#### 风格统一性
- **线条风格**: 统一使用线条图标（Line Icon）或填充图标（Filled Icon）
- **视觉重量**: 保持所有图标的视觉重量一致
- **细节层次**: 避免过于复杂的细节，确保在小尺寸下清晰可辨

#### 图标类型建议
- **数据输入**: 文件、文件夹、箭头向下、数据库
- **数据分析**: 图表、统计、放大镜、网格
- **数据集**: 数据表、连接、齿轮、处理
- **模型**: 神经网络、齿轮、箭头、评估

---

## 图标清单

### 01 Data Input（数据输入组件）- 灰色/黑白主题

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 优先级 |
|------|----------|----------|--------------|--------|
| 1 | Read CSV | ReadCSV | 文档图标 + CSV文字/表格 | 高 |
| 2 | Read Excel | ReadExcel | Excel图标/表格 + XLSX标识 | 高 |
| 3 | Write CSV | WriteCSV | 文档图标 + 箭头向外 + CSV | 高 |
| 4 | Write Excel | WriteExcel | Excel图标 + 箭头向外 | 高 |
| 5 | Load Dataset | LoadDataset | 数据库图标 + 下载箭头 | 中 |

**设计提示**:
- 使用灰色主题 (#757575) 或黑白配色，不使用彩色
- 文件相关图标使用文档/表格元素
- 写入操作添加向外箭头
- 数据集加载使用数据库或云图标
- 保持简洁的单色或灰度设计

---

### 02 Data Analysis（数据分析组件）- 灰色/黑白主题

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 优先级 |
|------|----------|----------|--------------|--------|
| 6 | Calculate Statistics | CalculateStatistics | 统计图表（柱状图/折线图） | 高 |
| 7 | Calculate Correlation | CalculateCorrelation | 相关性矩阵/网格 | 高 |
| 8 | Get Data Summary | GetDataSummary | 信息卡片/摘要文档 | 中 |
| 9 | Describe Features | DescribeFeatures | 特征列表/属性卡片 | 中 |

**设计提示**:
- 使用灰色主题 (#757575) 或黑白配色，不使用彩色
- 统计相关使用图表元素
- 相关性使用网格或矩阵
- 摘要使用文档或卡片图标
- 保持简洁的单色或灰度设计

---

### 03 Dataset（数据集相关组件）- 灰色/黑白主题

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 优先级 |
|------|----------|----------|--------------|--------|
| 10 | Create Dataset | CreateDataset | 数据表 + 加号/创建 | 高 |
| 11 | Deconstruct Dataset | DeconstructDataset | 数据表 + 拆分箭头 | 中 |
| 12 | Split Dataset | SplitDataset | 数据表 + 分割线/剪刀 | 高 |

**设计提示**:
- 使用灰色主题 (#757575) 或黑白配色，不使用彩色
- 数据表作为主要元素
- 操作使用箭头、加号、分割线等符号
- 保持数据集操作的视觉一致性
- 保持简洁的单色或灰度设计

---

### 04 Model（模型相关组件）- 按任务类型使用颜色

#### 4.1 通用训练组件（3个）

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 颜色主题 | 优先级 |
|------|----------|----------|--------------|----------|--------|
| 13 | Train Classifier | TrainClassifier | 分类图标（类别标签/分类器） | **黄色** (#FFC107) | 高 |
| 14 | Train Regressor | TrainRegressor | 回归图标（趋势线/预测） | **橙色** (#FF9800) | 高 |
| 15 | Train Cluster | TrainCluster | 聚类图标（分组/聚类点） | **绿色** (#4CAF50) | 高 |

**设计提示**:
- **分类组件**: 使用黄色主题 (#FFC107)，包括标签、类别、分类器图标
- **回归组件**: 使用橙色主题 (#FF9800)，包括趋势线、预测箭头、函数曲线
- **聚类组件**: 使用绿色主题 (#4CAF50)，包括分组、聚类点、簇图标

#### 4.2 算法特定训练组件（15个）

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 颜色主题 | 优先级 |
|------|----------|----------|--------------|----------|--------|
| 16 | Train Random Forest Classifier | TrainRandomForestClassifier | 树形图标（随机森林） | **黄色** (#FFC107) | 中 |
| 17 | Train Random Forest Regressor | TrainRandomForestRegressor | 树形图标（回归版本） | **橙色** (#FF9800) | 中 |
| 18 | Train SVM Classifier | TrainSVMClassifier | 支持向量机图标（分隔线） | **黄色** (#FFC107) | 中 |
| 19 | Train SVR | TrainSVR | 支持向量回归图标 | **橙色** (#FF9800) | 中 |
| 20 | Train Linear Regression | TrainLinearRegression | 线性回归图标（直线） | **橙色** (#FF9800) | 中 |
| 21 | Train Ridge Regression | TrainRidgeRegression | 岭回归图标（带正则化） | **橙色** (#FF9800) | 低 |
| 22 | Train Lasso Regression | TrainLassoRegression | Lasso回归图标（带正则化） | **橙色** (#FF9800) | 低 |
| 23 | Train KNN Classifier | TrainKNNClassifier | KNN图标（最近邻点） | **黄色** (#FFC107) | 中 |
| 24 | Train KNN Regressor | TrainKNNRegressor | KNN回归图标 | **橙色** (#FF9800) | 中 |
| 25 | Train Logistic Regression Classifier | TrainLogisticRegressionClassifier | 逻辑回归图标（S曲线） | **黄色** (#FFC107) | 中 |
| 26 | Train Naive Bayes Classifier | TrainNaiveBayesClassifier | 贝叶斯图标（概率/条件） | **黄色** (#FFC107) | 低 |
| 27 | Train Decision Tree Classifier | TrainDecisionTreeClassifier | 决策树图标 | **黄色** (#FFC107) | 中 |
| 28 | Train K-Means | TrainKMeans | K-Means图标（K个中心点） | **绿色** (#4CAF50) | 中 |
| 29 | Train DBSCAN | TrainDBSCAN | DBSCAN图标（密度聚类） | **绿色** (#4CAF50) | 低 |
| 30 | Train Agglomerative Clustering | TrainAgglomerativeClustering | 层次聚类图标（树状图） | **绿色** (#4CAF50) | 低 |

**设计提示**:
- **分类算法**: 使用黄色主题 (#FFC107) - Random Forest Classifier, SVM Classifier, KNN Classifier, Logistic Regression, Naive Bayes, Decision Tree
- **回归算法**: 使用橙色主题 (#FF9800) - Random Forest Regressor, SVR, Linear Regression, Ridge Regression, Lasso Regression, KNN Regressor
- **聚类算法**: 使用绿色主题 (#4CAF50) - K-Means, DBSCAN, Agglomerative Clustering
- 可以添加算法特有的视觉元素（如树、线、点、曲线等）
- 分类和回归版本可以使用相同基础图标，通过颜色和细节区分

#### 4.3 模型预测组件（3个）

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 颜色主题 | 优先级 |
|------|----------|----------|--------------|----------|--------|
| 31 | Predict Classifier | PredictClassifier | 预测箭头 + 分类标签 | **黄色** (#FFC107) | 高 |
| 32 | Predict Regressor | PredictRegressor | 预测箭头 + 数值/趋势 | **橙色** (#FF9800) | 高 |
| 33 | Predict Cluster | PredictCluster | 预测箭头 + 聚类标签 | **绿色** (#4CAF50) | 高 |

**设计提示**:
- **分类预测**: 使用黄色主题 (#FFC107)
- **回归预测**: 使用橙色主题 (#FF9800)
- **聚类预测**: 使用绿色主题 (#4CAF50)
- 统一使用预测箭头作为主要元素
- 通过颜色和辅助元素区分分类、回归、聚类
- 箭头方向：从左到右或从上到下

#### 4.4 模型评估组件（3个）

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 颜色主题 | 优先级 |
|------|----------|----------|--------------|----------|--------|
| 34 | Evaluate Classification | EvaluateClassification | 评估图标（对勾/评分/混淆矩阵） | **黄色** (#FFC107) | 高 |
| 35 | Evaluate Regression | EvaluateRegression | 评估图标（评分/误差） | **橙色** (#FF9800) | 高 |
| 36 | Evaluate Clustering | EvaluateClustering | 评估图标（评分/聚类质量） | **绿色** (#4CAF50) | 高 |

**设计提示**:
- **分类评估**: 使用黄色主题 (#FFC107)
- **回归评估**: 使用橙色主题 (#FF9800)
- **聚类评估**: 使用绿色主题 (#4CAF50)
- 统一使用评估相关图标（对勾、评分、图表）
- 通过颜色和细节区分不同类型的评估
- 强调准确性和性能评估

#### 4.5 模型管理组件（2个）

| 序号 | 组件名称 | 英文名称 | 建议图标元素 | 颜色主题 | 优先级 |
|------|----------|----------|--------------|----------|--------|
| 37 | Save Model | SaveModel | 保存图标（磁盘/下载） | **灰色** (#757575) | 高 |
| 38 | Load Model | LoadModel | 加载图标（上传/打开） | **灰色** (#757575) | 高 |

**设计提示**:
- 使用灰色主题 (#757575) 或黑白配色，不使用彩色
- 保存：使用磁盘、下载、保存图标
- 加载：使用上传、打开、文件夹图标
- 可以添加模型相关的小图标（如齿轮、神经网络）
- 保持简洁的单色或灰度设计

---

## 设计建议

### 1. 图标设计流程

1. **概念设计**
   - 理解组件功能
   - 选择核心视觉元素
   - 确定图标风格

2. **草图阶段**
   - 绘制多个方案
   - 测试不同布局
   - 确保小尺寸可读性

3. **数字化**
   - 使用矢量工具（Illustrator、Figma、Inkscape）
   - 保持线条清晰
   - 确保对齐和比例

4. **优化**
   - 测试不同尺寸
   - 检查对比度
   - 验证透明背景

5. **导出**
   - 导出为PNG格式
   - 确保Alpha通道
   - 按命名规范保存

### 2. 设计工具推荐

- **矢量设计**: Adobe Illustrator, Figma, Inkscape
- **图标库参考**: 
  - Material Icons
  - Font Awesome
  - Feather Icons
  - Fluent UI Icons
- **颜色工具**: 
  - Adobe Color
  - Coolors.co
  - Material Design Color Tool

### 3. 设计检查清单

- [ ] 尺寸：24×24像素（主要），48×48像素（高DPI）
- [ ] 格式：PNG，32位，透明背景
- [ ] 颜色：符合分类颜色规范
- [ ] 清晰度：小尺寸下清晰可辨
- [ ] 对比度：浅色和深色背景下都可见
- [ ] 一致性：风格统一，视觉重量一致
- [ ] 边距：内容距离边缘至少2像素
- [ ] 命名：符合文件命名规范

### 4. 设计参考

#### 数据输入类图标参考
- 文件图标：文档轮廓、表格、数据库
- 操作图标：箭头（向下=输入，向外=输出）

#### 数据分析类图标参考
- 图表：柱状图、折线图、饼图
- 统计：网格、矩阵、信息卡片

#### 数据集类图标参考
- 数据表：表格、数据库、连接
- 操作：加号、拆分、分割

#### 模型类图标参考
- 训练：齿轮、神经网络、处理
- 预测：箭头、趋势、标签
- 评估：对勾、评分、图表
- 管理：保存、加载、文件夹

---

## 文件命名规范

### 命名格式
```
{ComponentName}Icon.png
```

### 示例
- `ReadCSVIcon.png`
- `TrainClassifierIcon.png`
- `EvaluateClassificationIcon.png`

### 文件组织
建议将图标文件组织在以下目录结构中：
```
GHA_Project/
├── Icons/
│   ├── DataInput/
│   │   ├── ReadCSVIcon.png
│   │   ├── ReadExcelIcon.png
│   │   ├── WriteCSVIcon.png
│   │   ├── WriteExcelIcon.png
│   │   └── LoadDatasetIcon.png
│   ├── DataAnalysis/
│   │   ├── CalculateStatisticsIcon.png
│   │   ├── CalculateCorrelationIcon.png
│   │   ├── GetDataSummaryIcon.png
│   │   └── DescribeFeaturesIcon.png
│   ├── Dataset/
│   │   ├── CreateDatasetIcon.png
│   │   ├── DeconstructDatasetIcon.png
│   │   └── SplitDatasetIcon.png
│   └── Model/
│       ├── TrainClassifierIcon.png
│       ├── TrainRegressorIcon.png
│       ├── TrainClusterIcon.png
│       ├── TrainRandomForestClassifierIcon.png
│       ├── ... (其他算法图标)
│       ├── PredictClassifierIcon.png
│       ├── PredictRegressorIcon.png
│       ├── PredictClusterIcon.png
│       ├── EvaluateClassificationIcon.png
│       ├── EvaluateRegressionIcon.png
│       ├── EvaluateClusteringIcon.png
│       ├── SaveModelIcon.png
│       └── LoadModelIcon.png
```

---

## 优先级说明

### 高优先级（必须设计）
- 数据输入组件（5个）
- 数据分析组件（4个）
- 数据集组件（3个）
- 通用训练组件（3个）
- 预测组件（3个）
- 评估组件（3个）
- 模型管理组件（2个）

**总计：23个高优先级图标**

### 中优先级（建议设计）
- 常用算法特定训练组件（8个）
- 数据摘要组件（2个）

**总计：10个中优先级图标**

### 低优先级（可选设计）
- 不常用算法特定训练组件（5个）

**总计：5个低优先级图标**

---

## 快速设计模板

### 基础模板结构
```
[图标内容区域]
├── 主元素（核心功能图标）
├── 辅助元素（操作/类型标识）
└── 装饰元素（可选）
```

### 分类标识建议
- **分类任务**: 黄色主题 (#FFC107) - 所有分类相关的训练、预测、评估组件
- **回归任务**: 橙色主题 (#FF9800) - 所有回归相关的训练、预测、评估组件
- **聚类任务**: 绿色主题 (#4CAF50) - 所有聚类相关的训练、预测、评估组件
- **其他组件**: 灰色主题 (#757575) 或黑白 - 数据输入、数据分析、数据集管理、模型管理等非任务特定组件

---

## 总结

- **总组件数**: 38个
- **必须设计**: 23个（高优先级）
- **建议设计**: 10个（中优先级）
- **可选设计**: 5个（低优先级）

**建议优先完成高优先级图标，确保核心功能组件都有清晰的视觉标识。**

---

---

## 颜色方案总结

### 彩色组件（按任务类型）

#### 黄色主题 - 分类任务 (#FFC107)
- Train Classifier
- Train Random Forest Classifier
- Train SVM Classifier
- Train KNN Classifier
- Train Logistic Regression Classifier
- Train Naive Bayes Classifier
- Train Decision Tree Classifier
- Predict Classifier
- Evaluate Classification

#### 橙色主题 - 回归任务 (#FF9800)
- Train Regressor
- Train Random Forest Regressor
- Train SVR
- Train Linear Regression
- Train Ridge Regression
- Train Lasso Regression
- Train KNN Regressor
- Predict Regressor
- Evaluate Regression

#### 绿色主题 - 聚类任务 (#4CAF50)
- Train Cluster
- Train K-Means
- Train DBSCAN
- Train Agglomerative Clustering
- Predict Cluster
- Evaluate Clustering

### 灰色/黑白组件（非任务特定）

#### 数据输入组件
- Read CSV
- Read Excel
- Write CSV
- Write Excel
- Load Dataset

#### 数据分析组件
- Calculate Statistics
- Calculate Correlation
- Get Data Summary
- Describe Features

#### 数据集管理组件
- Create Dataset
- Deconstruct Dataset
- Split Dataset

#### 模型管理组件
- Save Model
- Load Model

---

*本文档最后更新日期：2026-01-27*
