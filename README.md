# SimpleML - Grasshopper 机器学习插件

基于 scikit-learn 的 Grasshopper 机器学习插件（免费 / MIT）。  
目标：让设计师与非专业用户也能轻松完成分类、回归、聚类。

**版本**: 1.2.0  

## 兼容性（原则）

| 项目 | 支持 |
|------|------|
| Rhino | **7 及以上**（含 8、后续版本） |
| 系统 | **Windows** 与 **macOS** |
| 运行时 | .NET Framework 4.8（`net48`） |
| Python | 3.9+（Rhino 7 用系统/Homebrew；Rhino 8+ 也可用 Rhinocode） |

构建时**默认优先引用 Rhino 7 程序集**，以便同一 `.gha` 向前兼容到 Rhino 8+。

## 1.2 上架向体验

- **新手向导** / **环境体检 AutoFix** / **统一预测·评估**
- **快速数据集**、**一键聚类上色**、**重置 Python 会话**
- `install.bat` / `install.sh` + `yak/manifest.yml` + `FOOD4RHINO.md`
- Ribbon：智能训练与统一组件置顶，高级算法电池后置

## 1.1.x 亮点

- 跨平台路径与 Python 发现（Win / Mac，Rhino 7/8+）
- 环境体检、智能训练、通俗评估解读
- 预处理随模型打包；特征重要性 / 轮廓系数
- 中英双语组件标签与文字图标

## 文档（请从这里开始）

| 文档 | 说明 |
|------|------|
| **[docs/INSTALL_GUIDE.md](docs/INSTALL_GUIDE.md)** | 在你的 Rhino 电脑上安装与验收 |
| [docs/USER_GUIDE.md](docs/USER_GUIDE.md) | 详细用户手册与最短工作流 |
| [docs/COMPONENT_REFERENCE.md](docs/COMPONENT_REFERENCE.md) | 组件一览 |
| [docs/README.md](docs/README.md) | 文档索引 |
| [tests/README.md](tests/README.md) | 自动化测试说明 |

## 快速开始（你的 Rhino 电脑）

**最快：用已编译包**

1. 解压 [`releases/SimpleML-1.2.0.zip`](releases/SimpleML-1.2.0.zip)
2. 复制到 Grasshopper `Libraries/SimpleML/`（见 [releases/README.md](releases/README.md)）
3. `python -m pip install -r myML/requirements.txt`（或 `python3`）
4. 重启 Rhino → **环境体检** → 按 [docs/FOR_RHINO_PC.md](docs/FOR_RHINO_PC.md) 验收

**或从源码一键编译 + 安装：**

```bash
bash scripts/build_and_install.sh
```

也会生成 `dist/SimpleML/` 与 zip。Windows 可用 `install.bat`；Mac 可用 `./install.sh`。

或手动：

1. `python3 -m pip install -r requirements.txt`（对 GH 实际使用的解释器）
2. 将 `SimpleML.gha` 与本仓库根（含 `components/`、`core/`）放到 Grasshopper Libraries，或设置 `SIMPLEML_PATH`
3. 重启 Rhino → **环境体检** → [examples/README.md](examples/README.md)

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

## 测试

```bash
python3 -m pip install -r requirements.txt
python3 tests/test_simpleml.py -v
python3 examples/quickstart_all.py
```

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
├── docs/                # 安装指南 / 用户手册 / 组件参考
├── components/          # Python 组件 API
├── core/                # ML 核心（env_bootstrap / model_bundle）
├── GHA_Project/         # C# Grasshopper 插件（RhinoCompat）
├── examples/            # 示例工作流与快速脚本
├── tests/               # 自动化测试
├── install.bat / .sh    # 本机一键安装到 Grasshopper Libraries
├── yak/                 # PackageManager 清单
└── requirements.txt
```

## 许可证

MIT License
