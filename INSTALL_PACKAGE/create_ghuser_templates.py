"""
创建Grasshopper用户对象模板
用于生成.ghuser文件的基础模板
"""

import os
import json
from pathlib import Path

# 用户对象模板结构
GHUSER_TEMPLATE = {
    "TypeName": "Python Script",
    "NickName": "SimpleML",
    "Category": "SimpleML",
    "SubCategory": "",
    "Description": "",
    "Icon": "",
    "Inputs": [],
    "Outputs": [],
    "Code": ""
}

# 电池定义
BATTERIES = {
    "Data Input": {
        "Read CSV": {
            "category": "Data Input",
            "description": "读取CSV文件",
            "inputs": [
                {"name": "filepath", "type": "String", "description": "CSV文件路径"},
                {"name": "encoding", "type": "String", "description": "文件编码，默认'utf-8'"},
                {"name": "header", "type": "Integer", "description": "表头行号，默认0"},
                {"name": "sep", "type": "String", "description": "分隔符，默认','"}
            ],
            "outputs": [
                {"name": "data", "type": "DataFrame", "description": "读取的数据"},
                {"name": "columns", "type": "List", "description": "列名列表"},
                {"name": "shape", "type": "Tuple", "description": "数据形状"}
            ],
            "code_template": """
import sys
sys.path.append(r'{simpleml_path}')

from components import read_csv

data, columns, shape = read_csv(
    filepath,
    encoding=encoding if encoding else 'utf-8',
    header=header if header is not None else 0,
    sep=sep if sep else ','
)

a = data
b = columns
c = shape
"""
        },
        "Read Excel": {
            "category": "Data Input",
            "description": "读取Excel文件",
            "inputs": [
                {"name": "filepath", "type": "String", "description": "Excel文件路径"},
                {"name": "sheet_name", "type": "String/Integer", "description": "工作表名称或索引"},
                {"name": "header", "type": "Integer", "description": "表头行号"}
            ],
            "outputs": [
                {"name": "data", "type": "DataFrame", "description": "读取的数据"},
                {"name": "columns", "type": "List", "description": "列名列表"},
                {"name": "shape", "type": "Tuple", "description": "数据形状"}
            ],
            "code_template": """
import sys
sys.path.append(r'{simpleml_path}')

from components import read_excel

data, columns, shape = read_excel(
    filepath,
    sheet_name=sheet_name if sheet_name else 0,
    header=header if header else 0
)

a = data
b = columns
c = shape
"""
        }
    }
    # 可以继续添加其他电池...
}

def create_ghuser_file(battery_name, battery_info, simpleml_path, output_dir):
    """创建.ghuser文件"""
    
    # 生成代码
    code = battery_info["code_template"].format(simpleml_path=simpleml_path)
    
    # 创建用户对象结构
    ghuser = {
        "TypeName": "Python Script",
        "NickName": f"SimpleML - {battery_name}",
        "Category": f"SimpleML - {battery_info['category']}",
        "SubCategory": "",
        "Description": battery_info["description"],
        "Icon": "",
        "Inputs": [
            {
                "Name": inp["name"],
                "NickName": inp["name"],
                "Description": inp["description"],
                "Type": inp["type"]
            }
            for inp in battery_info["inputs"]
        ],
        "Outputs": [
            {
                "Name": out["name"],
                "NickName": out["name"],
                "Description": out["description"],
                "Type": out["type"]
            }
            for out in battery_info["outputs"]
        ],
        "Code": code
    }
    
    # 保存为JSON文件（.ghuser文件实际上是XML格式，这里提供JSON模板）
    output_file = output_dir / f"{battery_name.replace(' ', '_')}.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(ghuser, f, indent=2, ensure_ascii=False)
    
    print(f"Created template: {output_file}")
    return output_file

def main():
    """主函数"""
    # 设置路径
    script_dir = Path(__file__).parent
    output_dir = script_dir / "UserObjects"
    output_dir.mkdir(exist_ok=True)
    
    # SimpleML路径（需要用户配置）
    simpleml_path = input("Enter SimpleML installation path (or press Enter for default): ").strip()
    if not simpleml_path:
        # 默认路径
        simpleml_path = r"C:\Users\%USERNAME%\AppData\Roaming\Grasshopper\UserObjects\SimpleML\myML"
    
    print(f"\nCreating user object templates...")
    print(f"SimpleML path: {simpleml_path}")
    print(f"Output directory: {output_dir}\n")
    
    # 创建所有电池的用户对象模板
    for category, batteries in BATTERIES.items():
        for battery_name, battery_info in batteries.items():
            create_ghuser_file(battery_name, battery_info, simpleml_path, output_dir)
    
    print(f"\nTemplates created in: {output_dir}")
    print("\nNote: These are JSON templates. You need to convert them to .ghuser XML format")
    print("or create them manually in Grasshopper using the User Objects feature.")

if __name__ == "__main__":
    main()
