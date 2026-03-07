# Exposure 分组配置总结

## ✅ 配置完成

所有15个算法组件都已配置为使用 Exposure 属性实现视觉分组。

## Exposure 值分布

### GH_Exposure.primary (分类算法 - 6个)
1. ✅ TrainRandomForestClassifierComponent
2. ✅ TrainSVMClassifierComponent
3. ✅ TrainKNNClassifierComponent
4. ✅ TrainLogisticRegressionClassifierComponent
5. ✅ TrainNaiveBayesClassifierComponent
6. ✅ TrainDecisionTreeClassifierComponent

### GH_Exposure.secondary (回归算法 - 6个)
1. ✅ TrainRandomForestRegressorComponent
2. ✅ TrainSVRComponent
3. ✅ TrainLinearRegressionComponent
4. ✅ TrainRidgeRegressionComponent
5. ✅ TrainLassoRegressionComponent
6. ✅ TrainKNNRegressorComponent

### GH_Exposure.tertiary (聚类算法 - 3个)
1. ✅ TrainKMeansComponent
2. ✅ TrainDBSCANComponent
3. ✅ TrainAgglomerativeClusteringComponent

## 实现细节

### SubCategory
所有算法组件都使用：`"SimpleML", "05 Algorithm"`

### Exposure 属性
每个组件都添加了：
```csharp
public override GH_Exposure Exposure => GH_Exposure.primary;   // 分类
public override GH_Exposure Exposure => GH_Exposure.secondary; // 回归
public override GH_Exposure Exposure => GH_Exposure.tertiary;  // 聚类
```

## 显示效果

在 Grasshopper Ribbon Panel 中：

```
┌─────────────────────────────────────────┐
│  05 Algorithm                            │
├─────────────────────────────────────────┤
│ [RF] [SVM] [KNN] [LR] [NB] [DT]         │ ← primary 组
│                                         │
│ ─────────────────────────────────────   │ ← 淡化竖线
│ [RFR] [SVR] [LR] [Ridge] [Lasso] [KNNR]│ ← secondary 组
│                                         │
│ ─────────────────────────────────────   │ ← 淡化竖线
│ [KM] [DBSCAN] [AC]                      │ ← tertiary 组
└─────────────────────────────────────────┘
```

## 工作原理

1. **同一个 SubCategory**: 所有组件都在 "05 Algorithm" 下
2. **Exposure 分组**: Grasshopper 根据 Exposure 值自动分组
3. **自动分隔**: 不同 Exposure 组之间自动显示淡化竖线
4. **字母排序**: 同一 Exposure 组内的组件按字母顺序排列

## 验证

- ✅ 所有15个算法组件都已设置 Exposure
- ✅ 所有组件都使用 "05 Algorithm" SubCategory
- ✅ 分类、回归、聚类算法分别使用 primary、secondary、tertiary

## 下一步

重新构建 GHA 文件后，算法组件将在 Ribbon Panel 中按 Exposure 值分组显示，不同组之间会有淡化的竖线分隔。
