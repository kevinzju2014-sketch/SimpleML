# SimpleML - Grasshopper 机器学习插件

基于 scikit-learn 的 Grasshopper 机器学习插件（免费 / MIT）。  
目标：让设计师与非专业用户也能轻松完成分类、回归、聚类。

**版本**: 1.1.1  

## 兼容性（原则）

| 项目 | 支持 |
|------|------|
| Rhino | **7 及以上**（含 8、后续版本） |
| 系统 | **Windows** 与 **macOS** |
| 运行时 | .NET Framework 4.8（`net48`） |
| Python | 3.9+（Rhino 7 用系统/Homebrew；Rhino 8+ 也可用 Rhinocode） |

构建时**默认优先引用 Rhino 7 程序集**，以便同一 `.gha` 向前兼容到 Rhino 8+。

## 1.1.x 亮点

- 跨平台路径与 Python 发现（Win / Mac，Rhino 7/8+）
- 环境体检、智能训练、通俗评估解读
- 预处理随模型打包；特征重要性 / 轮廓系数
- 中英双语组件标签与文字图标

## 快速开始

1. 安装 Python 依赖（请对 Grasshopper 实际使用的解释器执行）：
   ```bash
   python3 -m pip install -r requirements.txt
   ```
2. 将 `SimpleML.gha` 与本仓库 Python 根目录（含 `components/`、`core/`）放到 Grasshopper Libraries，或设置 `SIMPLEML_PATH`
3. 重启 Rhino → 运行 **环境体检** → 按 `examples/README.md` 连线

### 安装位置

**Windows**
```
%APPDATA%\Grasshopper\Libraries\SimpleML\
```

**macOS**
```
~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
```

### 环境变量（可选）

| 变量 | 含义 |
|------|------|
| `SIMPLEML_PATH` | Python 包根目录 |
| `PYTHON_PATH` | python / python3 可执行文件 |
| `SIMPLEML_TIMEOUT_MS` | 超时毫秒（默认 120000） |
| `SIMPLEML_PERSISTENT_PYTHON` | `1`/`0` 常驻会话 |

## 编译

```bash
# 默认：自动优先 Rhino 7，否则 8/9
dotnet build GHA_Project/SimpleML.csproj -c Release

# 指定主版本
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=8

# 手动指定 DLL 目录（Mac/自定义安装很有用）
dotnet build GHA_Project/SimpleML.csproj -c Release \
  -p:RhinoSystemDir="/path/to/Rhino/SystemOrResources" \
  -p:RhinoGrasshopperDir="/path/to/Grasshopper"
```

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
├── core/                # ML 核心（env_bootstrap / model_bundle）
├── GHA_Project/         # C# Grasshopper 插件（RhinoCompat）
├── examples/            # 示例工作流
├── INSTALL_PACKAGE/     # 打包相关
└── requirements.txt
```

## 许可证

MIT License
