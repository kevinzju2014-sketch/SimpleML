# SimpleML - Grasshopper 机器学习插件

基于 scikit-learn 的 Grasshopper 机器学习插件（免费 / MIT）。目标：让设计师与非专业用户也能轻松完成分类、回归、聚类。

**版本**: 1.1.0  

## 1.1 亮点

- **跨平台路径**：不再写死 `Administrator` 路径；自动发现 `~/.rhinocode` 与系统 Python
- **环境体检**：`09 Help → 环境体检` 一键检查依赖与路径
- **智能训练**：`04 Model → 智能训练`，Dataset 进、Model 出
- **预处理打包**：标准化/填补随模型保存，避免训练-预测不一致
- **通俗解读**：评估报告附带人话解释；新增特征重要性、轮廓系数
- **完整标签**：组件中英双语名称 + 带文字的图标
- **常驻 Python**：默认复用 Python 会话（可用环境变量关闭）

## 快速开始

1. 安装 Python 依赖（请对 Grasshopper 实际使用的解释器执行）：
   ```bash
   python -m pip install -r requirements.txt
   # macOS/Linux 可用 python3
   ```
2. 将 `SimpleML.gha` 与本仓库 Python 根目录（含 `components/`、`core/`）放到 Grasshopper Libraries，或设置 `SIMPLEML_PATH`
3. 重启 Rhino → 运行 **环境体检** → 打开 `examples/README.md` 按示例连线

### 环境变量（可选）

| 变量 | 含义 |
|------|------|
| `SIMPLEML_PATH` | Python 包根目录 |
| `PYTHON_PATH` | python 可执行文件 |
| `SIMPLEML_TIMEOUT_MS` | 超时毫秒（默认 120000） |
| `SIMPLEML_PERSISTENT_PYTHON` | `1`/`0` 常驻会话 |

## 组件导航（Ribbon）

| 面板 | 内容 |
|------|------|
| 01 Input | 读写 CSV/Excel、加载示例数据 |
| 02 Analysis | 统计 / 相关 / 描述 |
| 03 Dataset | 创建 / 解构 / 分割 |
| 04 Model | **智能训练**、训练分类/回归/聚类、保存/加载 |
| 05 Algorithm | 15 种算法参数电池（进阶） |
| 06 Prediction | 预测 |
| 07 Evaluation | 评估 + 特征重要性 + 轮廓系数 |
| 08 Visualization | 标签着色 / 回归图 / 降维 / 肘部法则 |
| 09 Help | 关于 / 安装指南 / **环境体检** |

## 新手推荐链路

```
加载数据集 → 创建数据集 → 分割数据集 → 智能训练 → 预测 → 评估
```

## 项目结构

```
simpleml/
├── components/          # Python 组件 API
├── core/                # ML 核心（含跨平台 env_bootstrap、model_bundle）
├── GHA_Project/         # C# Grasshopper 插件
├── examples/            # 示例工作流说明 + 快通脚本
├── INSTALL_PACKAGE/     # 打包相关
└── requirements.txt
```

## 支持的算法

- **分类**: 随机森林、SVM、逻辑回归、KNN、决策树、朴素贝叶斯  
- **回归**: 随机森林、SVR、线性、岭、Lasso、KNN  
- **聚类**: K-Means、DBSCAN、层次聚类  

## 许可证

MIT License
