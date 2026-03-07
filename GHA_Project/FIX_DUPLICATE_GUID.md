# 修复重复的 GUID

## 发现的问题

### 问题：组件 GUID 冲突
**错误信息**：`Grasshopper has been asked to load multiple components that share the same ID`

**冲突的组件**：
- `CreateDatasetComponent` - GUID: `A1B2C3D4-E5F6-7890-ABCD-EF1234567890`
- `TestComponent` - GUID: `A1B2C3D4-E5F6-7890-ABCD-EF1234567890`

**问题**：两个组件使用了相同的 GUID，Grasshopper 无法区分它们。

**已修复**：✅ 已更新 `TestComponent` 的 GUID 为：`T1E2S3T4-C5O6-7890-ABCD-EF1234567890`

## 修复步骤

### 步骤 1: 重新构建项目

1. 在 Visual Studio 中：
   - `生成` → `清理解决方案`
   - `生成` → `重新生成解决方案`
   - 确认没有错误

### 步骤 2: 重新安装 GHA 文件

运行修复脚本：
```powershell
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
.\fix_guid_and_path.ps1
```

或手动操作：
1. 完全关闭 Rhino
2. 删除旧文件：`%APPDATA%\Grasshopper\Libraries\SimpleML.gha`
3. 复制新文件：从 `bin\Release\SimpleML.gha` 到 `%APPDATA%\Grasshopper\Libraries\`
4. 重新启动 Rhino

### 步骤 3: 验证修复

1. 打开 Rhino
2. 打开 Grasshopper
3. 搜索组件：
   - "Test SimpleML" - 应该能找到
   - "Create Dataset" - 应该能找到
4. 不应该再出现 GUID 冲突错误

## GUID 唯一性规则

**重要**：每个组件必须有唯一的 GUID！

- ✅ 每个组件使用不同的 GUID
- ❌ 多个组件使用相同的 GUID（会导致冲突）

## 验证所有 GUID 唯一性

运行验证脚本检查是否有重复的 GUID：
```powershell
.\validate_all_guids.ps1
```

## 已修复的 GUID

| 组件 | 旧 GUID（冲突） | 新 GUID（唯一） |
|------|----------------|----------------|
| TestComponent | A1B2C3D4-E5F6-7890-ABCD-EF1234567890 | T1E2S3T4-C5O6-7890-ABCD-EF1234567890 |
| CreateDatasetComponent | A1B2C3D4-E5F6-7890-ABCD-EF1234567890 | （保持不变） |

## 验证清单

修复后，确认：

- [ ] 重新构建了项目（使用修复后的 GUID）
- [ ] GHA 文件已重新生成
- [ ] 删除了旧的 GHA 文件
- [ ] 复制了新的 GHA 文件到正确位置
- [ ] 完全关闭并重新启动了 Rhino
- [ ] 在 Grasshopper 中没有 GUID 冲突错误
- [ ] 所有组件都能正常使用

---

*最后更新：2026-01-27*
