# SimpleML 分发指南

## 创建安装包

### 方法1: 使用脚本（推荐）

1. 打开命令行
2. 进入 `INSTALL_PACKAGE` 目录
3. 运行：
   ```bash
   python create_package.py
   ```

脚本将自动：
- 创建 `SimpleML_Package` 文件夹
- 复制所有必需文件
- 创建ZIP压缩包

### 方法2: 手动创建

参考 `MANUAL_PACKAGE_CREATION.md` 文件

## 安装包内容

最终安装包应包含：

```
SimpleML_Package/
├── README.md                    # 安装说明
├── myML/                        # 核心代码
│   ├── components/
│   ├── core/
│   ├── examples/
│   └── requirements.txt
├── Install/                     # 安装脚本
│   ├── install.bat
│   ├── install.sh
│   └── requirements.txt
└── Docs/                        # 文档
    └── [所有文档文件]
```

## 分发步骤

### 1. 创建ZIP文件

运行 `create_package.py` 会自动创建ZIP文件，或手动压缩 `SimpleML_Package` 文件夹。

### 2. 命名规范

使用格式：`SimpleML_v版本号_日期.zip`

示例：`SimpleML_v1.0.0_20260126.zip`

### 3. 测试安装包

在干净的系统中测试：
1. 解压ZIP文件
2. 运行安装脚本
3. 验证功能

### 4. 分发渠道

- GitHub Releases
- 个人网站
- 邮件分发
- 云存储（Google Drive, Dropbox等）

## 用户安装步骤

### Windows用户

1. 下载 `SimpleML_v1.0.0.zip`
2. 解压文件
3. 双击 `Install\install.bat`
4. 按照提示完成安装
5. 重启Grasshopper

### Mac/Linux用户

1. 下载 `SimpleML_v1.0.0.zip`
2. 解压文件
3. 打开终端，进入解压目录
4. 运行：`bash Install/install.sh`
5. 重启Grasshopper

## 版本管理

### 版本号格式

使用语义化版本：`主版本.次版本.修订版本`

- **主版本**：不兼容的API修改
- **次版本**：向后兼容的功能性新增
- **修订版本**：向后兼容的问题修正

### 更新日志

建议创建 `CHANGELOG.md` 记录版本更新：

```markdown
# Changelog

## [1.0.0] - 2026-01-26
### Added
- 初始版本发布
- 8个分类的电池系统
- 15个算法特定训练电池
- Dataset管理功能
- 模型保存和加载功能
```

## 故障排除

### 常见问题

1. **Python未安装**
   - 提示用户安装Python 3.7+

2. **依赖安装失败**
   - 检查网络连接
   - 尝试使用国内镜像：`pip install -i https://pypi.tuna.tsinghua.edu.cn/simple ...`

3. **路径问题**
   - 确保使用绝对路径
   - 检查文件权限

4. **Grasshopper找不到模块**
   - 检查Python路径配置
   - 确认myML文件夹位置正确

## 支持文档

确保包含以下文档：
- README.md - 快速开始
- 安装指南
- 使用文档
- 故障排除指南

---

**创建日期**：2026年1月26日
