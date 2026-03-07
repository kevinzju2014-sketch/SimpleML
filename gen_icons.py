import os
import sys
from PIL import Image, ImageDraw, ImageFont

# 使用绝对路径
base_dir = r'D:\Helio\250928_机器学习课程\myML'

icons_dir = os.path.join(base_dir, 'icons')
os.makedirs(icons_dir, exist_ok=True)

components = {
    'data_components': 'DC',
    'dataset_components': 'DS',
    'dataset_loader': 'DL',
    'evaluate_components': 'EC',
    'file_io_components': 'FI',
    'model_io_components': 'MI',
    'predict_components': 'PC',
    'statistics_components': 'SC',
    'train_components': 'TC',
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

print('开始生成图标...')
print(f'图标保存目录: {icons_dir}')

for name, abbr in components.items():
    img = Image.new('RGBA', (24, 24), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    font_size = 14 if len(abbr) == 2 else 12
    try:
        font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', font_size)
    except:
        try:
            font = ImageFont.truetype('arial.ttf', font_size)
        except:
            font = ImageFont.load_default()
    
    bbox = draw.textbbox((0, 0), abbr, font=font)
    x = (24 - (bbox[2] - bbox[0])) / 2 - bbox[0]
    y = (24 - (bbox[3] - bbox[1])) / 2 - bbox[1]
    
    draw.text((x, y), abbr, fill=(0, 0, 0, 255), font=font)
    
    output_path = os.path.join(icons_dir, f'{name}.png')
    img.save(output_path, 'PNG')
    print(f'已创建: {name}.png')

print(f'完成！共生成 {len(components)} 个图标')
