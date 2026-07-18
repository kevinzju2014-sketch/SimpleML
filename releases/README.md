# SimpleML 发布包

本目录包含**已编译**的安装包，可直接拷到你的 Rhino 电脑使用。

| 文件 / 目录 | 说明 |
|-------------|------|
| `SimpleML-1.2.0/` | 解压即用的安装目录（含 `.gha` + `myML` + 文档） |
| `SimpleML-1.2.0.zip` | 同上内容的压缩包 |

## 在你的 Rhino 电脑上安装（推荐）

> 云端无法替你打开 Rhino 界面；请在**你的电脑**上执行下面一步，即可加载插件。

### Windows（一键）

1. 把整个 `releases/` 文件夹拷到本机（或只拷 zip + bat）
2. 双击 **`install_on_rhino_pc.bat`**
3. **完全退出并重启 Rhino** → Grasshopper → 运行 **环境体检**

### macOS（一键）

```bash
cd releases
chmod +x install_on_rhino_pc.sh
./install_on_rhino_pc.sh
```

然后重启 Rhino → **环境体检**。

### 手动（可选）

解压 `SimpleML-1.2.0.zip`，复制到：

- Windows: `%APPDATA%\Grasshopper\Libraries\SimpleML\`
- macOS: `~/Library/Application Support/McNeel/Rhinoceros/7.0|8.0/Plug-ins/Grasshopper/Libraries/SimpleML/`

再执行 `pip install -r myML/requirements.txt`。

## 从源码重新编译（可选）

在仓库根目录：

```bash
bash scripts/build_and_install.sh
```

会生成 `dist/SimpleML/` 与 `dist/SimpleML_Install.zip`，并安装到本机 Grasshopper Libraries。
