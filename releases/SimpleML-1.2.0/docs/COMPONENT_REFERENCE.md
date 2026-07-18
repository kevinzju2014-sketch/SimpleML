# SimpleML 组件速查表（1.2.0）

搜索时可用中文昵称或英文名。下表为上架/教学常用子集；完整列表以 Grasshopper 面板为准。

---

## 09 Help（请先用）

| 组件 | 昵称 | 作用 |
|------|------|------|
| 环境体检 Health Check | 体检 | 检查 Python/依赖/路径；AutoFix 可 pip |
| 新手向导 Beginner Wizard | 新手向导 | 输出接线配方 + 可选体检 |
| 安装指南 Installation Guide | 安装指南 | 跨平台安装说明 |
| 重置Python会话 Reset Python | 重置会话 | 终止卡住的 Python |
| 关于 About | 关于 | 版本与联系方式 |

## 01 Input

| 组件 | 作用 |
|------|------|
| 加载示例数据集 | 输出 **Dataset** + Features/Target/Feature Names |
| 读取 CSV / Excel | 表格入 GH |
| 写入 CSV / Excel | 导出 |

## 03 Dataset

| 组件 | 作用 |
|------|------|
| 快速创建数据集 | 简洁：X + 可选 y → Dataset |
| 创建数据集 | 含标准化/缺失值/异常值（进阶） |
| 分割数据集 | Train / Test |
| 解构数据集 | Dataset → X / y |

## 04 Model

| 组件 | 作用 |
|------|------|
| **智能训练** | Dataset → Model + Card + Next Steps |
| 训练分类器/回归器/聚类 | 进阶；可接 05 算法参数 |
| 保存/加载模型 | joblib 文件 |

## 05 Algorithm（进阶，面板靠后）

随机森林 / SVM / 逻辑回归 / KNN / 决策树 / 朴素贝叶斯；  
线性/岭/Lasso/SVR/KNN 回归；  
K-Means / DBSCAN / 层次聚类。  
输出 JSON 参数 → 接到「训练*」。

## 06 Prediction

| 组件 | 作用 |
|------|------|
| **预测** | 自动识别任务；Dataset 或 X |
| 预测分类/回归/聚类 | 专用（次要曝光） |

## 07 Evaluation

| 组件 | 作用 |
|------|------|
| **评估** | 自动识别；Verdict + Report |
| 评估分类/回归/聚类 | 专用 |
| 特征重要性 | 解释特征贡献 |
| 轮廓系数 | 聚类质量 |

## 08 Visualization

| 组件 | 作用 |
|------|------|
| 一键聚类上色 | Points → Colors |
| 可视化聚类/分类标签 | 按标签着色 |
| 可视化回归 | 散点与拟合 |
| 降维 | PCA / t-SNE |
| 肘部法则 | 选 K |

---

## 典型连线（记忆口诀）

```
体检 → 加载 → 分割 → 智能训练 → 预测 → 评估
```

进阶：

```
读 CSV → 创建数据集(标准化) → 分割 → 算法参数 → 训练* → 预测 → 评估 → 保存
```
