# Grasshopper Ribbon Panel 分组说明

## 视觉分组实现方式

在Grasshopper中，要在Ribbon Panel的同一个SubCategory下创建视觉分组（看起来像被淡化的竖线分隔），需要使用**子类别（SubCategory）**。

## 当前配置

所有算法组件都使用子类别格式：`05 Algorithm|子类别名称`

### 子类别结构

- **05 Algorithm|Classification** - 分类算法（6个组件）
- **05 Algorithm|Regression** - 回归算法（6个组件）
- **05 Algorithm|Clustering** - 聚类算法（3个组件）

## 显示效果

在Grasshopper的Ribbon Panel中，这些子类别会在 "05 Algorithm" 主类别下显示为**视觉分组**，每个子类别之间会有淡化的竖线分隔，看起来就像这样：

```
┌─────────────────────────────────────┐
│  05 Algorithm                       │
├─────────────────────────────────────┤
│ Classification                      │
│ [组件] [组件] [组件] [组件] [组件] │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线
│ Regression                          │
│ [组件] [组件] [组件] [组件] [组件] │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线
│ Clustering                          │
│ [组件] [组件] [组件]                │
└─────────────────────────────────────┘
```

## 组件列表

### Classification (6个)
1. TrainRandomForestClassifierComponent
2. TrainSVMClassifierComponent
3. TrainKNNClassifierComponent
4. TrainLogisticRegressionClassifierComponent
5. TrainNaiveBayesClassifierComponent
6. TrainDecisionTreeClassifierComponent

### Regression (6个)
1. TrainRandomForestRegressorComponent
2. TrainSVRComponent
3. TrainLinearRegressionComponent
4. TrainRidgeRegressionComponent
5. TrainLassoRegressionComponent
6. TrainKNNRegressorComponent

### Clustering (3个)
1. TrainKMeansComponent
2. TrainDBSCANComponent
3. TrainAgglomerativeClusteringComponent

## 说明

- 所有组件都在 **"05 Algorithm"** 这个主SubCategory下
- 通过子类别（使用 `|` 分隔符）创建视觉分组
- Grasshopper会自动在子类别之间显示淡化的竖线分隔符
- 这是Grasshopper中实现Ribbon Panel视觉分组的标准方法
