# SimpleML

Grasshopper 机器学习插件（免费 / MIT）  
基于 scikit-learn，面向设计师与教学场景。兼容 **Rhino 7+**（Windows / macOS）。

**版本**: 1.2.0  

开发目录约定（本机）：

```text
D:\Helio\250928_机器学习课程\
```

云端仓库与上述目录应保持同步（见 `sync_to_helio.bat`）。

## 最终开发结构

```text
SimpleML/
├── components/       # Python 组件 API
├── core/             # ML 核心
├── GHA_Project/      # C# Grasshopper 插件源码
├── examples/         # 可用示例（配方 + 冒烟脚本）
├── tests/            # 自动化测试
├── docs/             # 用户手册 / 组件参考
├── scripts/          # 构建脚本
├── requirements.txt
└── README.md
```

## Language (EN / ZH)

Default: **English**.

Use **SimpleML → 09 Help → Language**:

- Boolean `Chinese`: `false` = English, `true` = Chinese  
- Or **right-click** the component → `English` / `中文 Chinese`

Canvas component names and explanation texts refresh automatically — **no Rhino restart**.

Preference: `%APPDATA%\Grasshopper\SimpleML\language.txt`  
Env: `SIMPLEML_LANG=en|zh`

## 构建插件

需本机安装 Rhino + .NET SDK：

```bash
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
# 无本机 Rhino 程序集时可用 NuGet：
dotnet build GHA_Project/SimpleML.csproj -c Release -p:UseNuGetRhino=true
```

产物：`GHA_Project/bin/Release/SimpleML.gha`

开发期可将 `.gha` 与本仓库根目录（含 `components/`、`core/`）放到 Grasshopper Libraries，或设置 `SIMPLEML_PATH` 指向本仓库根。

## 测试

```bash
python -m pip install -r requirements.txt
python tests/test_simpleml.py -v
python examples/quickstart_all.py
```

## 可用示例

| 文件 | 说明 | 状态 |
|------|------|------|
| `examples/01_classification_iris.md` | 鸢尾花分类配方 | 可用 |
| `examples/02_clustering_color.md` | 聚类上色配方 | 可用 |
| `examples/03_regression.md` | 回归配方 | 可用 |
| `examples/quickstart_all.py` | 三任务 Python 冒烟 | 可用 |

## 文档

- [docs/USER_GUIDE.md](docs/USER_GUIDE.md)
- [docs/COMPONENT_REFERENCE.md](docs/COMPONENT_REFERENCE.md)

## 与本机文件夹同步

在 Windows 上双击仓库中的 `sync_to_helio.bat`，会把当前分支同步到：

`D:\Helio\250928_机器学习课程\`

## 许可证

MIT
