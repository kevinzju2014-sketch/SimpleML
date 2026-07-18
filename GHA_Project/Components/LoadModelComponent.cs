using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelManagement
{
    public class LoadModelComponent : GH_Component
    {
        public LoadModelComponent()
          : base("加载模型 Load Model", "加载模型",
              "加载模型",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "F", "模型文件路径", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "加载的模型对象", GH_ParamAccess.item);
            pManager.AddTextParameter("Model Info", "MI", "模型信息（Tree结构）", GH_ParamAccess.tree);
            pManager.AddTextParameter("Readme", "R", "组件使用说明和注意事项", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filepath = string.Empty;
            if (!DA.GetData(0, ref filepath)) return;

            if (string.IsNullOrEmpty(filepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "文件路径不能为空");
                return;
            }

            // 如果文件不存在，尝试添加 .pkl 扩展名
            string actualFilepath = filepath;
            if (!File.Exists(actualFilepath))
            {
                // 检查是否已经有扩展名
                string extension = Path.GetExtension(actualFilepath);
                if (string.IsNullOrEmpty(extension))
                {
                    // 没有扩展名，尝试添加 .pkl
                    actualFilepath = actualFilepath + ".pkl";
                }
                else if (!extension.Equals(".pkl", StringComparison.OrdinalIgnoreCase) && 
                         !extension.Equals(".joblib", StringComparison.OrdinalIgnoreCase) &&
                         !extension.Equals(".model", StringComparison.OrdinalIgnoreCase))
                {
                    // 有扩展名但不是模型文件扩展名，也尝试添加 .pkl
                    actualFilepath = actualFilepath + ".pkl";
                }
            }

            // 再次检查文件是否存在
            if (!File.Exists(actualFilepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"文件不存在: {filepath}（已尝试添加 .pkl 扩展名）");
                return;
            }

            try
            {
                string mymlPath = GetMyMLPath();
                if (string.IsNullOrEmpty(mymlPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                        "未找到myML文件夹。请设置SIMPLEML_PATH环境变量。");
                    return;
                }

                // 使用 base64 编码来安全地传递文件路径，避免特殊字符导致的语法错误
                byte[] filepathBytes = System.Text.Encoding.UTF8.GetBytes(actualFilepath);
                string filepathBase64 = Convert.ToBase64String(filepathBytes);

                string pythonCode = $@"
# -*- coding: utf-8 -*-
import sys
import os
import json
import site
import io
import pickle
import base64

# 设置标准输出编码为UTF-8，避免中文输出错误
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if sys.stderr.encoding != 'utf-8':
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# 添加项目路径
sys.path.insert(0, r'{mymlPath}')

# 确保Rhino Python的site-packages在路径中
try:
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    rhino_site_envs = str(next((p for root in [__import__('pathlib').Path.home()/'.rhinocode', __import__('pathlib').Path.home()/'Library'/'Application Support'/'McNeel'/'Rhinoceros'/'.rhinocode'] if root.exists() for p in root.glob('py*-rh*/site-envs') if p.is_dir()), __import__('pathlib').Path.home()/'.rhinocode'/'site-envs'))
    if os.path.exists(rhino_site_envs):
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass

from components.model_io_components import load_model

# 使用 base64 解码文件路径，安全处理特殊字符
filepath_bytes = base64.b64decode(r'''{filepathBase64}''')
filepath = filepath_bytes.decode('utf-8')
model, metadata = load_model(filepath)

# 序列化模型
model_bytes = pickle.dumps(model)
model_str = base64.b64encode(model_bytes).decode('utf-8')

# 组织成 Tree 风格的 Model Info（列表的列表），与通用训练器一致
model_type = metadata.get('model_type', 'unknown') if isinstance(metadata, dict) else 'unknown'
algorithm = metadata.get('algorithm', 'unknown') if isinstance(metadata, dict) else 'unknown'
saved_at = metadata.get('saved_at', '') if isinstance(metadata, dict) else ''
n_features = metadata.get('n_features', None) if isinstance(metadata, dict) else None
n_classes = metadata.get('n_classes', None) if isinstance(metadata, dict) else None
n_clusters = metadata.get('n_clusters', None) if isinstance(metadata, dict) else None
metrics = metadata.get('metrics', None) if isinstance(metadata, dict) else None

model_info_tree = []
model_info_tree.append(['模型类型', str(model_type)])
model_info_tree.append(['算法', str(algorithm)])
if n_features is not None:
    model_info_tree.append(['特征维度', str(n_features)])
if n_classes is not None:
    model_info_tree.append(['类别数量', str(n_classes)])
if n_clusters is not None:
    model_info_tree.append(['聚类数量', str(n_clusters)])
model_info_tree.append(['保存时间', str(saved_at)])
model_info_tree.append(['文件路径', str(filepath)])
if metrics is not None:
    try:
        if isinstance(metrics, dict):
            for k, v in metrics.items():
                # 注意：这里不要使用 f-string（避免宿主字符串插值/引号导致编译失败）
                model_info_tree.append(['指标: ' + str(k), str(v)])
        else:
            model_info_tree.append(['指标', str(metrics)])
    except Exception:
        model_info_tree.append(['指标', str(metrics)])

model_info_json = json.dumps(model_info_tree, ensure_ascii=False, separators=(',', ':'), default=str)

print('MODEL:' + model_str)
print('MODEL_INFO:' + model_info_json)
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                DA.SetData(0, ExtractValue(output, "MODEL:"));
                string modelInfoJson = ExtractValue(output, "MODEL_INFO:");
                var modelInfoTree = TreeConverter.ConvertJsonToTree(modelInfoJson);
                DA.SetDataTree(1, modelInfoTree);
                
                // Readme输出
                string readme = @"组件名称: Load Model
功能: 加载已保存的模型文件

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Filepath (文件路径) - Text类型，必需
   • 数据类型: 文本字符串
   • 格式: 完整的文件路径，包含文件名和扩展名
   • 示例: 
     - ""C:\Users\Models\iris_classifier.pkl""
     - ""D:\Projects\models\regression_model.model""
   • 文件要求: 
     - 必须是有效的模型文件（通常由Save Model组件保存）
     - 建议使用.pkl扩展名
     - 文件必须存在，否则会报错
   • 注意事项: 
     - 路径中包含中文或特殊字符时需确保编码正确
     - 确保文件没有被损坏

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 加载的模型对象（Base64编码的pickle对象）
   • 用途: 用于预测和评估
   • 连接建议:
     → Predict Classifier的Model输入（分类模型）
     → Predict Regressor的Model输入（回归模型）
     → Predict Cluster的Model输入（聚类模型）
     → Evaluate Classification的Model输入（评估分类模型）
     → Evaluate Regression的Model输入（评估回归模型）
     → Evaluate Clustering的Model输入（评估聚类模型）
   • 说明: 
     - 模型已加载完成，可以直接使用
     - 模型类型必须与预测/评估任务匹配

2. Model Info (模型信息) - Tree结构
   • 内容: 模型信息（Tree结构），与通用训练器的Model Info一致风格
   • 包含信息（示例）:
     - 模型类型（分类/回归/聚类）
     - 算法名称
     - 保存时间
     - 文件路径
     - 指标（如果保存时包含）
   • 用途: 了解模型训练/保存信息，用于模型管理与排错

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

加载模型进行预测:
Load Model → Predict Classifier/Regressor/Cluster → Predictions

加载模型进行评估:
Load Model → Evaluate Classification/Regression/Clustering → Metrics

模型版本管理:
Load Model (model_v1.pkl) → Predict → (使用v1模型)
Load Model (model_v2.pkl) → Predict → (使用v2模型)

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 文件路径必须是有效的模型文件
2. 模型文件必须是用Save Model组件保存的
3. 加载的模型可以传递给Predict或Evaluate组件
4. 模型类型必须与使用场景匹配（分类/回归/聚类）
5. 如果模型文件损坏或格式不正确，会报错
6. Metadata包含模型的训练信息，可用于模型管理";
                
                DA.SetData(2, readme);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"执行失败: {ex.Message}");
                RhinoApp.WriteLine($"SimpleML错误: {ex}");
            }
        }

        private string GetMyMLPath()
        {
            return PathResolver.GetMyMLPath();
        }

        private string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;
            int endIndex = output.IndexOf('\n', startIndex);
            if (endIndex == -1) endIndex = output.Length;
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(LoadModelComponent));
        public override Guid ComponentGuid => new Guid("B8C9D0E1-F2A3-4567-1234-567890123457");
    }
}
