# 01 Input 类别 Exposure 分组配置

## ✅ 已完成的更新

所有数据输入组件都已配置为使用 Exposure 属性实现视觉分组。

## Exposure 值设置

### GH_Exposure.primary (文件IO组件 - 4个)
1. ✅ ReadCSVComponent
2. ✅ ReadExcelComponent
3. ✅ WriteCSVComponent
4. ✅ WriteExcelComponent

### GH_Exposure.secondary (数据集加载组件 - 1个)
1. ✅ LoadDatasetComponent

## 显示效果

在 Grasshopper 的 Ribbon Panel 中，所有组件都在 **"01 Input"** 这一个 SubCategory 下，但会按照 Exposure 值分组显示：

```
┌─────────────────────────────────────┐
│  01 Input                            │
├─────────────────────────────────────┤
│ [Read CSV]                           │
│ [Read Excel]                         │ ← primary 组（文件IO）
│ [Write CSV]                          │
│ [Write Excel]                        │
│                                     │
│ ────────────────────────────────   │ ← 淡化竖线
│ [Load Dataset]                       │ ← secondary 组（数据集加载）
└─────────────────────────────────────┘
```

## 代码实现

### 文件IO组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.primary;
```

### 数据集加载组件
```csharp
public override GH_Exposure Exposure => GH_Exposure.secondary;
```

## 完整配置总结

### 01 Input 类别
- File IO (4个) → `GH_Exposure.primary`
- Dataset Loading (1个) → `GH_Exposure.secondary`

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

- **SubCategory**: 所有组件都使用 `"01 Input"`
- **Exposure 分组**: 
  - 文件IO组件 → `primary` (1)
  - 数据集加载组件 → `secondary` (2)
- **视觉效果**: Grasshopper 会自动在两组之间绘制淡化竖线

## 下一步

重新构建 GHA 文件后，文件IO和数据集加载组件将在 Ribbon Panel 中按 Exposure 值分组显示，两组之间会有淡化的竖线分隔。
