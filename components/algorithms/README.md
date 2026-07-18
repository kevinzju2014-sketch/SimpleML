# 算法参数电池（Python）

每个函数只配置算法参数（JSON），再接到通用 `train_classifier` / `train_regressor` / `train_cluster`。

```python
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[2]))

from components.algorithms.train_logistic_regression_classifier import (
    train_logistic_regression_classifier,
)
from components.train_components import train_classifier

params, _ = train_logistic_regression_classifier()
model, readme, info = train_classifier(dataset, params)
```

开发根目录：`D:\Helio\250928_机器学习课程\`（与 GitHub 同步）。
