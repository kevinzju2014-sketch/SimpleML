# 修复组件不显示在菜单栏的问题

## 问题描述
GHA 文件已被 Rhino/Grasshopper 加载，但组件没有显示在 Grasshopper 的菜单栏（标签栏）中。

## 原因分析

最常见的原因是 **Tab Layout（标签布局）** 问题。如果 Grasshopper 使用了自定义的 Tab Layout，新组件可能不会自动显示在标签栏中，即使它们已经被加载。

## 解决方案

### 方案 1: 检查组件是否已加载（首先验证）

**方法 1: 使用搜索功能**
1. 在 Grasshopper 画布上**双击空白处**（或按 `Tab` 键）
2. 输入组件名称，例如：`Read CSV` 或 `SimpleML`
3. 如果组件出现在搜索结果中，说明**组件已加载**，只是没有显示在标签栏

**方法 2: 查看已加载的插件**
1. 打开 Grasshopper
2. 查看 `文件` → `特殊文件夹` → `组件文件夹`
3. 或查看 Grasshopper 的 About 对话框，查看所有已加载的 GHA 文件

### 方案 2: 检查并重置 Tab Layout（最可能的原因）

**步骤 1: 检查当前 Tab Layout**
1. 在 Grasshopper 中，**右键点击标签栏区域**（组件面板顶部的标签）
2. 查看是否有多个 Tab Layout 选项
3. 查看当前激活的是哪个 Layout

**步骤 2: 重置 Tab Layout**
1. 右键点击标签栏
2. 选择 `Reset Tab Layout` 或 `Default Tab Layout`
3. 或者选择 `Show All Tabs`（如果可用）

**步骤 3: 手动添加 SimpleML 标签**
1. 右键点击标签栏
2. 选择 `Customize` 或 `Edit Tab Layout`（如果可用）
3. 查找 "SimpleML" 分类
4. 确保 SimpleML 及其子分类被添加到标签栏

### 方案 3: 使用组件搜索（临时解决方案）

如果组件已加载但不在标签栏中：
1. 在画布上**双击**（或按 `Tab` 键）
2. 输入组件名称搜索
3. 从搜索结果中拖拽组件到画布

### 方案 4: 检查组件分类名称

如果上述方法都不行，可能是组件分类名称问题。检查组件构造函数中的分类名称：

```csharp
: base("Component Name", "Nickname",
      "Description",
      "SimpleML", "SubCategory")
```

确保：
- 主分类是 "SimpleML"
- 子分类名称正确（如 "01 Data Input", "02 Data Analysis" 等）

### 方案 5: 清除 Grasshopper 缓存

如果 Tab Layout 重置后仍然不显示：

1. **完全关闭** Rhino 和 Grasshopper
2. **删除 Grasshopper 缓存**（可选，谨慎操作）:
   ```
   %APPDATA%\Grasshopper\*.cache
   ```
3. **重新启动** Rhino 和 Grasshopper

## 验证组件是否已加载

### 快速测试

1. 打开 Grasshopper
2. 在画布上**双击空白处**
3. 输入以下任一组件名称：
   - `Read CSV`
   - `About`
   - `SimpleML`
4. 如果组件出现在搜索结果中 → **组件已加载** ✅
5. 如果组件不出现 → **组件未加载** ❌（需要检查其他问题）

## 如果组件确实未加载

如果搜索也找不到组件，说明组件没有被正确加载。可能的原因：

### 1. 检查 GHA 文件位置
- 确认文件在: `%APPDATA%\Grasshopper\Libraries\SimpleML.gha`
- 文件大小正确（约 405KB）
- 文件扩展名是 `.gha`（不是 `.dll`）

### 2. 检查 Rhino 错误日志
- 打开 Rhino
- 查看底部命令行窗口
- 启动 Grasshopper 时查看是否有错误信息

### 3. 检查组件类实现
- 所有组件都必须是 `public class`
- 所有组件都必须继承自 `GH_Component`
- 所有组件都必须有唯一的 `ComponentGuid`

### 4. 重新构建并重新安装
1. 在 Visual Studio 中重新构建项目
2. 完全关闭 Rhino
3. 删除旧的 GHA 文件
4. 复制新的 GHA 文件
5. 重新启动 Rhino

## 常见问题

### Q1: 组件在搜索结果中能找到，但不在标签栏中

**A**: 这是 Tab Layout 问题。按照方案 2 重置 Tab Layout。

### Q2: 组件完全找不到（搜索也没有）

**A**: 组件可能没有正确加载。检查：
- GHA 文件位置
- Rhino 错误日志
- 组件类实现

### Q3: 只有部分组件显示

**A**: 可能是组件分类问题。检查所有组件的分类名称是否一致。

### Q4: 之前能显示，现在不显示了

**A**: 可能是 Tab Layout 被更改了。重置 Tab Layout。

## 快速修复步骤总结

1. ✅ **验证组件是否已加载**：在画布上双击，搜索组件名称
2. ✅ **如果已加载但不在标签栏**：右键标签栏 → 重置 Tab Layout
3. ✅ **如果未加载**：检查 GHA 文件位置和 Rhino 错误日志
4. ✅ **重新构建并安装**：如果问题持续，重新构建项目

## 下一步

修复后，组件应该：
- ✅ 出现在组件搜索中
- ✅ 显示在 SimpleML 标签下
- ✅ 可以正常拖拽到画布使用

---

*最后更新：2026-01-27*
