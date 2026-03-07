# 批量更新所有组件以支持Rhino Python虚拟环境

## 问题

pandas安装在Rhino Python的虚拟环境中：
```
C:\Users\Administrator\.rhinocode\py39-rh8\site-envs\default-sFQ4Ch2s\pandas\__init__.py
```

需要将所有组件的Python代码更新，添加虚拟环境路径到sys.path。

## ✅ 已更新的组件

- ✅ `ReadCSVComponent.cs` - 已更新
- ✅ `ReadExcelComponent.cs` - 已更新

## 📝 需要更新的其他组件

所有使用Python的组件都需要添加相同的代码。可以：

### 方法1: 手动更新（如果只有几个组件有问题）

找到组件文件中的Python代码部分，在`sys.path.insert(0, r'{mymlPath}')`之后添加：

```python
import site

# 确保Rhino Python的site-packages在路径中
try:
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    rhino_site_envs = r'C:\Users\Administrator\.rhinocode\py39-rh8\site-envs'
    if os.path.exists(rhino_site_envs):
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass
```

### 方法2: 使用Python脚本批量更新

运行 `update_all_components_site_packages.py`（需要修复脚本中的路径问题）

### 方法3: 在Python组件代码中统一处理

更好的方案是在`components/file_io_components.py`等Python文件中统一处理，这样所有组件都能受益。

## 🔧 推荐的解决方案

在Python组件代码的开头统一添加路径处理，这样所有组件都能自动支持Rhino Python虚拟环境。

让我知道您希望使用哪种方法，或者我可以帮您批量更新所有组件。
