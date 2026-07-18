"""
Grasshopper ML Plugin - Components Module
"""

# 文件IO组件
from .file_io_components import (
    read_csv,
    read_excel,
    dataframe_to_numpy,
    extract_features_and_target,
    get_data_info
)

# 统计分析组件
from .statistics_components import (
    calculate_statistics,
    calculate_correlation,
    get_data_summary,
    describe_features
)

# 数据集组件
from .dataset_components import (
    Dataset,
    create_dataset,
    deconstruct_dataset,
    get_dataset_info,
    split_dataset
)

# 数据预处理组件
from .data_components import (
    prepare_data,
    split_data,
    normalize_data,
    handle_missing_data
)

# 模型训练组件
from .train_components import (
    train_classifier,
    train_regressor,
    train_cluster,
    get_available_algorithms
)

# 模型预测组件
from .predict_components import (
    predict_classifier,
    predict_regressor,
    predict_cluster,
)

# 模型评估组件
from .evaluate_components import (
    evaluate_classification,
    evaluate_regression,
    evaluate_clustering
)

# 模型IO组件
from .model_io_components import (
    save_model,
    save_model_with_manager,
    load_model,
    load_model_as_manager,
    load_model_for_prediction,
    list_models
)

# 易用性增强
from .smart_train import smart_train
from .health_check import run_health_check
from .explain_components import (
    calculate_feature_importance,
    calculate_silhouette,
)

__all__ = [
    'read_csv',
    'read_excel',
    'dataframe_to_numpy',
    'extract_features_and_target',
    'get_data_info',
    'Dataset',
    'create_dataset',
    'deconstruct_dataset',
    'get_dataset_info',
    'split_dataset',
    'calculate_statistics',
    'calculate_correlation',
    'get_data_summary',
    'describe_features',
    'prepare_data',
    'split_data',
    'normalize_data',
    'handle_missing_data',
    'train_classifier',
    'train_regressor',
    'train_cluster',
    'get_available_algorithms',
    'predict_classifier',
    'predict_regressor',
    'predict_cluster',
    'evaluate_classification',
    'evaluate_regression',
    'evaluate_clustering',
    'save_model',
    'save_model_with_manager',
    'load_model',
    'load_model_as_manager',
    'load_model_for_prediction',
    'list_models',
    'smart_train',
    'run_health_check',
    'calculate_feature_importance',
    'calculate_silhouette',
]

from .ux_helpers import predict_auto, evaluate_auto, wizard_recipe, model_card, next_steps_for_model
from .health_check import run_health_check, auto_install_deps

__all__ += [
    'predict_auto', 'evaluate_auto', 'wizard_recipe', 'model_card', 'next_steps_for_model',
    'auto_install_deps',
]
