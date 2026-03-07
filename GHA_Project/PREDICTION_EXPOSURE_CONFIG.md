# 06 Prediction 类别 Exposure 分组配置

## ✅ 已完成的更新

所有预测和评估组件都已配置为使用 Exposure 属性实现视觉分组。

## Exposure 值设置

### GH_Exposure.primary (预测组件 - 3个)
1. ✅ PredictClassifierComponent
2. ✅ PredictRegressorComponent
3. ✅ PredictClusterComponent

### GH_Exposure.secondary (评估组件 - 3个)
1. ✅ EvaluateClassificationComponent
2. ✅ EvaluateRegressionComponent
3. ✅ EvaluateClusteringComponent

## 显示效果

在 Grasshopper 的 Ribbon Panel 中，所有组件都在 **"06 Prediction"** 这一个 SubCategory 下，但会按照 Exposure 值分组显示：

```
┌─────────────────────────────────────┐
│  06 Prediction                       │
├─────────────────────────────────────┤
│ [Predict Classifier]                │
│ [Predict Regressor]                  │ ← primary 组（预测）
│ [Predict Cluster]                   │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线
│ [Evaluate Classification]            │
│ [Evaluate Regression]                │ ← secondary 组（评估）
│ [Evaluate Clustering]                │
└─────────────────────────────────────┘
```

## 代码实现

### 预测组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.primary;
```

### 评估组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.secondary;
```

## 配置总结

- **SubCategory**: 所有组件都使用 `"06 Prediction"`
- **Exposure 分组**: 
  - 预测组件 → `primary` (1)
  - 评估组件 → `secondary` (2)
- **视觉效果**: Grasshopper 会自动在两组之间绘制淡化竖线

## 完整配置

### 05 Algorithm 类别
- Classification (6个) → `GH_Exposure.primary`
- Regression (6个) → `GH_Exposure.secondary`
- Clustering (3个) → `GH_Exposure.tertiary`

### 06 Prediction 类别
- Prediction (3个) → `GH_Exposure.primary`
- Evaluation (3个) → `GH_Exposure.secondary`

## 下一步

重新构建 GHA 文件后，预测和评估组件将在 Ribbon Panel 中按 Exposure 值分组显示，两组之间会有淡化的竖线分隔。
