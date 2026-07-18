"""
数据集组件
用于 Grasshopper Python 组件
封装特征数据（X）和标签数据（y）为数据集对象
"""

import sys
import os
import site

# 添加项目路径
current_dir = os.path.dirname(os.path.abspath(__file__))
project_dir = os.path.dirname(current_dir)
if project_dir not in sys.path:
    sys.path.insert(0, project_dir)

# 确保Rhino Python的site-packages在路径中
# 跨平台引导 Rhino / 系统 site-packages
try:
    from core.env_bootstrap import bootstrap_python_paths
    bootstrap_python_paths(project_dir)
except Exception:
    pass

import numpy as np
import warnings

# 抑制所有警告（在Grasshopper环境中，警告可能会干扰用户体验）
warnings.filterwarnings('ignore')


class Dataset:
    """
    数据集类
    封装特征数据（X）和标签数据（y）
    支持Grasshopper的tree结构（列表的列表）
    """
    
    def __init__(self, X, y=None, column_names=None, all_column_names=None):
        """
        初始化数据集
        
        参数:
            X: 特征数据（列表、列表的列表、或numpy数组）
            y: 标签数据（可选，列表或numpy数组）
            column_names: 特征列名列表（可选），用于标识每列的名称
            all_column_names: 所有列的列名列表（可选，包括特征列名和标签列名）
        """
        # 抑制所有警告
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            
            # 处理X：如果是tree结构（列表的列表），展平为2D数组
            if isinstance(X, list):
                # 检查是否是tree结构（列表的列表）
                if len(X) > 0 and isinstance(X[0], list):
                    # Tree结构：每个子列表是一行数据
                    # 确保所有行长度相同
                    max_len = max(len(row) if isinstance(row, list) else 1 for row in X)
                    X_array = []
                    for row in X:
                        if isinstance(row, list):
                            # 如果行长度不足，用0填充（或保持原样，让numpy处理）
                            if len(row) < max_len:
                                row = row + [0.0] * (max_len - len(row))
                            X_array.append(row)
                        else:
                            # 单个值，转换为列表
                            X_array.append([row] + [0.0] * (max_len - 1))
                    # 尝试转换为float数组，如果失败则使用object类型
                    try:
                        self.X = np.array(X_array, dtype=float)
                    except (ValueError, TypeError):
                        # 如果包含非数值类型，使用object类型
                        self.X = np.array(X_array, dtype=object)
                else:
                    # 一维列表，转换为2D数组
                    X_arr = np.array(X)
                    if len(X_arr.shape) == 1:
                        self.X = X_arr.reshape(-1, 1)
                    else:
                        self.X = X_arr
            else:
                self.X = np.array(X)
            
            # 确保X是2D数组
            if len(self.X.shape) == 1:
                self.X = self.X.reshape(-1, 1)
            
            # 处理y
            # 检查y是否有效（不是None且不是空列表）
            if y is not None:
                # 处理空列表的情况：空列表视为没有标签
                if isinstance(y, list) and len(y) == 0:
                    self.y = None
                # 处理numpy数组的情况：空数组视为没有标签
                elif isinstance(y, np.ndarray) and y.size == 0:
                    self.y = None
                else:
                    # 如果y是列表的列表（Tree结构），需要正确处理
                    if isinstance(y, list):
                        if len(y) > 0 and isinstance(y[0], list):
                            # Tree结构：每个子列表是一个分支
                            # 对于标签，每个分支通常只有一个值，但需要处理多个值的情况
                            y_flat = []
                            for branch in y:
                                if isinstance(branch, list):
                                    # 如果分支有多个值，取第一个（或展平）
                                    if len(branch) == 1:
                                        y_flat.append(branch[0])
                                    else:
                                        # 多个值的情况：可能是多标签，展平
                                        y_flat.extend(branch)
                                else:
                                    # 单个值
                                    y_flat.append(branch)
                            self.y = np.array(y_flat).ravel()
                        else:
                            # 一维列表
                            self.y = np.array(y).ravel()
                    else:
                        # numpy数组或其他类型
                        self.y = np.array(y).ravel()
                    
                    # 确保X和y的长度匹配（只有在y有效时才检查）
                    if len(self.X) != len(self.y):
                        raise ValueError(f"X和y的长度不匹配: X有{len(self.X)}个样本，y有{len(self.y)}个样本。请检查输入数据的Tree结构：X应该有与y相同数量的分支（每个分支代表一个样本）。")
            else:
                self.y = None
            
            self.n_samples = len(self.X)
            self.n_features = self.X.shape[1] if len(self.X.shape) > 1 else 1
            
            # 保存列名信息
            self.column_names = column_names
            if self.column_names is not None and len(self.column_names) != self.n_features:
                # 如果列名数量不匹配，静默忽略列名信息（不发出警告）
                # 这样可以避免在预测场景中（只有X和X Names）出现不必要的警告
                self.column_names = None
            
            # 保存所有列的列名信息（包括特征列名和标签列名）
            self.all_column_names = all_column_names
            if self.all_column_names is None and self.column_names is not None:
                # 如果没有提供all_column_names，但有column_names，根据是否有标签构建
                if self.y is not None:
                    # 有标签：特征列名 + 标签列名
                    self.all_column_names = self.column_names.copy()
                    if 'target' not in self.all_column_names:
                        self.all_column_names.append('target')
                else:
                    # 没有标签：只有特征列名
                    self.all_column_names = self.column_names.copy()

            # 预处理器（标准化等），随 Dataset / Model 传递，保证预测一致
            self.preprocessor = None
            self.preprocess_info = None
    
    def get_X(self):
        """获取特征数据"""
        return self.X
    
    def get_y(self):
        """获取标签数据"""
        return self.y
    
    def has_labels(self):
        """检查是否有标签数据"""
        return self.y is not None

    def set_preprocessor(self, preprocessor, info=None):
        """附加已拟合的预处理器，供训练打包与预测复用。"""
        self.preprocessor = preprocessor
        self.preprocess_info = info
    
    def __repr__(self):
        return (
            f"Dataset(n_samples={self.n_samples}, n_features={self.n_features}, "
            f"has_labels={self.has_labels()}, has_preprocessor={self.preprocessor is not None})"
        )


def create_dataset(X, y=None, column_names=None, X_names=None, y_names=None, all_column_names=None):
    """
    创建数据集组件
    
    输入:
        X: 特征数据（列表、列表的列表/tree结构、或numpy数组）
            - 如果是列表的列表：每个子列表是一行数据（tree结构）
            - 如果是一维列表：每个元素是一个特征值（单特征）
            - 如果是二维列表：每行是一个样本，每列是一个特征
        y: 标签数据（可选，列表或numpy数组）
            - 可以留空（None）或传入空列表 []，用于预测场景（没有标签）
            - 如果提供，必须与X的样本数量匹配
        column_names: 特征列名列表（可选），用于标识每列的名称
        X_names: 特征列名列表（可选），column_names的别名，如果column_names为None则使用X_names
        y_names: 标签列名（可选），目前未使用，保留用于未来扩展
        all_column_names: 所有列的列名列表（可选），包括特征列名和标签列名
    
    输出:
        dataset: Dataset对象，包含X和y（如果没有提供y，y将为None）
    
    注意:
        - 预测场景：可以不提供y参数，直接传入X和X_names即可创建数据集用于预测
        - 训练场景：需要提供y参数，用于监督学习
        - 如果column_names和X_names都提供，优先使用column_names
    """
    # 抑制所有警告
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        
        # 处理列名：支持X_names作为column_names的别名
        # 处理空列表和空字符串的情况
        if column_names is None:
            if X_names is not None:
                # 检查X_names是否是空列表或空字符串
                if isinstance(X_names, list):
                    if len(X_names) == 0:
                        column_names = None
                    else:
                        column_names = X_names
                elif isinstance(X_names, str):
                    # 如果是字符串"None"或空字符串，设为None
                    if X_names.lower() == 'none' or X_names == '':
                        column_names = None
                    else:
                        # 尝试解析为列表（如果是从C#传递的字符串格式）
                        try:
                            import ast
                            parsed = ast.literal_eval(X_names)
                            if isinstance(parsed, list):
                                column_names = parsed if len(parsed) > 0 else None
                            else:
                                column_names = None
                        except:
                            column_names = None
                else:
                    column_names = X_names if X_names is not None else None
            else:
                column_names = None
        
        # 最后统一处理：如果column_names是空列表，设为None
        if isinstance(column_names, list) and len(column_names) == 0:
            column_names = None
        
        # 确保X不为None或空
        if X is None:
            raise ValueError("X参数不能为空，必须提供特征数据")
        
        # 处理X为空的情况
        if isinstance(X, list) and len(X) == 0:
            raise ValueError("X不能为空列表，必须提供至少一个样本的特征数据")
        
        if isinstance(X, np.ndarray) and X.size == 0:
            raise ValueError("X不能为空数组，必须提供至少一个样本的特征数据")
        
        # 处理y为None或空的情况（预测场景）
        if y is None:
            try:
                # 确保column_names正确处理（空列表应该被视为None）
                if column_names is not None:
                    if isinstance(column_names, list) and len(column_names) == 0:
                        column_names = None
                
                dataset = Dataset(X, y=None, column_names=column_names, all_column_names=all_column_names)
                return dataset
            except Exception as e:
                # 提供更详细的错误信息
                error_msg = f"创建Dataset失败: {str(e)}\n"
                error_msg += f"X类型: {type(X)}, X形状: {getattr(X, 'shape', 'N/A')}\n"
                error_msg += f"column_names类型: {type(column_names)}, column_names值: {column_names}"
                raise ValueError(error_msg)
        
        # 处理y为空列表的情况
        if isinstance(y, list) and len(y) == 0:
            try:
                dataset = Dataset(X, y=None, column_names=column_names, all_column_names=all_column_names)
                return dataset
            except Exception as e:
                raise ValueError(f"创建Dataset失败: {str(e)}")
        
        # 处理y为numpy空数组的情况
        if isinstance(y, np.ndarray) and y.size == 0:
            try:
                dataset = Dataset(X, y=None, column_names=column_names, all_column_names=all_column_names)
                return dataset
            except Exception as e:
                raise ValueError(f"创建Dataset失败: {str(e)}")
        
        # 正常情况：有有效的y数据
        try:
            dataset = Dataset(X, y, column_names=column_names, all_column_names=all_column_names)
            return dataset
        except Exception as e:
            raise ValueError(f"创建Dataset失败: {str(e)}")


def deconstruct_dataset(dataset):
    """
    解构数据集组件
    
    输入:
        dataset: Dataset对象
    
    输出:
        X: 特征数据（numpy数组）
        y: 标签数据（numpy数组，如果存在）
        has_labels: 是否有标签数据（布尔值）
        column_names: 列名列表（如果存在）
    """
    if not isinstance(dataset, Dataset):
        raise ValueError("输入必须是Dataset对象")
    
    X = dataset.get_X()
    y = dataset.get_y()
    has_labels = dataset.has_labels()
    all_column_names = getattr(dataset, 'all_column_names', None)
    
    if has_labels:
        return X, y, has_labels, all_column_names
    else:
        return X, None, has_labels, all_column_names


def get_dataset_info(dataset):
    """
    获取数据集信息组件
    
    输入:
        dataset: Dataset对象
    
    输出:
        info: 数据集信息字典
            - n_samples: 样本数量
            - n_features: 特征数量
            - has_labels: 是否有标签
            - X_shape: X的形状
            - y_shape: y的形状（如果有）
    """
    if not isinstance(dataset, Dataset):
        raise ValueError("输入必须是Dataset对象")
    
    info = {
        'n_samples': dataset.n_samples,
        'n_features': dataset.n_features,
        'has_labels': dataset.has_labels(),
        'X_shape': dataset.X.shape
    }
    
    if dataset.has_labels():
        info['y_shape'] = dataset.y.shape
    
    return info


def split_dataset(dataset, test_size=0.2, random_state=42):
    """
    划分数据集组件（从dataset创建训练集和测试集dataset）
    
    输入:
        dataset: Dataset对象
        test_size: 测试集比例（0-1之间的浮点数）
        random_state: 随机种子（整数）
    
    输出:
        train_dataset: 训练集Dataset对象
        test_dataset: 测试集Dataset对象
    """
    if not isinstance(dataset, Dataset):
        raise ValueError("输入必须是Dataset对象")
    
    if not dataset.has_labels():
        raise ValueError("数据集必须包含标签才能进行划分")
    
    from core.data_preprocessing import DataPreprocessor
    
    preprocessor = DataPreprocessor()
    X_train, X_test, y_train, y_test = preprocessor.split_data(
        dataset.X, dataset.y, test_size=test_size, random_state=random_state
    )
    
    # 传递列名信息到新的Dataset对象
    train_dataset = Dataset(X_train, y_train, column_names=dataset.column_names, all_column_names=dataset.all_column_names)
    test_dataset = Dataset(X_test, y_test, column_names=dataset.column_names, all_column_names=dataset.all_column_names)

    # 分割后继承同一预处理器（已在完整数据上拟合）
    if getattr(dataset, "preprocessor", None) is not None:
        train_dataset.set_preprocessor(dataset.preprocessor, getattr(dataset, "preprocess_info", None))
        test_dataset.set_preprocessor(dataset.preprocessor, getattr(dataset, "preprocess_info", None))
    
    return train_dataset, test_dataset
