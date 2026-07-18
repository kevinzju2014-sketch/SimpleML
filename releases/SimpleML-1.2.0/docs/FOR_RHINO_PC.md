# 在你的 Rhino 电脑上安装并试用（一页纸）

本页是最短路径；完整说明见 [INSTALL_GUIDE.md](INSTALL_GUIDE.md)。

## 1. 拿到代码

把本仓库拉到本机（或下载 ZIP 解压）。

## 2. 编译 `.gha`（只需一次）

本机需已安装 Rhino 与 .NET SDK：

```bash
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
```

产物：`GHA_Project/bin/Release/SimpleML.gha`

## 3. 一键安装

- **Windows**：双击仓库根目录 `install.bat`
- **macOS**：`chmod +x install.sh && ./install.sh`

## 4. 重启 Rhino → 打开 Grasshopper

搜索并放置这些组件，按顺序连线：

1. **环境体检** → `Run=true` → 期望 `PASS`
2. **加载数据集** → `iris` → 用 **Dataset** 输出
3. **分割数据集**
4. **智能训练**
5. **预测** / **评估** → 看 **Verdict**

也可先放 **新手向导**，按它输出的步骤连线。

## 5. 给你评估用的检查表

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
