# SimpleML 示例工作流

在 Grasshopper 中按下列连线即可跑通（无需外部 CSV）。

> 请先运行 **环境体检 Health Check**，确认状态为 `PASS`。

---

## 示例 1：鸢尾花分类（新手推荐）

**目标**：5 分钟完成「数据 → 训练 → 预测 → 评估」

1. `加载示例数据集 Load Dataset` → Dataset Name = `iris`
2. `创建数据集 Create Dataset`（X/y 来自 Load Dataset 输出）
3. `分割数据集 Split Dataset`（默认 0.2）
4. `智能训练 Smart Train` ← Train Dataset，Task = `classification`（或 `auto`）
5. `预测分类 Predict Classifier` ← Model + Test Dataset
6. `评估分类 Evaluate Classification` ← Model + Test Dataset  
   阅读 Report 末尾「通俗解读」
7. （可选）`特征重要性 Feature Importance` ← Model

---

## 示例 2：聚类 + 肘部法则

1. `加载示例数据集 Load Dataset` → `make_blobs`（或自有点数据）
2. `创建数据集 Create Dataset`（无 y 亦可）
3. `肘部法则 Elbow Method` 观察合适的 K
4. `智能训练 Smart Train`，Task = `clustering`，N Clusters = 选中的 K
5. `预测聚类 Predict Cluster`
6. `轮廓系数 Silhouette Score` 或 `评估聚类 Evaluate Clustering`
7. `可视化聚类标签 Visualize Cluster Labels` 给几何上色

---

## 示例 3：回归

1. `加载示例数据集 Load Dataset` → `diabetes` 或 `california_housing`
2. `创建数据集` →（可选打开 Normalize）→ `分割数据集`
3. `智能训练 Smart Train`，Task = `regression`
4. `预测回归` + `评估回归`（查看 R² / RMSE 通俗解读）
5. `可视化回归 Visualize Regression`

---

## 进阶：精细调参

`05 Algorithm` 中的参数电池（如「随机森林分类」）只输出 JSON 参数，  
再连接到 `训练分类器 / 训练回归器 / 训练聚类`。  
新手请优先使用 **智能训练**。
