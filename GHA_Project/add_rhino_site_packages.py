"""
为所有组件添加Rhino Python虚拟环境路径支持
"""
import re
import os
from pathlib import Path

components_dir = Path(__file__).parent / "Components"

# 要添加的代码片段
rhino_site_packages_code = """
import site

# 确保Rhino Python的site-packages在路径中
# Rhino Python使用虚拟环境，需要确保site-packages可用
try:
    # 获取site-packages路径
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    # 尝试添加Rhino Python的site-envs路径
    rhino_site_envs = r'C:\\Users\\Administrator\\.rhinocode\\py39-rh8\\site-envs'
    if os.path.exists(rhino_site_envs):
        # 查找所有虚拟环境
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                # 添加虚拟环境的site-packages
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass  # 如果添加路径失败，继续执行
"""

# 查找所有需要更新的组件文件
for file_path in components_dir.glob("*.cs"):
    content = file_path.read_text(encoding='utf-8')
    
    # 检查是否已经有site import
    if 'import site' in content:
        print(f"跳过 {file_path.name} (已包含site处理)")
        continue
    
    # 检查是否有sys.path.insert
    if 'sys.path.insert(0, r\'' in content or 'sys.path.insert(0, r"' in content:
        # 查找sys.path.insert的位置
        match = re.search(r'(import sys\s+import os\s+import json\s+.*?sys\.path\.insert\(0, r[\'"]\{mymlPath\}[\'"])', content, re.DOTALL)
        if match:
            # 在sys.path.insert之后添加site处理代码
            old_code = match.group(1)
            # 在sys.path.insert之后添加
            new_code = old_code + rhino_site_packages_code.replace('\n', '\n                ')
            
            content = content.replace(old_code, new_code)
            file_path.write_text(content, encoding='utf-8')
            print(f"已更新 {file_path.name}")
        else:
            print(f"未找到匹配模式 {file_path.name}")

print("\n完成！")
