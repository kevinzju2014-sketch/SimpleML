# -*- coding: utf-8 -*-
"""
生成SimpleML组件的图标
为每个组件创建24x24像素的PNG图标，背景透明，显示组件缩写
"""

import os
import sys

# 尝试导入PIL
try:
    from PIL import Image, ImageDraw, ImageFont
    HAS_PIL = True
except ImportError:
    HAS_PIL = False
    print("警告: 未安装PIL/Pillow库")
    print("请运行: pip install Pillow")
    sys.exit(1)

# 获取脚本所在目录
script_dir = os.path.dirname(os.path.abspath(__file__))
icons_dir = os.path.join(script_dir, "icons")
os.makedirs(icons_dir, exist_ok=True)

# 组件列表：组件类名 -> 缩写
components = {
    "AboutComponent": "AB",
    "CalculateCorrelationComponent": "COR",
    "CalculateStatisticsComponent": "STA",
    "CreateDatasetComponent": "CDS",
    "DeconstructDatasetComponent": "DDS",
    "DescribeFeaturesComponent": "DES",
    "EvaluateClassificationComponent": "ECL",
    "EvaluateClusteringComponent": "ECU",
    "EvaluateRegressionComponent": "ERG",
    "GetDataSummaryComponent": "SUM",
    "InstallationGuideComponent": "IG",
    "LoadDatasetComponent": "LDS",
    "LoadModelComponent": "LDM",
    "PredictClassifierComponent": "PCL",
    "PredictClusterComponent": "PCU",
    "PredictRegressorComponent": "PGR",
    "ReadCSVComponent": "RCS",
    "ReadExcelComponent": "REX",
    "SaveModelComponent": "SVM",
    "SplitDatasetComponent": "SDS",
    "TrainAgglomerativeClusteringComponent": "TAC",
    "TrainClassifierComponent": "TCL",
    "TrainClusterComponent": "TCU",
    "TrainDBSCANComponent": "TDB",
    "TrainDecisionTreeClassifierComponent": "TDT",
    "TrainKMeansComponent": "TKM",
    "TrainKNNClassifierComponent": "TKN",
    "TrainKNNRegressorComponent": "TKR",
    "TrainLassoRegressionComponent": "TLS",
    "TrainLinearRegressionComponent": "TLR",
    "TrainLogisticRegressionClassifierComponent": "TLG",
    "TrainNaiveBayesClassifierComponent": "TNB",
    "TrainRandomForestClassifierComponent": "TRF",
    "TrainRandomForestRegressorComponent": "TFR",
    "TrainRegressorComponent": "TGR",
    "TrainRidgeRegressionComponent": "TRD",
    "TrainSVMClassifierComponent": "TSV",
    "TrainSVRComponent": "TSR",
    "WriteCSVComponent": "WCS",
    "WriteExcelComponent": "WEX",
}

def create_icon(abbreviation, output_path):
    """
    创建24x24像素的图标
    
    参数:
        abbreviation: 组件缩写（2-3个大写字母）
        output_path: 输出文件路径
    """
    # 创建24x24的透明图像
    size = 24
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 尝试使用系统字体
    font_size = 11
    font = None
    
    # Windows系统字体路径
    font_paths = [
        "C:/Windows/Fonts/arial.ttf",
        "C:/Windows/Fonts/calibri.ttf",
        "C:/Windows/Fonts/msyh.ttc",  # 微软雅黑
    ]
    
    for font_path in font_paths:
        try:
            if os.path.exists(font_path):
                font = ImageFont.truetype(font_path, font_size)
                break
        except:
            continue
    
    # 如果找不到字体，使用默认字体
    if font is None:
        try:
            font = ImageFont.load_default()
        except:
            # 如果默认字体也失败，创建一个简单的字体
            font = None
    
    # 计算文本位置（居中）
    if font:
        bbox = draw.textbbox((0, 0), abbreviation, font=font)
    else:
        # 如果没有字体，估算文本大小
        text_width = len(abbreviation) * 6
        text_height = 12
        bbox = (0, 0, text_width, text_height)
    
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    x = (size - text_width) / 2 - bbox[0]
    y = (size - text_height) / 2 - bbox[1]
    
    # 绘制黑色文本
    if font:
        draw.text((x, y), abbreviation, fill=(0, 0, 0, 255), font=font)
    else:
        # 如果没有字体，使用基本绘制
        draw.text((x, y), abbreviation, fill=(0, 0, 0, 255))
    
    # 保存为PNG
    img.save(output_path, 'PNG')
    print(f"已创建: {os.path.basename(output_path)}")

def main():
    """生成所有组件图标"""
    print("=" * 50)
    print("SimpleML 组件图标生成器")
    print("=" * 50)
    print(f"图标文件夹: {icons_dir}")
    print(f"共 {len(components)} 个组件")
    print()
    
    success_count = 0
    error_count = 0
    
    for component_name, abbreviation in sorted(components.items()):
        try:
            # 生成文件名（去掉Component后缀）
            filename = component_name.replace("Component", "") + ".png"
            output_path = os.path.join(icons_dir, filename)
            
            create_icon(abbreviation, output_path)
            success_count += 1
        except Exception as e:
            print(f"错误: 无法创建 {component_name} 的图标: {str(e)}")
            error_count += 1
    
    print()
    print("=" * 50)
    print(f"完成！成功: {success_count}, 失败: {error_count}")
    print(f"图标保存在: {icons_dir}")
    print("=" * 50)

if __name__ == "__main__":
    main()
