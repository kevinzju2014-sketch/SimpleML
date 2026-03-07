# -*- coding: utf-8 -*-
"""
批量更新所有组件的图标属性
"""

import os
import re

components_dir = os.path.join(os.path.dirname(__file__), 'Components')
icon_pattern = r'protected override System\.Drawing\.Bitmap Icon\s*=>\s*null;'
icon_replacement = r'protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof({component_name}));'

# 获取所有组件文件
component_files = [f for f in os.listdir(components_dir) if f.endswith('.cs') and 'Component' in f]

print(f'找到 {len(component_files)} 个组件文件')
print('-' * 60)

for filename in sorted(component_files):
    filepath = os.path.join(components_dir, filename)
    
    # 读取文件内容
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 提取组件类名
    class_match = re.search(r'public class (\w+Component)', content)
    if not class_match:
        print(f'跳过 {filename}: 未找到组件类名')
        continue
    
    component_name = class_match.group(1)
    
    # 检查是否已经有图标设置
    if 'IconLoader.LoadComponentIcon' in content:
        print(f'跳过 {filename}: 已设置图标')
        continue
    
    # 替换图标属性
    if re.search(icon_pattern, content):
        new_icon_line = icon_replacement.format(component_name=component_name)
        content = re.sub(icon_pattern, new_icon_line, content)
        
        # 检查是否需要添加using语句
        if 'using SimpleML.Core;' not in content:
            # 在namespace之前添加using
            namespace_match = re.search(r'(namespace\s+\S+)', content)
            if namespace_match:
                insert_pos = namespace_match.start()
                content = content[:insert_pos] + 'using SimpleML.Core;\n\n' + content[insert_pos:]
        
        # 写入文件
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print(f'✓ 已更新: {filename} -> {component_name}')
    else:
        print(f'⚠ 跳过 {filename}: 未找到Icon属性')

print('-' * 60)
print('完成！')
