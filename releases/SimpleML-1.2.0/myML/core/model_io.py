"""
模型保存和加载模块
使用 joblib 进行模型序列化
"""

import os
import joblib
import json
from datetime import datetime


class ModelIO:
    """模型输入输出管理器"""
    
    @staticmethod
    def save_model(model, filepath, metadata=None):
        """
        保存模型到文件
        
        参数:
            model: 要保存的模型对象
            filepath: 保存路径（.pkl 或 .joblib 扩展名）
            metadata: 可选的元数据字典（算法类型、训练时间等）
        
        返回:
            保存的文件路径
        """
        # 确保文件扩展名正确
        if not filepath.endswith(('.pkl', '.joblib')):
            filepath += '.pkl'
        
        # 创建保存目录（如果不存在）
        os.makedirs(os.path.dirname(filepath) if os.path.dirname(filepath) else '.', exist_ok=True)
        
        # 准备保存的数据
        save_data = {
            'model': model,
            'metadata': metadata or {},
            'saved_at': datetime.now().isoformat()
        }
        
        # 保存模型
        joblib.dump(save_data, filepath)
        
        return filepath
    
    @staticmethod
    def load_model(filepath):
        """
        从文件加载模型
        
        参数:
            filepath: 模型文件路径
        
        返回:
            模型对象和元数据
        """
        if not os.path.exists(filepath):
            raise FileNotFoundError(f"模型文件不存在: {filepath}")
        
        # 加载模型
        save_data = joblib.load(filepath)
        
        model = save_data.get('model')
        metadata = save_data.get('metadata', {})
        
        return model, metadata
    
    @staticmethod
    def save_model_info(model, filepath, model_type, algorithm, metrics=None):
        """
        保存模型及其信息
        
        参数:
            model: 模型对象
            filepath: 保存路径
            model_type: 模型类型 ('classification', 'regression', 'clustering')
            algorithm: 使用的算法
            metrics: 评估指标（可选）
        
        返回:
            保存的文件路径
        """
        metadata = {
            'model_type': model_type,
            'algorithm': algorithm,
            'metrics': metrics,
            'saved_at': datetime.now().isoformat()
        }
        
        return ModelIO.save_model(model, filepath, metadata)
    
    @staticmethod
    def list_models(directory='.'):
        """
        列出目录中的所有模型文件
        
        参数:
            directory: 要搜索的目录
        
        返回:
            模型文件列表
        """
        model_files = []
        for filename in os.listdir(directory):
            if filename.endswith(('.pkl', '.joblib')):
                filepath = os.path.join(directory, filename)
                try:
                    _, metadata = ModelIO.load_model(filepath)
                    model_files.append({
                        'filename': filename,
                        'filepath': filepath,
                        'metadata': metadata
                    })
                except:
                    pass
        
        return model_files
