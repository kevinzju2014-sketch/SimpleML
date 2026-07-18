# SimpleML 安装指南（Rhino 本机）

面向：在你当前的 Rhino 电脑上安装并验收插件。  
兼容：**Rhino 7 及以上** · **Windows / macOS** · 版本 **1.2.0**

---

## 一、你需要准备什么

| 项目 | 要求 |
|------|------|
| Rhino | 7 或 8（或更新） |
| Grasshopper | 随 Rhino 自带 |
| Python | 3.9+（`python` / `python3`） |
| 本仓库 | 已拉取到本机（含 `components/`、`core/`） |
| 编译产物 | `SimpleML.gha`（需在装有 Rhino 的机器上编译） |

---

## 二、推荐安装流程（10 分钟）

### 最快：使用已编译发布包

1. 下载 / 打开仓库中的 `releases/SimpleML-1.2.0.zip`
2. 解压，将内容放到 Grasshopper `Libraries/SimpleML/`（见 `releases/README.md`）
3. `pip install -r myML/requirements.txt`
4. 重启 Rhino → **环境体检**

### 步骤 A：安装 Python 依赖（从源码时）

在**将要被插件调用的同一个 Python** 中执行：

```bash
# Windows（PowerShell / CMD）
python -m pip install -r requirements.txt

# macOS
python3 -m pip install -r requirements.txt
```

依赖：`scikit-learn`、`numpy`、`pandas`、`joblib`、`openpyxl`。

### 步骤 B：编译插件

**一键（推荐，无本机 Rhino 也可用 NuGet 编译）：**

```bash
bash scripts/build_and_install.sh
```

产物：`dist/SimpleML/SimpleML.gha`、`dist/SimpleML_Install.zip`，并写入 Libraries。

**或手动：**

```bash
cd GHA_Project
dotnet build SimpleML.csproj -c Release -p:RhinoMajorVersion=7
# 无本机 Rhino 时：
dotnet build SimpleML.csproj -c Release -p:UseNuGetRhino=true
```

成功后生成：

```
GHA_Project/bin/Release/SimpleML.gha
```

> 只用 Rhino 8 时可改 `-p:RhinoMajorVersion=8`。  
> 默认优先引用 Rhino 7，便于一份 GHA 兼容 7/8。

### 步骤 C：一键部署（推荐）

**Windows**（双击或 CMD）：

```bat
install.bat
```

**macOS / Linux**：

```bash
bash install.sh
# 或完整编译+安装：
bash scripts/build_and_install.sh
```

脚本会：

1. `pip install` 依赖  
2. 复制 `components` / `core` 到 Grasshopper Libraries 下的 `SimpleML/myML`  
3. 若已编译，复制 `SimpleML.gha`

### 步骤 D：手动部署（不用脚本时）

**Windows 目标目录：**

```
%APPDATA%\Grasshopper\Libraries\SimpleML\
```

放入：

```
SimpleML\
  SimpleML.gha
  myML\
    components\
    core\
    requirements.txt
```

也可把 `SimpleML.gha` 直接放在 `Libraries\`，把 `myML` 放旁边。

**macOS 目标目录（按你的 Rhino 主版本选一个）：**

```
~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
```

### 步骤 E：重启并验收

1. **完全退出** Rhino 后重新打开  
2. 打开 Grasshopper  
3. 搜索并放置：**环境体检**  
4. `Run = true`  
5. 看输出：
   - `Status = PASS` → 可以继续  
   - `FAIL` → 阅读 `Next Steps`，或打开 **安装指南** / 将 `AutoFix = true` 再跑一次  

可选环境变量（一般脚本装好后不必设）：

| 变量 | 用途 |
|------|------|
| `SIMPLEML_PATH` | 指向含 `components`+`core` 的目录 |
| `PYTHON_PATH` | 指向 python.exe / python3 |
| `SIMPLEML_TIMEOUT_MS` | 超时毫秒（默认 120000） |

---

## 三、5 分钟上手验收（建议你照做）

在 Grasshopper 画布上：

1. **新手向导** → Task = `classification`，Run Health Check = true  
2. **加载示例数据集** → `iris` → 使用 **Dataset** 输出  
3. **分割数据集**  
4. **智能训练** ← Train Dataset  
5. **预测** ← Model + Test Dataset  
6. **评估** ← Model + Test Dataset → 看 **Verdict**

预期：体检 PASS；评估结论类似「很强 / 可用」。

更多练习见 `examples/` 与 `docs/USER_GUIDE.md`。

---

## 四、常见问题

### 1）组件面板没有 SimpleML

- 确认 `.gha` 在 Libraries 中  
- 关闭 Rhino 再开  
- macOS 确认 7.0 / 8.0 路径是否对应你正在用的 Rhino  

### 2）提示找不到 myML / SIMPLEML_PATH

- 确认目录内有 `components` 与 `core`  
- 再跑 `install.bat` / `install.sh`  
- 或设置 `SIMPLEML_PATH`  

### 3）ModuleNotFoundError: sklearn / pandas

- 对插件实际使用的 Python 再执行一次 pip  
- 环境体检看 `Python` 输出路径是否一致  
- 可试 `AutoFix = true`  

### 4）训练超时 / 卡住

- 增大 `SIMPLEML_TIMEOUT_MS`（如 `300000`）  
- 使用 **重置 Python 会话**  

### 5）Rhino 7 没有 Rhinocode

- 正常。请用系统 / Homebrew 的 `python3`，并用 `PYTHON_PATH` 指过去。  

---

## 五、本机验收检查表（给你打勾）

- [ ] `pip install -r requirements.txt` 成功  
- [ ] 已生成 / 已复制 `SimpleML.gha`  
- [ ] Libraries 下可见 `SimpleML` 或 `myML`  
- [ ] 重启 Rhino 后能搜到「环境体检」「智能训练」  
- [ ] 环境体检 = **PASS**  
- [ ] iris：加载 → 分割 → 智能训练 → 预测 → 评估 跑通  
- [ ] （可选）make_blobs + 一键聚类上色  
- [ ] （可选）diabetes 回归评估有 Verdict  

全部勾选后，即可按日常项目使用，并准备 Food4Rhino / Yak 上架（见 `FOOD4RHINO.md`）。
