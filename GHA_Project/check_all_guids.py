"""
检查所有组件的GUID，找出重复项
"""
import re
import os
from collections import defaultdict
from pathlib import Path

components_dir = Path(__file__).parent / "Components"

guid_map = defaultdict(list)

# 读取所有.cs文件
for file_path in components_dir.glob("*.cs"):
    content = file_path.read_text(encoding='utf-8')
    
    # 查找GUID
    match = re.search(r'new Guid\(["\']([^"\']+)["\']\)', content)
    if match:
        guid = match.group(1)
        guid_map[guid].append(file_path.name)

# 找出重复的GUID
duplicates = {guid: files for guid, files in guid_map.items() if len(files) > 1}

if duplicates:
    print("发现重复的GUID:")
    print("=" * 80)
    for guid, files in duplicates.items():
        print(f"\nGUID: {guid}")
        print(f"冲突的文件数: {len(files)}")
        for file in files:
            print(f"  - {file}")
else:
    print("✅ 没有发现重复的GUID")

# 统计
print("\n" + "=" * 80)
print(f"总组件数: {len(guid_map)}")
print(f"唯一GUID数: {len(set(guid_map.keys()))}")
print(f"重复GUID数: {len(duplicates)}")
