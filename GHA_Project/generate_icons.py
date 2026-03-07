"""
生成SimpleML组件的图标
为每个组件创建24x24像素的PNG图标，背景透明，显示组件缩写
"""

import os
from PIL import Image, ImageDraw, ImageFont

# 创建icons文件夹
icons_dir = os.path.join(os.path.dirname(__file__), "icons")
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
    
    # 尝试使用系统字体，如果失败则使用默认字体
    try:
        # Windows系统字体
        font_size = 12
        try:
            font = ImageFont.truetype("arial.ttf", font_size)
        except:
            try:
                font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", font_size)
            except:
                font = ImageFont.load_default()
    except:
        font = ImageFont.load_default()
    
    # 计算文本位置（居中）
    bbox = draw.textbbox((0, 0), abbreviation, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    x = (size - text_width) / 2 - bbox[0]
    y = (size - text_height) / 2 - bbox[1]
    
    # 绘制黑色文本
    draw.text((x, y), abbreviation, fill=(0, 0, 0, 255), font=font)
    
    # 保存为PNG
    img.save(output_path, 'PNG')
    print(f"已创建: {output_path}")

def main():
    """生成所有组件图标"""
    print("开始生成组件图标...")
    print(f"图标文件夹: {icons_dir}")
    print()
    
    for component_name, abbreviation in components.items():
        # 生成文件名（去掉Component后缀）
        filename = component_name.replace("Component", "") + ".png"
        output_path = os.path.join(icons_dir, filename)
        
        create_icon(abbreviation, output_path)
    
    print()
    print(f"完成！共生成 {len(components)} 个图标")
    print(f"图标保存在: {icons_dir}")

if __name__ == "__main__":
    main()
