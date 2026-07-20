# SimpleML 用户手册（详细版）

**版本：** 1.2.0  
**定位：** 在 Grasshopper 中用最少步骤完成机器学习（分类 / 回归 / 聚类）  
**许可：** MIT（免费）

---

## 1. 产品是什么

SimpleML 把 **scikit-learn** 封装成 Grasshopper「电池」，让建筑师、设计师等非算法用户也能：

- 读入 CSV / Excel / 内置示例数据  
- 一键训练模型（智能训练）  
- 预测与评估（含中文「结论」）  
- 把聚类结果上色到几何上  

技术结构：

```
Grasshopper 组件 (.gha, C#)
    → 调用本机 Python
        → components/ + core/（本仓库）
            → scikit-learn
```

---

## 2. 第一次使用（最短路径）

> 开发目录：`D:\Helio\250928_机器学习课程\`（用根目录 `sync_to_helio.bat` 与 GitHub 同步）。  
> 使用前请确认 Grasshopper 中 **环境体检 = PASS**。

### 2.1 分类（推荐演示：鸢尾花）

| 步骤 | 组件 | 要点 |
|------|------|------|
| 1 | 环境体检 | Run=true，Status=PASS |
| 2 | 加载示例数据集 | DN=`iris`，用 **Dataset** 口 |
| 3 | 分割数据集 | 得到 Train / Test |
| 4 | 智能训练 | Dataset=Train，Task=`classification` 或 `auto` |
| 5 | 预测 | Model + Test Dataset |
| 6 | 评估 | Model + Test Dataset → 看 **Verdict** |

也可只放一个 **新手向导**（Task=`classification`），按输出的 Recipe 搜索组件连接。

### 2.2 聚类上色

1. 加载 `make_blobs` 或自有点  
2. 智能训练 → Task=`clustering`，K=3  
3. **一键聚类上色** → Points + Model → Colors 接 Custom Preview  

### 2.3 回归

1. 加载 `diabetes`  
2. 分割 → 智能训练（Task=`regression`）  
3. 预测 → 评估（看 R² 结论）  

图文步骤文件：`examples/01_classification_iris.md` 等。

---

## 3. Ribbon 面板说明

| 面板 | 用途 | 新手优先 |
|------|------|----------|
| 01 Input | 读 CSV/Excel、加载示例 | 加载示例数据集 |
| 02 Analysis | 统计 / 相关 / 描述 | 可选 |
| 03 Dataset | 创建 / 快速创建 / 分割 / 解构 | 快速数据集、分割 |
| 04 Model | **智能训练**、训练*、保存/加载 | **智能训练** |
| 05 Algorithm | 算法参数电池（进阶） | 后置，可不碰 |
| 06 Prediction | **预测**、分类/回归/聚类预测 | **预测** |
| 07 Evaluation | **评估**、分类/回归/聚类评估、特征重要性、轮廓系数 | **评估** |
| 08 Visualization | 上色 / 回归图 / 降维 / 肘部法则 / **一键聚类上色** | 按需 |
| 09 Help | 关于、安装指南、**环境体检**、**新手向导**、重置会话 | 最先用 |

---

## 4. 核心概念

### 4.1 Dataset

封装特征 `X` 与可选标签 `y` 的对象（在连线里以 Generic 传递）。

常见来源：

- 加载示例数据集  
- 快速创建数据集 / 创建数据集（CSV 读入后）  
- 分割数据集 → Train / Test  

### 4.2 Model

训练结果。v1.1+ 起可打包预处理（标准化等），减少预测漂移。

用 **Model Card**（智能训练输出）查看：任务类型、算法、特征数、是否含预处理。

### 4.3 新手 vs 进阶

| 新手 | 进阶 |
|------|------|
| 智能训练 | 05 Algorithm 参数 → 训练分类器/回归器/聚类 |
| 预测 / 评估（自动识别） | 预测分类 / 评估回归 等专用电池 |
| 快速创建数据集 | 创建数据集（标准化、缺失值、异常值） |

---

## 5. 组件使用要点

### 5.1 环境体检

- **Run**：执行检查  
- **AutoFix**：缺失依赖时尝试 `pip install`（需网络）  
- 输出 **Next Steps**：失败时按此修复  

### 5.2 智能训练

输入：Dataset、Task（`auto`/`classification`/`regression`/`clustering`）、可选 Algorithm、K  

输出：

- Model  
- Model Info（树）  
- **Model Card**  
- **Next Steps**（告诉你下一步接哪个组件）  
- Explanation / Readme  

### 5.3 预测 / 评估

- **预测**：自动识别任务；优先接 Dataset，也可接 X  
- **评估**：接 Model + 测试 Dataset；看 **Verdict** 一句话结论  

### 5.4 创建数据集（含预处理）

打开 Normalize / Handle Missing 时，预处理器会打进 Dataset，并在训练时打进 Model。  
同一套预处理请用于后续「原始新数据」预测（可用 Model 的 `predict_raw` 语义；GH 内已处理的 Dataset 不要重复标准化）。

---

## 6. 内置示例数据名

| 名称 | 类型 | 说明 |
|------|------|------|
| iris | 分类 | 经典入门 |
| wine / breast_cancer / digits | 分类 | 更大一点的演示 |
| diabetes / california_housing | 回归 | 回归入门 |
| make_classification / make_regression / make_blobs | 生成数据 | 聚类常用 blobs |

---

## 7. 结果怎么读

评估报告末尾会有类似：

```text
结论: 很强（准确率 96.7%）。可进入试用；仍建议看混淆矩阵是否某类偏弱。
```

| 任务 | 主要看 |
|------|--------|
| 分类 | 准确率、精确率/召回、混淆矩阵、Verdict |
| 回归 | R²、RMSE/MAE、Verdict |
| 聚类 | 轮廓系数、肘部法则、Verdict |

特征重要性：看哪些输入对模型影响更大（相关≠因果）。

---

## 8. 从 CSV 到模型（常见项目流）

1. 读取 CSV / Excel  
2. 快速创建数据集（或创建数据集 + 标准化）  
3. 分割数据集  
4. 智能训练  
5. 预测 / 评估  
6. 保存模型（需要时）  

几何/点云聚类：点坐标作 X → 智能训练(clustering) → 一键聚类上色。

---

## 9. 故障与性能

| 现象 | 处理 |
|------|------|
| 无 SimpleML 面板 | 检查 .gha 路径并重启 |
| 找不到包 | 安装指南 / SIMPLEML_PATH / 重跑 install 脚本 |
| 缺库 | 体检 AutoFix 或 pip |
| 超时 | SIMPLEML_TIMEOUT_MS；重置 Python 会话 |
| 预测离谱 | 确认是否用了与训练一致的预处理；看 Model Card |

长训练时组件会提示「训练中…」；卡住用 **重置 Python 会话**。

---

## 10. 文档地图

| 文件 | 内容 |
|------|------|
| `docs/USER_GUIDE.md` | 本手册 |
| `docs/COMPONENT_REFERENCE.md` | 组件速查 |
| `examples/` | 三个任务配方 + Python 快通 |
| `README.md` | 仓库总览 |
| `sync_to_helio.bat` | 同步到 `D:\Helio\250928_机器学习课程\` |
| `tests/test_simpleml.py` | 自动化测试 |

---

## 11. 支持

- 邮箱：zhao_guijia@outlook.com  
- 仓库：https://github.com/kevinzju2014-sketch/SimpleML  
- 画布内：**关于** 组件  
