# -*- coding: utf-8 -*-
"""Generate ParamTable map entries (name-keyed, stable)."""
import re
from pathlib import Path

ROOT = Path(r"D:\Helio\250928_机器学习课程")
COMP = ROOT / "GHA_Project" / "Components"

# English name -> Chinese name
ZH_NAME = {
    "Dataset": "数据集", "Test Dataset": "测试数据集", "Train Dataset": "训练数据集",
    "Model": "模型", "Model Info": "模型信息", "Model Card": "模型卡片",
    "Algorithm": "算法", "Algorithm Params": "算法参数", "Task": "任务",
    "Readme": "说明", "Predictions": "预测", "Report": "报告", "Verdict": "结论",
    "Next Steps": "下一步", "Explanation": "解释", "N Clusters": "簇数",
    "Test Size": "测试比例", "Random State": "随机种子", "Filepath": "文件路径",
    "Features": "特征", "Target": "目标", "Feature Names": "特征名",
    "Metrics": "指标", "Metrics JSON": "指标JSON", "Confusion Matrix": "混淆矩阵",
    "Geometry": "几何", "Labels": "标签", "Colors": "颜色",
    "Colored Geometry": "着色几何", "Color Map": "颜色映射",
    "Points": "点", "Scatter Points": "散点", "Fit Line": "拟合线",
    "Show Fit Line": "显示拟合线", "Y True": "真实值", "Y Pred": "预测值",
    "Importance": "重要性", "Score": "得分", "Scores": "得分列表",
    "Status": "状态", "Summary": "摘要", "Recipe": "配方",
    "Run": "运行", "AutoFix": "自动修复", "Package Root": "包路径",
    "Python": "Python路径", "Health Status": "体检状态",
    "Plugin Name": "插件名", "Version": "版本", "Author": "作者",
    "Description": "描述", "Contact": "联系方式",
    "Plugin Installation": "插件安装", "Python Requirements": "Python依赖",
    "Installation Steps": "安装步骤", "Troubleshooting": "故障排查",
    "Chinese": "中文", "Is Chinese": "是否中文", "Language": "语言", "Tip": "提示",
    "Dataset Name": "数据集名", "Data": "数据", "Encoding": "编码",
    "Separator": "分隔符", "Header": "表头", "Sheet Name": "工作表",
    "Write": "写入", "Save": "保存", "Index": "索引", "Info": "信息",
    "Saved Path": "保存路径", "Handle Missing": "处理缺失",
    "Missing Strategy": "缺失策略", "Normalize": "标准化",
    "Normalize Method": "标准化方法", "Remove Outliers": "移除异常",
    "X Names": "特征名", "y Names": "标签名", "X": "X", "y": "y",
    "N Estimators": "树数量", "Max Depth": "最大深度",
    "Min Samples Split": "最小分割样本", "Min Samples Leaf": "最小叶子样本",
    "Max Features": "最大特征数", "Bootstrap": "Bootstrap", "Criterion": "划分标准",
    "N Neighbors": "邻居数", "Weights": "权重", "Leaf Size": "叶子大小",
    "Metric": "距离度量", "Kernel": "核函数", "Degree": "度数",
    "Gamma": "Gamma", "Coef0": "Coef0", "C": "C", "Epsilon": "Epsilon",
    "Tol": "容差", "Max Iter": "最大迭代", "Probability": "概率估计",
    "Class Weight": "类别权重", "Penalty": "正则", "Solver": "求解器",
    "Fit Intercept": "拟合截距", "Alpha": "Alpha", "Selection": "选择策略",
    "Fit Prior": "学习先验", "Type": "类型", "Var Smoothing": "方差平滑",
    "Init": "初始化", "N Init": "初始化次数", "Eps": "Eps", "Min Samples": "最小样本",
    "Linkage": "连接方式", "Affinity": "亲和度", "Distance Threshold": "距离阈值",
    "Compute Full Tree": "完整树", "N Jobs": "并行数", "Warm Start": "热启动",
    "Copy X": "复制X", "Method": "方法", "Dimensions": "维度",
    "Explained Variance": "解释方差", "Analysis": "分析",
    "Feature Index": "特征索引", "Statistics": "统计", "Correlation Matrix": "相关矩阵",
    "Max Clusters": "最大簇数", "Compute Distances": "计算距离",
    "Run Health Check": "运行体检", "Hint": "提示", "Reset": "重置",
    "Probabilities": "概率", "Current": "当前", "P": "P",
}

# English name -> Chinese nick shown on grips (only curated; else keep EN nick)
ZH_NICK_BY_NAME = {
    "Dataset": "数据", "Test Dataset": "测试", "Train Dataset": "训练",
    "Model": "模型", "Model Info": "信息", "Model Card": "卡片",
    "Algorithm": "算法", "Algorithm Params": "参数", "Task": "任务",
    "Readme": "说明", "Predictions": "预测", "Report": "报告", "Verdict": "结论",
    "Next Steps": "下一步", "Explanation": "解释", "N Clusters": "簇数",
    "Test Size": "比例", "Random State": "种子", "Filepath": "路径",
    "Features": "X", "Target": "y", "Feature Names": "列名",
    "Metrics": "指标", "Metrics JSON": "JSON", "Confusion Matrix": "混淆",
    "Geometry": "几何", "Labels": "标签", "Colors": "颜色",
    "Colored Geometry": "着色", "Color Map": "色表",
    "Points": "点", "Scatter Points": "散点", "Fit Line": "拟合",
    "Show Fit Line": "拟合线", "Y True": "真值", "Y Pred": "预测",
    "Importance": "重要", "Score": "得分", "Scores": "得分",
    "Status": "状态", "Summary": "摘要", "Recipe": "配方",
    "Run": "运行", "AutoFix": "修复", "Package Root": "路径",
    "Health Status": "体检", "Plugin Name": "名称", "Version": "版本",
    "Author": "作者", "Description": "描述", "Contact": "联系",
    "Plugin Installation": "安装", "Python Requirements": "依赖",
    "Installation Steps": "步骤", "Troubleshooting": "排查",
    "Chinese": "中文", "Is Chinese": "中文?", "Language": "语言", "Tip": "提示",
    "Dataset Name": "名", "Data": "数据", "Write": "写入", "Save": "保存",
    "Saved Path": "路径", "Statistics": "统计", "Correlation Matrix": "相关",
    "Run Health Check": "体检", "Hint": "提示", "Reset": "重置",
    "Probabilities": "概率", "Next Steps": "下一步",
}


def esc(s: str) -> str:
    return s.replace("\\", "\\\\").replace('"', '\\"')


pat = re.compile(r'Add\w+Parameter\(\s*"([^"]+)"\s*,\s*"([^"]+)"')
by_name = {}
for p in COMP.rglob("*.cs"):
    for m in pat.finditer(p.read_text(encoding="utf-8", errors="ignore")):
        by_name[m.group(1)] = m.group(2)

lines = []
keys = set()


def add(key, en_name, zh_name, en_nick, zh_nick):
    if not key or key in keys:
        return
    keys.add(key)
    lines.append(
        f'                {{ "{esc(key)}", new Entry("{esc(en_name)}", "{esc(zh_name)}", '
        f'"{esc(en_nick)}", "{esc(zh_nick)}", "", "") }},'
    )


for name, nick in sorted(by_name.items()):
    zh = ZH_NAME.get(name, name)
    zh_nick = ZH_NICK_BY_NAME.get(name, nick)
    add(name, name, zh, nick, zh_nick)
    add(zh, name, zh, nick, zh_nick)

# Unique-nick aliases only
counts = {}
for n in by_name.values():
    counts[n] = counts.get(n, 0) + 1
for name, nick in sorted(by_name.items()):
    if counts[nick] != 1:
        continue
    zh = ZH_NAME.get(name, name)
    zh_nick = ZH_NICK_BY_NAME.get(name, nick)
    add(nick, name, zh, nick, zh_nick)
    if zh_nick != nick:
        add(zh_nick, name, zh, nick, zh_nick)

extras = [
    ("Train", "Train Dataset", "训练集", "Train", "训练"),
    ("Test", "Test Dataset", "测试集", "Test", "测试"),
    ("训练集", "Train Dataset", "训练集", "Train", "训练"),
    ("测试集", "Test Dataset", "测试集", "Test", "测试"),
    ("TrainDS", "Train Dataset", "训练数据集", "TrainDS", "训练"),
    ("TestDS", "Test Dataset", "测试数据集", "TestDS", "测试"),
]
for row in extras:
    add(*row)

out = ROOT / "GHA_Project" / "Localization" / "_ParamTableGenerated.txt"
out.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("entries", len(lines), "unique names", len(by_name))
