# 批量修复组件指南

## 当前状态

### 已修复的组件（9个）
1. ✅ CalculateStatisticsComponent - pandas导入 + Readme
2. ✅ GetDataInfoComponent - pandas导入 + Readme
3. ✅ ReadCSVComponent - Readme
4. ✅ ReadExcelComponent - Readme
5. ✅ CalculateCorrelationComponent - pandas导入 + Readme
6. ✅ DeconstructDatasetComponent - numpy导入 + Readme
7. ✅ ExtractFeaturesAndTargetComponent - 之前已修复pandas导入
8. ✅ PrepareDataComponent - 之前已修复Tree结构
9. ✅ SplitDataComponent - 之前已修复Tree结构

### 待修复的组件（36个）

## 修复步骤

### 步骤1: 修复Python导入顺序

查找模式：
```csharp
string pythonCode = $@"
import sys
import os
import json
import pandas as pd  // 或 import numpy as np
sys.path.insert(0, r'{mymlPath}')
```

替换为模板代码（见 `COMPONENT_FIX_TEMPLATE.md`）

### 步骤2: 添加Readme输出

1. 在 `RegisterOutputParams` 中添加：
```csharp
pManager.AddTextParameter("Readme", "R", "组件使用说明和注意事项", GH_ParamAccess.item);
```

2. 在 `SolveInstance` 的最后添加：
```csharp
// Readme输出
string readme = @"组件名称: [名称]
功能: [功能描述]
...";
DA.SetData([最后一个输出索引+1], readme);
```

### 步骤3: 检查数据结构

- 数据输出（X, y, Data等）→ 使用Tree结构
- 列表输出 → 使用List结构（如果可能）
- 元数据（Info, Metrics等）→ 保持Text/JSON

## 批量修复建议

由于组件数量较多，建议：
1. 按分类逐个修复（Data Input → Data Analysis → ...）
2. 使用查找替换功能批量修复导入顺序
3. Readme内容需要根据每个组件的具体功能编写

## 优先级

**高优先级（常用组件）：**
- GetDataSummaryComponent
- DescribeFeaturesComponent
- CreateDatasetComponent
- SplitDatasetComponent
- GetDatasetInfoComponent

**中优先级（训练组件）：**
- 所有Train*Component（18个）

**低优先级：**
- Evaluate*Component（3个）
- Model Management组件（5个）
- 其他组件（10个）
