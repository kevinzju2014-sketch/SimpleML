# Exposure 分组实现总结

## ✅ 已完成的更新

所有算法组件现在都使用 **Exposure 属性**来实现 Ribbon Panel 中的视觉分组效果。

## 实现原理

Grasshopper 会根据同一个 SubCategory 下组件的 `Exposure` 值进行分组，并在不同 Exposure 组之间自动绘制淡化的竖线分隔。

### Exposure 值设置

- **分类算法** (6个): `GH_Exposure.primary` (1)
- **回归算法** (6个): `GH_Exposure.secondary` (2)
- **聚类算法** (3个): `GH_Exposure.tertiary` (4)

## 组件列表

### Classification (GH_Exposure.primary)
1. TrainRandomForestClassifierComponent
2. TrainSVMClassifierComponent
3. TrainKNNClassifierComponent
4. TrainLogisticRegressionClassifierComponent
5. TrainNaiveBayesClassifierComponent
6. TrainDecisionTreeClassifierComponent

### Regression (GH_Exposure.secondary)
1. TrainRandomForestRegressorComponent
2. TrainSVRComponent
3. TrainLinearRegressionComponent
4. TrainRidgeRegressionComponent
5. TrainLassoRegressionComponent
6. TrainKNNRegressorComponent

### Clustering (GH_Exposure.tertiary)
1. TrainKMeansComponent
2. TrainDBSCANComponent
3. TrainAgglomerativeClusteringComponent

## 显示效果

在 Grasshopper 的 Ribbon Panel 中，所有算法组件都在 **"05 Algorithm"** 这一个 SubCategory 下，但会按照 Exposure 值分组显示：

```
┌─────────────────────────────────────┐
│  05 Algorithm                       │
├─────────────────────────────────────┤
│ [分类组件] [分类组件] [分类组件]    │
│ [分类组件] [分类组件] [分类组件]    │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线（primary → secondary）
│ [回归组件] [回归组件] [回归组件]    │
│ [回归组件] [回归组件] [回归组件]    │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线（secondary → tertiary）
│ [聚类组件] [聚类组件] [聚类组件]    │
└─────────────────────────────────────┘
```

## 代码实现

每个算法组件都添加了 Exposure 属性覆盖：

```csharp
public override GH_Exposure Exposure => GH_Exposure.primary;   // 分类算法
public override GH_Exposure Exposure => GH_Exposure.secondary; // 回归算法
public override GH_Exposure Exposure => GH_Exposure.tertiary;  // 聚类算法
```

## 配置总结

- **SubCategory**: 所有算法组件都使用 `"05 Algorithm"`
- **Exposure 分组**: 
  - Classification → `primary` (1)
  - Regression → `secondary` (2)
  - Clustering → `tertiary` (4)
- **视觉效果**: Grasshopper 会自动在不同 Exposure 组之间绘制淡化竖线

## 优势

1. ✅ 所有组件都在同一个 SubCategory 下
2. ✅ 通过 Exposure 值实现视觉分组
3. ✅ 自动显示淡化竖线分隔符
4. ✅ 符合 Grasshopper 的标准实现方式

## 下一步

重新构建 GHA 文件后，算法组件将在 Ribbon Panel 中按 Exposure 值分组显示，不同组之间会有淡化的竖线分隔。
