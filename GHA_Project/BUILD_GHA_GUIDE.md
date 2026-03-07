# SimpleML GHA文件生成指南

## 概述

本文档说明如何重新生成SimpleML的GHA文件。所有Python代码已经根据最新文档更新完成，现在需要构建C#组件并生成GHA文件。

## 前置要求

1. **Visual Studio 2019/2022** 或 **.NET SDK 6.0+**
2. **Rhino 8** 已安装
3. **Python 3.7+** 已安装

## 构建步骤

### 方法1: 使用build.bat脚本（推荐）

1. 打开**命令提示符（CMD）**（不是PowerShell），进入项目目录：
   ```cmd
   cd /d "d:\Helio\250928_机器学习课程\myML\GHA_Project"
   ```

2. 运行构建脚本：
   ```cmd
   build.bat
   ```

3. 构建完成后，GHA文件位于：
   ```
   bin\Release\SimpleML.gha
   ```

### 方法2: 使用Visual Studio

1. 打开Visual Studio
2. 打开项目文件：`SimpleML.csproj`
3. 选择 `Release` 配置
4. 右键项目 → `生成` 或按 `Ctrl+Shift+B`
5. 构建完成后，将 `bin\Release\SimpleML.dll` 重命名为 `SimpleML.gha`

### 方法3: 使用命令行（dotnet CLI）

```cmd
cd /d "d:\Helio\250928_机器学习课程\myML\GHA_Project"
dotnet restore
dotnet build -c Release
copy bin\Release\SimpleML.dll bin\Release\SimpleML.gha
```

## 重要说明

### 组件更新状态

根据最新文档（`COMPONENTS_DOCUMENTATION.md`），以下组件需要更新以符合新架构：

#### 已更新的Python代码
- ✅ `components/train_components.py` - 已移除Model Manager输出
- ✅ `components/predict_components.py` - 已更新为直接使用Model
- ✅ `components/evaluate_components.py` - 已更新为直接使用Model
- ✅ 所有算法特定训练组件 - 已更新为参数配置组件

#### 需要更新的C#组件

以下C#组件文件需要手动更新以匹配Python代码的变更：

1. **TrainClassifierComponent.cs**
   - 移除 `Model Manager` 输出
   - 只保留 `Model` 和 `Readme` 输出
   - 更新输入：只保留 `Dataset` 和 `Algorithm`

2. **TrainRegressorComponent.cs**
   - 同上

3. **TrainClusterComponent.cs**
   - 同上

4. **PredictComponent.cs** (需要重命名/创建)
   - 创建 `PredictClassifierComponent.cs`
   - 创建 `PredictRegressorComponent.cs`
   - 创建 `PredictClusterComponent.cs`
   - 输入改为直接接收 `Model`（而不是Model Manager）

5. **EvaluateClassificationComponent.cs**
   - 输入改为直接接收 `Model`（而不是Model Manager）
   - 添加 `Metrics` 输入参数
   - 输出添加 `Report` 和 `Confusion Matrix`

6. **EvaluateRegressionComponent.cs**
   - 同上（无Confusion Matrix）

7. **EvaluateClusteringComponent.cs**
   - 同上（无Confusion Matrix）

8. **SaveModelComponent.cs**
   - 输入改为直接接收 `Model`（而不是Model Manager）
   - 添加 `Save` Boolean输入

## 快速更新方案

如果当前组件可以编译，可以先构建GHA文件进行测试，然后逐步更新组件。

### 步骤1: 构建当前版本

```cmd
cd /d "d:\Helio\250928_机器学习课程\myML\GHA_Project"
build.bat
```

### 步骤2: 测试基本功能

1. 将生成的 `SimpleML.gha` 复制到 Grasshopper 的 Libraries 目录
2. 重启 Grasshopper
3. 测试基本的数据读取和训练功能

### 步骤3: 逐步更新组件

根据使用情况，逐步更新需要修改的组件文件。

## 组件文件位置

所有组件C#文件位于：
```
GHA_Project/Components/
├── DataInput/
├── DataAnalysis/
├── DataPreprocessing/
├── DatasetManagement/
├── ModelTraining/
├── ModelPrediction/
├── ModelEvaluation/
└── ModelManagement/
```

## 故障排除

### 问题1: 找不到MSBuild

**解决方案**：
- 安装 Visual Studio 2019/2022
- 或安装 Visual Studio Build Tools
- 或安装 .NET SDK

### 问题2: 找不到Grasshopper.dll

**解决方案**：
编辑 `SimpleML.csproj`，修改以下路径为您的Rhino 8安装路径：
```xml
<Reference Include="Grasshopper">
  <HintPath>C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll</HintPath>
</Reference>
<Reference Include="RhinoCommon">
  <HintPath>C:\Program Files\Rhino 8\System\RhinoCommon.dll</HintPath>
</Reference>
```

### 问题3: 构建成功但组件不显示

**解决方案**：
1. 检查GHA文件是否在正确的目录
2. 检查文件是否损坏
3. 查看Grasshopper的错误日志
4. 确保所有依赖项正确

## 下一步

1. ✅ Python代码已更新完成
2. ⏳ 构建GHA文件
3. ⏳ 更新C#组件以匹配Python代码
4. ⏳ 测试所有组件功能
5. ⏳ 生成最终版本的GHA文件

## 联系

如有问题，请检查：
- `COMPONENTS_DOCUMENTATION.md` - 组件文档
- `FINAL_MODIFICATIONS_COMPLETE.md` - Python代码修改总结
- `GHA_BUILD_GUIDE.md` - 详细构建指南
