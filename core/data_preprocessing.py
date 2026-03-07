"""
数据预处理模块
提供数据清洗、标准化、归一化等功能
"""

import numpy as np
from sklearn.preprocessing import StandardScaler, MinMaxScaler, RobustScaler
from sklearn.model_selection import train_test_split


class DataPreprocessor:
    """数据预处理器"""
    
    def __init__(self):
        self.scaler = None
        self.scaler_type = None
    
    def normalize(self, X, method='standard'):
        """
        数据标准化/归一化
        
        参数:
            X: 输入数据
            method: 'standard' (标准化), 'minmax' (最小最大归一化), 'robust' (鲁棒标准化)
        
        返回:
            标准化后的数据
        """
        X = np.array(X)
        
        if method == 'standard':
            self.scaler = StandardScaler()
        elif method == 'minmax':
            self.scaler = MinMaxScaler()
        elif method == 'robust':
            self.scaler = RobustScaler()
        else:
            raise ValueError(f"不支持的标准化方法: {method}")
        
        self.scaler_type = method
        X_scaled = self.scaler.fit_transform(X)
        
        return X_scaled
    
    def transform(self, X):
        """
        使用已拟合的标准化器转换数据
        
        参数:
            X: 输入数据
        
        返回:
            转换后的数据
        """
        if self.scaler is None:
            raise ValueError("标准化器尚未拟合，请先调用 normalize 方法")
        
        X = np.array(X)
        return self.scaler.transform(X)
    
    def split_data(self, X, y, test_size=0.2, random_state=42):
        """
        划分训练集和测试集
        
        参数:
            X: 特征数据
            y: 标签数据
            test_size: 测试集比例
            random_state: 随机种子
        
        返回:
            X_train, X_test, y_train, y_test
        """
        X = np.array(X)
        y = np.array(y).ravel()
        
        return train_test_split(X, y, test_size=test_size, random_state=random_state)
    
    def handle_missing_values(self, X, strategy='mean'):
        """
        处理缺失值
        
        参数:
            X: 输入数据
            strategy: 'mean', 'median', 'most_frequent', 或 'drop'
        
        返回:
            处理后的数据
        """
        from sklearn.impute import SimpleImputer
        
        X = np.array(X)
        
        if strategy == 'drop':
            # 删除包含缺失值的行
            mask = ~np.isnan(X).any(axis=1)
            return X[mask]
        else:
            imputer = SimpleImputer(strategy=strategy)
            return imputer.fit_transform(X)
    
    def remove_outliers(self, X, method='iqr', factor=1.5):
        """
        移除异常值
        
        参数:
            X: 输入数据
            method: 'iqr' (四分位距方法)
            factor: IQR 因子
        
        返回:
            清理后的数据和保留的索引
        """
        X = np.array(X)
        
        if method == 'iqr':
            Q1 = np.percentile(X, 25, axis=0)
            Q3 = np.percentile(X, 75, axis=0)
            IQR = Q3 - Q1
            
            lower_bound = Q1 - factor * IQR
            upper_bound = Q3 + factor * IQR
            
            mask = np.all((X >= lower_bound) & (X <= upper_bound), axis=1)
            return X[mask], np.where(mask)[0]
        else:
            raise ValueError(f"不支持的异常值处理方法: {method}")
    
    def prepare_data(self, X, y=None, normalize=False, normalize_method='standard',
                    remove_outliers=False, handle_missing=False, missing_strategy='mean'):
        """
        完整的数据准备流程
        
        参数:
            X: 特征数据
            y: 标签数据（可选）
            normalize: 是否标准化
            normalize_method: 标准化方法
            remove_outliers: 是否移除异常值
            handle_missing: 是否处理缺失值
            missing_strategy: 缺失值处理策略
        
        返回:
            处理后的数据
        """
        X = np.array(X)
        
        # 处理缺失值
        if handle_missing:
            X = self.handle_missing_values(X, strategy=missing_strategy)
        
        # 移除异常值
        if remove_outliers:
            X, indices = self.remove_outliers(X)
            if y is not None:
                y = np.array(y)[indices]
        
        # 标准化
        if normalize:
            X = self.normalize(X, method=normalize_method)
        
        if y is not None:
            return X, np.array(y)
        else:
            return X
