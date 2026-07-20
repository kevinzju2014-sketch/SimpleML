"""
文件读取组件
用于 Grasshopper Python 组件
支持读取 CSV 和 Excel 文件
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
# Rhino Python使用虚拟环境，需要确保site-packages可用
try:
    # 获取site-packages路径
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    # 尝试添加Rhino Python的site-envs路径
    # pandas / openpyxl 由 env_bootstrap 注入的 site-packages 提供
    rhino_site_envs = str(next((p for root in [__import__('pathlib').Path.home()/'.rhinocode', __import__('pathlib').Path.home()/'Library'/'Application Support'/'McNeel'/'Rhinoceros'/'.rhinocode'] if root.exists() for p in root.glob('py*-rh*/site-envs') if p.is_dir()), __import__('pathlib').Path.home()/'.rhinocode'/'site-envs'))
    if os.path.exists(rhino_site_envs):
        # 查找所有虚拟环境
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                # 添加虚拟环境根目录（pandas等库可能直接在这里）
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                # 添加虚拟环境的Lib\site-packages（如果存在）
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass  # 如果添加路径失败，继续执行

import pandas as pd
import numpy as np


def read_csv(filepath, encoding='utf-8', header=0, sep=','):
    """
    读取CSV文件组件
    
    输入:
        filepath: CSV文件路径（字符串）
        encoding: 文件编码（字符串，默认'utf-8'，可选'gbk'、'gb2312'等）
        header: 表头行号（整数，默认0，表示第一行是表头，None表示无表头）
        sep: 分隔符（字符串，默认','，可选';'、'\t'等）
    
    输出:
        data: 读取的数据（pandas DataFrame）
        columns: 列名列表
        shape: 数据形状（行数, 列数）的元组
    """
    # 如果指定了编码，直接尝试
    if encoding and encoding.lower() != 'auto':
        try:
            data = pd.read_csv(filepath, encoding=encoding, header=header, sep=sep)
            columns = data.columns.tolist()
            shape = data.shape
            return data, columns, shape
        except UnicodeDecodeError:
            # 如果指定编码失败，继续尝试其他编码
            pass
    
    # 自动尝试多种编码（按常见程度排序）
    encodings_to_try = ['utf-8', 'gbk', 'gb2312', 'gb18030', 'utf-8-sig', 'latin1', 'cp1252']
    
    # 如果用户指定了编码，将其放在最前面
    if encoding and encoding.lower() != 'auto':
        if encoding.lower() in encodings_to_try:
            encodings_to_try.remove(encoding.lower())
        encodings_to_try.insert(0, encoding.lower())
    
    last_error = None
    for enc in encodings_to_try:
        try:
            data = pd.read_csv(filepath, encoding=enc, header=header, sep=sep)
            columns = data.columns.tolist()
            shape = data.shape
            return data, columns, shape
        except UnicodeDecodeError as e:
            last_error = e
            continue
        except Exception as e:
            # 其他错误（如文件不存在等）直接抛出
            raise ValueError(f"读取CSV文件失败: {str(e)}")
    
    # 所有编码都失败
    raise ValueError(f"读取CSV文件失败: 无法使用以下编码读取文件: {', '.join(encodings_to_try)}。错误: {str(last_error)}。请尝试手动指定正确的编码（如'gbk'或'gb2312'）。")


def read_excel(filepath, sheet_name=0, header=0):
    """
    读取Excel文件组件
    
    输入:
        filepath: Excel文件路径（字符串，支持.xlsx和.xls格式）
        sheet_name: 工作表名称或索引（字符串或整数，默认0表示第一个工作表）
        header: 表头行号（整数，默认0，表示第一行是表头，None表示无表头）
    
    输出:
        data: 读取的数据（pandas DataFrame）
        columns: 列名列表
        shape: 数据形状（行数, 列数）的元组
    """
    # 根据文件扩展名选择引擎
    file_ext = os.path.splitext(filepath)[1].lower()
    
    # 尝试的引擎顺序
    engines_to_try = []
    if file_ext == '.xlsx':
        engines_to_try = ['openpyxl', 'xlrd']
    elif file_ext == '.xls':
        engines_to_try = ['xlrd', 'openpyxl']
    else:
        # 未知扩展名，尝试所有引擎
        engines_to_try = ['openpyxl', 'xlrd']
    
    last_error = None
    for engine in engines_to_try:
        try:
            data = pd.read_excel(filepath, sheet_name=sheet_name, header=header, engine=engine)
            columns = data.columns.tolist()
            shape = data.shape
            return data, columns, shape
        except ImportError as e:
            # 如果引擎不存在，尝试下一个
            last_error = e
            continue
        except ValueError as e:
            # 如果是工作表名称错误，尝试使用索引0
            if "Worksheet named" in str(e) and "not found" in str(e):
                # 如果sheet_name是字符串（工作表名称），尝试使用索引0
                if isinstance(sheet_name, str):
                    try:
                        # 获取所有工作表名称以便在错误信息中显示
                        excel_file = pd.ExcelFile(filepath, engine=engine)
                        available_sheets = excel_file.sheet_names
                        excel_file.close()
                        
                        # 尝试使用索引0（第一个工作表）
                        data = pd.read_excel(filepath, sheet_name=0, header=header, engine=engine)
                        columns = data.columns.tolist()
                        shape = data.shape
                        # 成功读取，返回数据（静默使用第一个工作表）
                        return data, columns, shape
                    except Exception as e2:
                        # 如果使用索引0也失败，抛出包含可用工作表信息的错误
                        available_sheets_str = ', '.join(available_sheets) if 'available_sheets' in locals() else '无法获取'
                        raise ValueError(
                            f"读取Excel文件失败: 工作表名称 '{sheet_name}' 不存在，且无法读取第一个工作表。"
                            f"可用的工作表: {available_sheets_str}。"
                            f"请使用工作表名称或索引（0表示第一个工作表）。"
                        )
                else:
                    last_error = e
                    continue
            else:
                last_error = e
                continue
        except Exception as e:
            # 其他错误（如文件格式问题），也尝试下一个引擎
            last_error = e
            continue
    
    # 所有引擎都失败
    error_msg = str(last_error) if last_error else "未知错误"
    if 'openpyxl' in error_msg.lower() or 'xlrd' in error_msg.lower() or 'Missing optional dependency' in error_msg:
        raise ValueError(
            f"读取Excel文件失败: 缺少必需的库。\n"
            f"错误: {error_msg}\n"
            f"请安装openpyxl（用于.xlsx文件）或xlrd（用于.xls文件）。\n"
            f"安装命令: pip install openpyxl xlrd"
        )
    else:
        raise ValueError(f"读取Excel文件失败: {error_msg}")


def dataframe_to_numpy(data, columns=None):
    """
    将DataFrame转换为numpy数组组件
    
    输入:
        data: pandas DataFrame对象
        columns: 要选择的列名列表（可选，None表示选择所有列）
    
    输出:
        array: numpy数组
        selected_columns: 选择的列名列表
    """
    if columns is None:
        selected_columns = data.columns.tolist()
        array = data.values
    else:
        selected_columns = columns
        array = data[columns].values
    
    return array, selected_columns


def extract_features_and_target(data, feature_columns, target_column=None):
    """
    从DataFrame中提取特征和目标变量组件
    
    输入:
        data: pandas DataFrame对象
        feature_columns: 特征列名列表（字符串列表）
        target_column: 目标列名（字符串，可选，None表示只提取特征）
    
    输出:
        X: 特征数据（numpy数组）
        y: 目标数据（numpy数组，如果target_column为None则返回None）
        feature_names: 特征名称列表
    """
    # 提取特征
    X = data[feature_columns].values
    feature_names = feature_columns
    
    # 提取目标（如果提供）
    if target_column is not None:
        y = data[target_column].values
        return X, y, feature_names
    else:
        return X, None, feature_names


def get_data_info(data):
    """
    获取数据基本信息组件
    
    输入:
        data: pandas DataFrame对象
    
    输出:
        info: 数据信息字典
            - shape: 数据形状
            - columns: 列名列表
            - dtypes: 数据类型字典
            - memory_usage: 内存使用量（字节）
    """
    info = {
        'shape': data.shape,
        'columns': data.columns.tolist(),
        'dtypes': data.dtypes.to_dict(),
        'memory_usage': data.memory_usage(deep=True).sum()
    }
    
    return info


def write_csv(data, filepath, index=False, encoding='utf-8', sep=','):
    """
    写入CSV文件组件
    
    输入:
        data: 要写入的数据（pandas DataFrame或列表的列表）
        filepath: 保存路径（字符串）
        index: 是否写入行索引，默认False
        encoding: 文件编码，默认'utf-8'
        sep: 分隔符，默认','
    
    输出:
        saved_path: 保存的文件路径
        info: 保存信息（行数、列数等）
    """
    # 如果data是列表的列表，转换为DataFrame
    if isinstance(data, list):
        if len(data) > 0 and isinstance(data[0], list):
            # 列表的列表（Tree结构）
            data = pd.DataFrame(data)
        else:
            # 一维列表
            data = pd.DataFrame(data)
    
    # 确保是DataFrame
    if not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    # 写入CSV文件
    data.to_csv(filepath, index=index, encoding=encoding, sep=sep)
    
    info = f'文件已保存: {filepath}, 行数: {data.shape[0]}, 列数: {data.shape[1]}'
    
    return filepath, info


def write_excel(data, filepath, sheet_name='Sheet1', index=False):
    """
    写入Excel文件组件
    
    输入:
        data: 要写入的数据（pandas DataFrame或列表的列表）
        filepath: 保存路径（字符串，.xlsx或.xls）
        sheet_name: 工作表名称，默认'Sheet1'
        index: 是否写入行索引，默认False
    
    输出:
        saved_path: 保存的文件路径
        info: 保存信息（行数、列数等）
    """
    # 如果data是列表的列表，转换为DataFrame
    if isinstance(data, list):
        if len(data) > 0 and isinstance(data[0], list):
            # 列表的列表（Tree结构）
            data = pd.DataFrame(data)
        else:
            # 一维列表
            data = pd.DataFrame(data)
    
    # 确保是DataFrame
    if not isinstance(data, pd.DataFrame):
        data = pd.DataFrame(data)
    
    # 根据文件扩展名选择引擎
    file_ext = os.path.splitext(filepath)[1].lower()
    engine = 'openpyxl' if file_ext == '.xlsx' else 'xlwt'
    
    # 写入Excel文件
    try:
        data.to_excel(filepath, sheet_name=sheet_name, index=index, engine=engine)
    except ImportError:
        # 如果引擎不存在，尝试另一个
        if engine == 'openpyxl':
            engine = 'xlwt'
        else:
            engine = 'openpyxl'
        data.to_excel(filepath, sheet_name=sheet_name, index=index, engine=engine)
    
    info = f'文件已保存: {filepath}, 工作表: {sheet_name}, 行数: {data.shape[0]}, 列数: {data.shape[1]}'
    
    return filepath, info
