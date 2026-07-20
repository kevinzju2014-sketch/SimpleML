# 示例 3：回归（糖尿病示例）

目标：跑通回归，并用 Verdict 读懂 R² 好坏。

## 画布电池

1. `Health Check` → PASS
2. `Load Dataset` → DN=`diabetes` → Dataset
3. `Split Dataset`
4. `Smart Train` → Task=`regression`（或 `auto`）
5. `Predict` + `Evaluate` → 看 R² / Verdict
6. （可选）`Visualize Regression`

## 换数据源

| 场景 | 做法 |
|------|------|
| 换示例 | DN 改为其他回归数据集 |
| 用自己的 CSV | `Read CSV` → `Quick Dataset`(X=特征, y=连续目标) → 接 `Split` |

其余训练/评估电池保持默认即可。

## 进阶文件

`gh/03b_regression_advanced.gh`：同流程，带更多展示与注释。
