# 在你的 Rhino 电脑上安装并试用（一页纸）

本页是最短路径；完整说明见 [INSTALL_GUIDE.md](INSTALL_GUIDE.md)。

## 1. 拿到已编译包（最快）

仓库里已有发布包：

- `releases/SimpleML-1.2.0.zip`（或目录 `releases/SimpleML-1.2.0/`）

解压后复制到 Grasshopper `Libraries/SimpleML/`，再装 Python 依赖。详见 `releases/README.md`。

## 2. 或从源码一键编译 + 安装

```bash
bash scripts/build_and_install.sh
```

会：安装 Python 依赖 → 编译 `SimpleML.gha`（无本机 Rhino 时用 NuGet）→ 生成 `dist/SimpleML/` → 安装到 Libraries。

本机有 Rhino 时也可：

```bash
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
```

然后：

- **Windows**：双击 `install.bat`
- **macOS**：`chmod +x install.sh && ./install.sh`

## 3. 重启 Rhino → 打开 Grasshopper

搜索并放置这些组件，按顺序连线：

1. **环境体检** → `Run=true` → 期望 `PASS`
2. **加载数据集** → `iris` → 用 **Dataset** 输出
3. **分割数据集**
4. **智能训练**
5. **预测** / **评估** → 看 **Verdict**

也可先放 **新手向导**，按它输出的步骤连线。

## 4. 给你评估用的检查表

- [ ] 组件面板能搜到 SimpleML / 环境体检 / 智能训练  
- [ ] 环境体检 PASS  
- [ ] iris 分类跑通，有准确率与结论  
- [ ] （可选）diabetes 回归、make_blobs 聚类上色  

遇到问题：看体检的 `Next Steps`，或打开文档 `USER_GUIDE.md` 故障排除一节。

## 文档与测试（本机可选）

```bash
python3 -m pip install -r requirements.txt
python3 tests/test_simpleml.py -v
python3 examples/quickstart_all.py
```
