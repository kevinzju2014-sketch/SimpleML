"""
分析所有Components的输出类型，找出应该改为Tree结构的输出
"""
import re
from pathlib import Path

components_dir = Path(__file__).parent / "Components"

# 应该使用Tree结构的输出模式
tree_output_patterns = [
    "数据", "Data", "X", "y", "预测", "Predict", "结果", "Result", 
    "特征", "Feature", "标签", "Label", "值", "Value", "数组", "Array"
]

# 应该保持Text的输出模式
text_output_patterns = [
    "JSON", "信息", "Info", "形状", "Shape", "列名", "Column", 
    "指标", "Metric", "建议", "Suggestion", "Readme", "readme"
]

results = []

for file_path in sorted(components_dir.glob("*.cs")):
    content = file_path.read_text(encoding='utf-8')
    
    # 提取组件名称
    component_match = re.search(r'class (\w+)Component', content)
    component_name = component_match.group(1) if component_match else "Unknown"
    
    # 查找RegisterOutputParams
    output_match = re.search(r'RegisterOutputParams.*?\{([^}]+)\}', content, re.DOTALL)
    if not output_match:
        continue
    
    outputs = []
    output_section = output_match.group(1)
    
    # 查找所有AddTextParameter, AddGenericParameter等
    param_matches = re.finditer(
        r'pManager\.Add(Text|Generic|Number|Integer|Boolean)Parameter\("([^"]+)",\s*"([^"]*)",\s*"([^"]*)",\s*GH_ParamAccess\.(\w+)',
        output_section
    )
    
    for match in param_matches:
        param_type = match.group(1)  # Text, Generic, Number等
        param_name = match.group(2)
        param_nickname = match.group(3)
        param_desc = match.group(4)
        access_type = match.group(5)  # item, tree, list
        
        should_be_tree = False
        reason = ""
        
        # 检查是否应该使用Tree
        for pattern in tree_output_patterns:
            if pattern.lower() in param_name.lower() or pattern.lower() in param_desc.lower():
                should_be_tree = True
                reason = f"包含'{pattern}'"
                break
        
        # 检查是否应该保持Text
        for pattern in text_output_patterns:
            if pattern.lower() in param_name.lower() or pattern.lower() in param_desc.lower():
                should_be_tree = False
                reason = f"包含'{pattern}'，应保持Text"
                break
        
        outputs.append({
            'name': param_name,
            'type': param_type,
            'access': access_type,
            'desc': param_desc,
            'should_be_tree': should_be_tree and access_type != 'tree',
            'reason': reason
        })
    
    if outputs:
        results.append({
            'file': file_path.name,
            'component': component_name,
            'outputs': outputs
        })

# 输出结果
print("=" * 80)
print("组件输出分析报告")
print("=" * 80)
print()

for result in results:
    print(f"\n组件: {result['component']} ({result['file']})")
    print("-" * 80)
    
    for output in result['outputs']:
        status = "✓" if output['access'] == 'tree' else ("⚠" if output['should_be_tree'] else "  ")
        print(f"  {status} {output['name']} ({output['type']}, {output['access']})")
        print(f"     描述: {output['desc']}")
        if output['should_be_tree']:
            print(f"     ⚠ 建议改为Tree结构: {output['reason']}")
        print()

print("\n" + "=" * 80)
print("总结")
print("=" * 80)

total_should_change = sum(len([o for o in r['outputs'] if o['should_be_tree']]) for r in results)
print(f"建议修改的输出数量: {total_should_change}")
