# 示例 1：鸢尾花分类（打开即照做）

目标：用最少步骤跑通「数据 → 训练 → 预测 → 评估」。  
练熟后，只换数据源即可用于自己的分类任务。

## 画布电池（从左到右）

1. `Health Check` / 环境体检 → Run=true → Status 应为 **PASS**
2. `Load Dataset` / 加载示例 → DN=`iris` → 用 **Dataset** 输出
3. `Split Dataset` / 分割 → Dataset
4. `Smart Train` / 智能训练 → Train Dataset，Task=`classification`（或 `auto`）
5. `Predict` / 预测 → Model + Test Dataset
6. `Evaluate` / 评估 → Model + Test Dataset → 阅读 **Verdict**
7. （可选）`Feature Importance` / 特征重要性

## 换数据源

| 场景 | 做法 |
|------|------|
| 换示例 | `Load Dataset` 的 DN 改成 `wine` / `breast_cancer` 等 |
| 用自己的 CSV | `Read CSV` → `Quick Dataset`(X, y) → 接到 `Split`，替代 `Load Dataset` |

**不必改** Smart Train / Predict / Evaluate 的默认参数。

## 预期

- 准确率通常很高（演示数据）
- Verdict 显示「很强 / Strong …」之类结论句

## 也可用

`Beginner Wizard`，Task=`classification`，按输出配方接线。
