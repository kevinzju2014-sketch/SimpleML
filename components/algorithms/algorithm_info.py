"""
算法信息模块
提供每个算法的详细信息：名称、适用范围、参数范围、评估结果预期
"""

ALGORITHM_INFO = {
    # 分类算法
    'random_forest_classifier': {
        'name': '随机森林分类器',
        'name_en': 'Random Forest Classifier',
        'applicability': '适用于大多数分类问题，特别是特征较多、样本量较大的情况。对过拟合有较好的抵抗能力。',
        'parameters': {
            'n_estimators': {
                'description': '树的数量',
                'range': '10-500',
                'default': 100,
                'recommended': '100-200',
                'note': '树越多模型越稳定，但计算时间也越长'
            },
            'max_depth': {
                'description': '树的最大深度',
                'range': '1-50或None',
                'default': None,
                'recommended': '10-20或None',
                'note': 'None表示不限制深度，可能过拟合'
            },
            'min_samples_split': {
                'description': '内部节点再划分所需最小样本数',
                'range': '2-20',
                'default': 2,
                'recommended': '2-10',
                'note': '值越大，树越简单，越不容易过拟合'
            },
            'min_samples_leaf': {
                'description': '叶子节点最少样本数',
                'range': '1-10',
                'default': 1,
                'recommended': '1-5',
                'note': '值越大，树越简单'
            },
            'max_features': {
                'description': '寻找最佳分割时考虑的特征数量',
                'range': "'sqrt', 'log2', None, 或整数",
                'default': 'sqrt',
                'recommended': "'sqrt'或'log2'",
                'note': 'sqrt通常效果最好'
            }
        },
        'expected_results': {
            'accuracy': '通常70-95%',
            'precision': '通常70-95%',
            'recall': '通常70-95%',
            'f1_score': '通常70-95%',
            'note': '在平衡数据集上表现优秀，对特征缩放不敏感'
        }
    },
    
    'svm_classifier': {
        'name': '支持向量机分类器',
        'name_en': 'Support Vector Machine Classifier',
        'applicability': '适用于中小规模数据集，特别是高维数据。对特征缩放敏感，需要标准化。',
        'parameters': {
            'C': {
                'description': '正则化参数',
                'range': '0.01-1000',
                'default': 1.0,
                'recommended': '0.1-100',
                'note': '值越大对误分类惩罚越大，可能过拟合'
            },
            'kernel': {
                'description': '核函数类型',
                'range': "'linear', 'poly', 'rbf', 'sigmoid'",
                'default': 'rbf',
                'recommended': "'rbf'（最常用）",
                'note': 'rbf适用于大多数非线性问题'
            },
            'gamma': {
                'description': '核函数系数',
                'range': "'scale', 'auto', 或0.0001-10",
                'default': 'scale',
                'recommended': "'scale'或'auto'",
                'note': '值越大，决策边界越复杂'
            },
            'degree': {
                'description': '多项式核的度数',
                'range': '1-10',
                'default': 3,
                'recommended': '2-4',
                'note': '仅当kernel=poly时有效'
            }
        },
        'expected_results': {
            'accuracy': '通常60-90%',
            'precision': '通常60-90%',
            'recall': '通常60-90%',
            'f1_score': '通常60-90%',
            'note': '在小数据集上表现优秀，但计算复杂度高'
        }
    },
    
    'logistic_regression_classifier': {
        'name': '逻辑回归分类器',
        'name_en': 'Logistic Regression Classifier',
        'applicability': '适用于线性可分的分类问题，计算速度快，可解释性强。适合作为基线模型。',
        'parameters': {
            'C': {
                'description': '正则化强度的倒数',
                'range': '0.001-1000',
                'default': 1.0,
                'recommended': '0.1-10',
                'note': '值越小正则化越强'
            },
            'penalty': {
                'description': '正则化类型',
                'range': "'l1', 'l2', 'elasticnet'",
                'default': 'l2',
                'recommended': "'l2'（最常用）",
                'note': 'l1可用于特征选择'
            },
            'solver': {
                'description': '优化算法',
                'range': "'lbfgs', 'liblinear', 'newton-cg', 'sag', 'saga'",
                'default': 'lbfgs',
                'recommended': "'lbfgs'（小数据集）或'sag'（大数据集）",
                'note': '根据数据规模选择'
            },
            'max_iter': {
                'description': '最大迭代次数',
                'range': '50-10000',
                'default': 100,
                'recommended': '100-1000',
                'note': '如果收敛慢，可以增加'
            }
        },
        'expected_results': {
            'accuracy': '通常60-85%',
            'precision': '通常60-85%',
            'recall': '通常60-85%',
            'f1_score': '通常60-85%',
            'note': '在线性可分问题上表现良好，速度快'
        }
    },
    
    'knn_classifier': {
        'name': 'K近邻分类器',
        'name_en': 'K-Nearest Neighbors Classifier',
        'applicability': '适用于小到中等规模数据集，对局部模式敏感。计算速度随数据量增长而变慢。',
        'parameters': {
            'n_neighbors': {
                'description': '邻居数量',
                'range': '1-50',
                'default': 5,
                'recommended': '3-15（奇数）',
                'note': '值越大，决策边界越平滑'
            },
            'weights': {
                'description': '权重函数',
                'range': "'uniform', 'distance'",
                'default': 'uniform',
                'recommended': "'distance'（通常更好）",
                'note': 'distance给近邻更大权重'
            },
            'algorithm': {
                'description': '计算最近邻的算法',
                'range': "'auto', 'ball_tree', 'kd_tree', 'brute'",
                'default': 'auto',
                'recommended': "'auto'（自动选择）",
                'note': 'auto会根据数据自动选择最优算法'
            },
            'p': {
                'description': '距离度量参数',
                'range': '1-2',
                'default': 2,
                'recommended': '2（欧氏距离）',
                'note': '1为曼哈顿距离，2为欧氏距离'
            }
        },
        'expected_results': {
            'accuracy': '通常65-90%',
            'precision': '通常65-90%',
            'recall': '通常65-90%',
            'f1_score': '通常65-90%',
            'note': '对特征缩放敏感，需要标准化'
        }
    },
    
    # 回归算法
    'random_forest_regressor': {
        'name': '随机森林回归器',
        'name_en': 'Random Forest Regressor',
        'applicability': '适用于大多数回归问题，特别是非线性关系。对过拟合有较好的抵抗能力，适合建筑能源预测等场景。',
        'parameters': {
            'n_estimators': {
                'description': '树的数量',
                'range': '10-500',
                'default': 100,
                'recommended': '100-200',
                'note': '树越多模型越稳定，但计算时间也越长'
            },
            'max_depth': {
                'description': '树的最大深度',
                'range': '1-50或None',
                'default': None,
                'recommended': '10-20或None',
                'note': 'None表示不限制深度'
            },
            'min_samples_split': {
                'description': '内部节点再划分所需最小样本数',
                'range': '2-20',
                'default': 2,
                'recommended': '2-10',
                'note': '值越大，树越简单'
            },
            'min_samples_leaf': {
                'description': '叶子节点最少样本数',
                'range': '1-10',
                'default': 1,
                'recommended': '1-5',
                'note': '值越大，树越简单'
            },
            'max_features': {
                'description': '寻找最佳分割时考虑的特征数量',
                'range': "'sqrt', 'log2', None, 或整数",
                'default': 'sqrt',
                'recommended': "'sqrt'或'log2'",
                'note': 'sqrt通常效果最好'
            }
        },
        'expected_results': {
            'r2_score': '通常0.7-0.95',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '在非线性问题上表现优秀，对特征缩放不敏感'
        }
    },
    
    'svr': {
        'name': '支持向量回归器',
        'name_en': 'Support Vector Regressor',
        'applicability': '适用于中小规模回归问题，特别是非线性关系。对特征缩放敏感，需要标准化。',
        'parameters': {
            'C': {
                'description': '正则化参数',
                'range': '0.01-1000',
                'default': 1.0,
                'recommended': '0.1-100',
                'note': '值越大对误差惩罚越大'
            },
            'kernel': {
                'description': '核函数类型',
                'range': "'linear', 'poly', 'rbf', 'sigmoid'",
                'default': 'rbf',
                'recommended': "'rbf'（最常用）",
                'note': 'rbf适用于大多数非线性问题'
            },
            'gamma': {
                'description': '核函数系数',
                'range': "'scale', 'auto', 或0.0001-10",
                'default': 'scale',
                'recommended': "'scale'或'auto'",
                'note': '值越大，决策边界越复杂'
            },
            'epsilon': {
                'description': '不敏感损失函数的参数',
                'range': '0.01-1.0',
                'default': 0.1,
                'recommended': '0.1-0.5',
                'note': '值越大允许的误差越大'
            }
        },
        'expected_results': {
            'r2_score': '通常0.6-0.9',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '在小数据集上表现优秀，但计算复杂度高'
        }
    },
    
    'linear_regression': {
        'name': '线性回归器',
        'name_en': 'Linear Regressor',
        'applicability': '适用于线性关系明显的回归问题，计算速度快，可解释性强。适合作为基线模型。',
        'parameters': {
            'fit_intercept': {
                'description': '是否计算截距',
                'range': 'True或False',
                'default': True,
                'recommended': 'True',
                'note': '通常应该计算截距'
            },
            'normalize': {
                'description': '是否标准化特征',
                'range': 'True或False',
                'default': False,
                'recommended': 'False（在预处理阶段标准化）',
                'note': '建议在数据预处理阶段进行标准化'
            }
        },
        'expected_results': {
            'r2_score': '通常0.5-0.85（线性关系）',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '在线性关系上表现良好，速度快'
        }
    },
    
    'ridge_regression': {
        'name': '岭回归器',
        'name_en': 'Ridge Regressor',
        'applicability': '适用于多重共线性问题，L2正则化防止过拟合。适合特征较多的情况。',
        'parameters': {
            'alpha': {
                'description': '正则化强度',
                'range': '0.01-100',
                'default': 1.0,
                'recommended': '0.1-10',
                'note': '值越大正则化越强'
            },
            'fit_intercept': {
                'description': '是否计算截距',
                'range': 'True或False',
                'default': True,
                'recommended': 'True',
                'note': '通常应该计算截距'
            },
            'max_iter': {
                'description': '最大迭代次数',
                'range': '100-10000',
                'default': None,
                'recommended': '1000-5000',
                'note': '如果收敛慢，可以增加'
            }
        },
        'expected_results': {
            'r2_score': '通常0.6-0.9',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '在多重共线性问题上表现优秀'
        }
    },
    
    'lasso_regression': {
        'name': 'Lasso回归器',
        'name_en': 'Lasso Regressor',
        'applicability': '适用于特征选择，L1正则化可以将不重要的特征系数变为0。适合特征很多的情况。',
        'parameters': {
            'alpha': {
                'description': '正则化强度',
                'range': '0.001-10',
                'default': 1.0,
                'recommended': '0.1-1.0',
                'note': '值越大，特征选择越严格'
            },
            'fit_intercept': {
                'description': '是否计算截距',
                'range': 'True或False',
                'default': True,
                'recommended': 'True',
                'note': '通常应该计算截距'
            },
            'max_iter': {
                'description': '最大迭代次数',
                'range': '100-10000',
                'default': 1000,
                'recommended': '1000-5000',
                'note': '如果收敛慢，可以增加'
            }
        },
        'expected_results': {
            'r2_score': '通常0.6-0.9',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '可以自动进行特征选择，适合高维数据'
        }
    },
    
    'knn_regressor': {
        'name': 'K近邻回归器',
        'name_en': 'K-Nearest Neighbors Regressor',
        'applicability': '适用于局部模式明显的回归问题。计算速度随数据量增长而变慢。',
        'parameters': {
            'n_neighbors': {
                'description': '邻居数量',
                'range': '1-50',
                'default': 5,
                'recommended': '3-15（奇数）',
                'note': '值越大，预测越平滑'
            },
            'weights': {
                'description': '权重函数',
                'range': "'uniform', 'distance'",
                'default': 'uniform',
                'recommended': "'distance'（通常更好）",
                'note': 'distance给近邻更大权重'
            },
            'algorithm': {
                'description': '计算最近邻的算法',
                'range': "'auto', 'ball_tree', 'kd_tree', 'brute'",
                'default': 'auto',
                'recommended': "'auto'（自动选择）",
                'note': 'auto会根据数据自动选择最优算法'
            },
            'p': {
                'description': '距离度量参数',
                'range': '1-2',
                'default': 2,
                'recommended': '2（欧氏距离）',
                'note': '1为曼哈顿距离，2为欧氏距离'
            }
        },
        'expected_results': {
            'r2_score': '通常0.6-0.9',
            'rmse': '取决于目标变量量级',
            'mae': '取决于目标变量量级',
            'note': '对特征缩放敏感，需要标准化'
        }
    },
    
    # 聚类算法
    'kmeans': {
        'name': 'K-Means聚类',
        'name_en': 'K-Means Clustering',
        'applicability': '适用于球形聚类，需要预先指定聚类数量。计算速度快，适合大规模数据。',
        'parameters': {
            'n_clusters': {
                'description': '聚类数量',
                'range': '2-100',
                'default': 3,
                'recommended': '2-20',
                'note': '可以使用肘部法则确定最优K值'
            },
            'init': {
                'description': '初始化方法',
                'range': "'k-means++', 'random', 或数组",
                'default': 'k-means++',
                'recommended': "'k-means++'（通常更好）",
                'note': 'k-means++通常收敛更快'
            },
            'n_init': {
                'description': '不同初始化运行次数',
                'range': '1-50',
                'default': 10,
                'recommended': '10-20',
                'note': '值越大，结果越稳定'
            },
            'max_iter': {
                'description': '最大迭代次数',
                'range': '100-1000',
                'default': 300,
                'recommended': '300-500',
                'note': '通常300足够'
            }
        },
        'expected_results': {
            'n_clusters': '等于设定的K值',
            'note': '适用于球形聚类，对初始值敏感'
        }
    },
    
    'dbscan': {
        'name': 'DBSCAN聚类',
        'name_en': 'DBSCAN Clustering',
        'applicability': '适用于任意形状的聚类，不需要预先指定聚类数量。可以发现噪声点。',
        'parameters': {
            'eps': {
                'description': '邻域半径',
                'range': '0.1-10.0',
                'default': 0.5,
                'recommended': '0.3-2.0',
                'note': '需要根据数据尺度调整，建议先标准化数据'
            },
            'min_samples': {
                'description': '形成核心点所需的最小样本数',
                'range': '2-20',
                'default': 5,
                'recommended': '3-10',
                'note': '值越大，对噪声越不敏感，但可能遗漏小聚类'
            },
            'metric': {
                'description': '距离度量方法',
                'range': "'euclidean', 'manhattan', 'cosine'等",
                'default': 'euclidean',
                'recommended': "'euclidean'（最常用）",
                'note': 'euclidean适用于大多数情况'
            }
        },
        'expected_results': {
            'n_clusters': '由eps和min_samples决定',
            'note': '可以发现任意形状的聚类，噪声点标记为-1'
        }
    },
    
    'agglomerative_clustering': {
        'name': '层次聚类',
        'name_en': 'Agglomerative Clustering',
        'applicability': '适用于需要层次结构的聚类，可以生成树状图。计算复杂度较高。',
        'parameters': {
            'n_clusters': {
                'description': '聚类数量',
                'range': '2-100',
                'default': 3,
                'recommended': '2-20',
                'note': '可以使用树状图确定最优K值'
            },
            'linkage': {
                'description': '链接准则',
                'range': "'ward', 'complete', 'average', 'single'",
                'default': 'ward',
                'recommended': "'ward'（最常用）",
                'note': 'ward最小化类内方差，效果通常最好'
            },
            'affinity': {
                'description': '距离度量或亲和度',
                'range': "'euclidean', 'l1', 'l2', 'manhattan', 'cosine'",
                'default': 'euclidean',
                'recommended': "'euclidean'（与ward配合）",
                'note': 'ward只能与euclidean配合使用'
            }
        },
        'expected_results': {
            'n_clusters': '等于设定的K值',
            'note': '可以生成层次结构，适合需要树状图的场景'
        }
    }
}


def get_algorithm_info(algorithm_key):
    """
    获取算法信息
    
    参数:
        algorithm_key: 算法键名（如'random_forest_regressor'）
    
    返回:
        算法信息字典，如果不存在返回None
    """
    return ALGORITHM_INFO.get(algorithm_key)


def format_algorithm_readme(algorithm_key):
    """
    格式化算法信息为可读的文本
    
    参数:
        algorithm_key: 算法键名
    
    返回:
        格式化的文本字符串
    """
    info = get_algorithm_info(algorithm_key)
    if info is None:
        return f"未找到算法信息: {algorithm_key}"
    
    readme = []
    readme.append("=" * 60)
    readme.append(f"算法名称: {info['name']} ({info['name_en']})")
    readme.append("=" * 60)
    readme.append("")
    readme.append("适用范围:")
    readme.append(f"  {info['applicability']}")
    readme.append("")
    readme.append("参数说明:")
    readme.append("-" * 60)
    
    for param_name, param_info in info['parameters'].items():
        readme.append(f"  {param_name}:")
        readme.append(f"    描述: {param_info['description']}")
        readme.append(f"    取值范围: {param_info['range']}")
        readme.append(f"    默认值: {param_info['default']}")
        readme.append(f"    推荐值: {param_info['recommended']}")
        readme.append(f"    备注: {param_info['note']}")
        readme.append("")
    
    readme.append("预计评估结果:")
    readme.append("-" * 60)
    for key, value in info['expected_results'].items():
        readme.append(f"  {key}: {value}")
    
    return "\n".join(readme)
