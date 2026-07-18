# SimpleML 发布包

本目录包含**已编译**的安装包，可直接拷到你的 Rhino 电脑使用。

| 文件 / 目录 | 说明 |
|-------------|------|
| `SimpleML-1.2.0/` | 解压即用的安装目录（含 `.gha` + `myML` + 文档） |
| `SimpleML-1.2.0.zip` | 同上内容的压缩包 |

## 在你的 Rhino 电脑上安装（推荐）

### Windows

1. 解压 `SimpleML-1.2.0.zip`
2. 将整个 `SimpleML-1.2.0` 文件夹复制到：
   ```
   %APPDATA%\Grasshopper\Libraries\SimpleML\
   ```
   最终应有：
   ```
   ...\Libraries\SimpleML\SimpleML.gha
   ...\Libraries\SimpleML\myML\components\
   ...\Libraries\SimpleML\myML\core\
   ```
3. 安装 Python 依赖（与 Grasshopper 使用同一 Python）：
   ```bat
   python -m pip install -r myML\requirements.txt
   ```
4. **完全退出并重启 Rhino** → 打开 Grasshopper → 运行 **环境体检**

### macOS

1. 解压 zip
2. 复制到（按你的 Rhino 版本选一个）：
   ```
   ~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
   ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
   ```
3. `python3 -m pip install -r myML/requirements.txt`
4. 重启 Rhino → **环境体检**

## 从源码重新编译（可选）

在仓库根目录：

```bash
bash scripts/build_and_install.sh
```

会生成 `dist/SimpleML/` 与 `dist/SimpleML_Install.zip`，并安装到本机 Grasshopper Libraries。
