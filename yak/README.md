# Yak 打包说明

1. 编译插件（建议 Rhino 7 引用）：
   ```bash
   dotnet build ../GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
   ```
2. 创建发布目录：
   ```bash
   mkdir -p dist
   cp ../GHA_Project/bin/Release/SimpleML.gha dist/
   mkdir -p dist/myML
   cp -R ../components ../core ../requirements.txt ../examples dist/myML/
   cp manifest.yml dist/
   ```
3. 在 `dist/` 中执行 `yak build`（需安装 Yak CLI）并上传 Food4Rhino。

详见仓库根目录 `FOOD4RHINO.md`。
