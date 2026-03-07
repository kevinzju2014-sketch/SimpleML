# 修复所有 GUID 格式错误

## 发现的问题

### 问题组件：InstallationGuideComponent
**错误 GUID**：`"G2H3I4J5-K6L7-8901-MNOP-QR1234567890"`

**问题**：包含非十六进制字符：
- G, H, I, J, K, L, M, N, O, P, Q, R

**GUID 规则**：
- 只能包含十六进制字符：0-9, A-F
- 不能包含 G-Z

**已修复**：✅ 已更新为正确的 GUID 格式：`"A2B3C4D5-E6F7-8901-ABCD-EF1234567890"`

## 修复步骤

### 步骤 1: 重新构建项目

1. 在 Visual Studio 中：
   - `生成` → `清理解决方案`
   - `生成` → `重新生成解决方案`
   - 确认没有错误

2. 验证 GHA 文件：
   - 位置：`bin\Release\SimpleML.gha`
   - 大小：应该约 405KB

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
   - "Test SimpleML"
   - "About"
   - "Installation Guide"
4. 应该能找到所有组件了！

## 已修复的 GUID

| 组件 | 旧 GUID（错误） | 新 GUID（正确） |
|------|----------------|----------------|
| InstallationGuideComponent | G2H3I4J5-K6L7-8901-MNOP-QR1234567890 | A2B3C4D5-E6F7-8901-ABCD-EF1234567890 |
| TestComponent | TEST-COMPONENT-GUID-123456789012 | A1B2C3D4-E5F6-7890-ABCD-EF1234567890 |

## GUID 格式规则

**正确格式**：
```
XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
```
- 每个 X 必须是十六进制字符：0-9, A-F
- 总共 32 个十六进制字符，用连字符分隔

**错误示例**：
- ❌ `"TEST-COMPONENT-GUID-123456789012"` - 包含 T, E, S, C, O, M, P, N, U, D, G
- ❌ `"G2H3I4J5-K6L7-8901-MNOP-QR1234567890"` - 包含 G, H, I, J, K, L, M, N, O, P, Q, R

**正确示例**：
- ✅ `"A1B2C3D4-E5F6-7890-ABCD-EF1234567890"`
- ✅ `"12345678-1234-1234-1234-123456789012"`

## 验证清单

修复后，确认：

- [ ] 重新构建了项目（使用修复后的 GUID）
- [ ] GHA 文件已重新生成
- [ ] 删除了旧的 GHA 文件
- [ ] 复制了新的 GHA 文件到正确位置
- [ ] 完全关闭并重新启动了 Rhino
- [ ] 在 Grasshopper 中能搜索到组件
- [ ] 没有 GUID 格式错误

---

*最后更新：2026-01-27*
