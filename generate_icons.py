"""
生成components图标脚本
为每个component创建24x24像素、透明背景、黑色文字的PNG图标
"""

from PIL import Image, ImageDraw, ImageFont
import os
import sys

# 获取脚本所在目录
script_dir = os.path.dirname(os.path.abspath(__file__))
# 创建icons文件夹
icons_dir = os.path.join(script_dir, 'icons')
os.makedirs(icons_dir, exist_ok=True)

# 定义components及其缩写
components = {
    # 主components目录
    'data_components': 'DC',
    'dataset_components': 'DS',
    'dataset_loader': 'DL',
    'evaluate_components': 'EC',
    'file_io_components': 'FI',
    'model_io_components': 'MI',
    'predict_components': 'PC',
    'statistics_components': 'SC',
    'train_components': 'TC',
    
    # algorithms目录
    'train_agglomerative_clustering': 'AC',
    'train_dbscan': 'DB',
    'train_decision_tree_classifier': 'DT',
    'train_kmeans': 'KM',
    'train_knn_classifier': 'KC',
    'train_knn_regressor': 'KR',
    'train_lasso_regression': 'LA',
    'train_linear_regression': 'LR',
    'train_logistic_regression_classifier': 'LG',
    'train_naive_bayes_classifier': 'NB',
    'train_random_forest_classifier': 'RF',
    'train_random_forest_regressor': 'RR',
    'train_ridge_regression': 'RG',
    'train_svm_classifier': 'SV',
    'train_svr': 'SR',
}

def create_icon(text, output_path, size=(24, 24)):
    """
    创建图标
    
    参数:
        text: 显示的文本（2-3个大写字母）
        output_path: 输出文件路径
        size: 图标尺寸，默认(24, 24)
    """
    # 创建透明背景的图像
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 尝试使用系统字体，如果失败则使用默认字体
    try:
        # Windows系统字体
        font_size = 14 if len(text) == 2 else 12
        font = ImageFont.truetype("arial.ttf", font_size)
    except:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", font_size)
        except:
            # 使用默认字体
            font_size = 14 if len(text) == 2 else 12
            font = ImageFont.load_default()
    
    # 计算文本位置（居中）
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    x = (size[0] - text_width) / 2 - bbox[0]
    y = (size[1] - text_height) / 2 - bbox[1]
    
    # 绘制黑色文字
    draw.text((x, y), text, fill=(0, 0, 0, 255), font=font)
    
    # 保存为PNG（保持透明背景）
    img.save(output_path, 'PNG')
    print(f"已创建图标: {output_path}")

def main():
    """生成所有图标"""
    print("开始生成图标...")
    print(f"图标保存目录: {icons_dir}")
    print("-" * 50)
    
    for component_name, abbreviation in components.items():
        output_path = os.path.join(icons_dir, f"{component_name}.png")
        create_icon(abbreviation, output_path)
    
    print("-" * 50)
    print(f"完成！共生成 {len(components)} 个图标")

if __name__ == '__main__':
    main()
