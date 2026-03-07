# -*- coding: utf-8 -*-
"""
将图标集成到GHA项目的脚本
1. 复制图标文件到GHA_Project/Icons目录
2. 创建图标资源映射
"""

import os
import shutil

# 路径设置
project_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
icons_src = os.path.join(project_root, 'icons')
icons_dst = os.path.join(os.path.dirname(__file__), 'Icons')

# 创建目标目录
os.makedirs(icons_dst, exist_ok=True)

# 复制图标文件
png_files = [f for f in os.listdir(icons_src) if f.endswith('.png')]
for png_file in png_files:
    src_path = os.path.join(icons_src, png_file)
    dst_path = os.path.join(icons_dst, png_file)
    shutil.copy2(src_path, dst_path)
    print(f'已复制: {png_file}')

print(f'\n完成！共复制 {len(png_files)} 个图标文件到 {icons_dst}')
