# 04 Model 类别 Exposure 分组配置

## ✅ 已完成的更新

所有模型训练和模型IO组件都已配置为使用 Exposure 属性实现视觉分组。

## Exposure 值设置

### GH_Exposure.primary (模型训练组件 - 3个)
1. ✅ TrainClassifierComponent
2. ✅ TrainRegressorComponent
3. ✅ TrainClusterComponent

### GH_Exposure.secondary (模型IO组件 - 2个)
1. ✅ SaveModelComponent
2. ✅ LoadModelComponent

## 显示效果

在 Grasshopper 的 Ribbon Panel 中，所有组件都在 **"04 Model"** 这一个 SubCategory 下，但会按照 Exposure 值分组显示：

```
┌─────────────────────────────────────┐
│  04 Model                            │
├─────────────────────────────────────┤
│ [Train Classifier]                  │
│ [Train Regressor]                    │ ← primary 组（模型训练）
│ [Train Cluster]                     │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线
│ [Save Model]                        │
│ [Load Model]                         │ ← secondary 组（模型IO）
└─────────────────────────────────────┘
```

## 代码实现

### 模型训练组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.primary;
```

### 模型IO组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.secondary;
```

## 完整配置总结

### 04 Model 类别
- Model Training (3个) → `GH_Exposure.primary`
- Model IO (2个) → `GH_Exposure.secondary`

### 05 Algorithm 类别
- Classification (6个) → `GH_Exposure.primary`
- Regression (6个) → `GH_Exposure.secondary`
- Clustering (3个) → `GH_Exposure.tertiary`

### 06 Prediction 类别
- Prediction (3个) → `GH_Exposure.primary`
- Evaluation (3个) → `GH_Exposure.secondary`

## 配置总结

- **SubCategory**: 所有组件都使用 `"04 Model"`
- **Exposure 分组**: 
  - 模型训练组件 → `primary` (1)
  - 模型IO组件 → `secondary` (2)
- **视觉效果**: Grasshopper 会自动在两组之间绘制淡化竖线

## 下一步

重新构建 GHA 文件后，模型训练和模型IO组件将在 Ribbon Panel 中按 Exposure 值分组显示，两组之间会有淡化的竖线分隔。
