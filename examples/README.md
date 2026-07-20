# SimpleML Teaching Files（可视化教学文件）

打开下面任意 `.gh`，**不用从零搭线**也能学会主流程。  
学会后，通常**只改数据来源**就能换成自己的数据，不必改一堆参数。

## 文件一览

| 文件 | 任务 | 你会学到 | 换数据时改哪里 |
|------|------|----------|----------------|
| `gh/01_classification_iris.gh` | 分类 | 体检 → 加载 → 分割 → 智能训练 → 预测 → 评估 | `Load Dataset` 名字，或换成 `Read CSV` + `Quick Dataset` |
| `gh/02_clustering_color.gh` | 聚类上色 | 加载 → 智能训练 → 一键上色 | 点输入 / 数据集名 |
| `gh/03_regression_diabetes.gh` | 回归 | 加载 → 分割 → 智能训练 → 预测 → 评估 | 数据集名或 CSV |
| `gh/03b_regression_advanced.gh` | 回归（进阶） | 同上，含更多展示 | 同上 |

旧中文文件名仍保留在同目录，推荐用上面英文编号文件。

## 最快上手（3 步）

1. 双击打开对应 `.gh`（Rhino 已装 SimpleML，并设置 `SIMPLEML_PATH`）
2. 看画布上的 **Panel / 注释**，按电池从左到右理解
3. 把数据源换成你的：见下方「换数据源」

## 换数据源（核心）

**演示数据（默认）**

- `Load Dataset` → `iris` / `diabetes` / `make_blobs` …

**换成你的 CSV（推荐）**

1. 断开或禁用原来的 `Load Dataset`
2. `Read CSV` → 文件路径  
3. `Quick Dataset` → X=特征列，y=标签列（可选）  
4. 把 `Quick Dataset` 的 **Dataset** 接到原来的 `Split` / `Smart Train`

其余电池（Smart Train / Predict / Evaluate）一般**不用改参数**。

## 推荐默认参数（尽量别改）

- **Smart Train**：`Task=auto`（或 `classification` / `regression` / `clustering`）
- **Split**：默认比例即可
- **Evaluate**：接测试集，主要看 **Verdict** 结论句

## 语言切换

- 放一个 `Language` 组件 → 右键 **English / 中文**，或把 Boolean `ZH` 设为 true/false  
- 画布上的组件名 / 端口说明会立即刷新  
- 顶部 Ribbon 搜索名需**重启 Rhino** 后才完全切换（Grasshopper 限制）

## Markdown 配方（无 .gh 时）

- [01_classification_iris.md](01_classification_iris.md)
- [02_clustering_color.md](02_clustering_color.md)
- [03_regression.md](03_regression.md)
- Python 冒烟：`quickstart_all.py`

用 Rhino 重建标准教学画布（需已开 Rhino + `mcpstart`）：

```text
python scripts/build_teaching_gh.py
```
