using System;
using System.IO;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

namespace SimpleML.Components.ModelManagement
{
    public class SaveModelComponent : GH_Component
    {
        public SaveModelComponent()
          : base("保存模型 Save Model", "保存模型",
              "保存模型",
              "SimpleML", "04 Model")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "要保存的模型对象（Generic类型），通常来自训练组件", GH_ParamAccess.item);
            pManager.AddTextParameter("Filepath", "F", "保存路径，文件扩展名建议使用.pkl或.model", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save", "S", "选择True时执行存储命令", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved Path", "SP", "保存的文件路径", GH_ParamAccess.item);
            pManager.AddTextParameter("Readme", "R", "组件使用说明和注意事项", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object modelObj = null;
            string filepath = string.Empty;
            bool save = false;

            if (!DA.GetData(0, ref modelObj)) return;
            if (!DA.GetData(1, ref filepath)) return;
            DA.GetData(2, ref save);

            if (!save)
            {
                // 如果Save为False，不执行保存操作
                DA.SetData(0, "");
                DA.SetData(1, "Save参数为False，未执行保存操作");
                return;
            }

            if (string.IsNullOrEmpty(filepath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "文件路径不能为空");
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

                string modelStr = modelObj?.ToString() ?? "";
                // 使用 base64 编码来安全地传递文件路径，避免特殊字符导致的语法错误
                byte[] filepathBytes = System.Text.Encoding.UTF8.GetBytes(filepath);
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
    
    rhino_site_envs = str(__import__('pathlib').Path.home() / '.rhinocode' / 'py39-rh8' / 'site-envs')
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

from components.model_io_components import save_model

# 反序列化模型
try:
    model_bytes = base64.b64decode(r'''{modelStr}''')
    model = pickle.loads(model_bytes)
except:
    print('ERROR:无效的模型对象')
    exit(1)

# 使用 base64 解码文件路径，安全处理特殊字符
filepath_bytes = base64.b64decode(r'''{filepathBase64}''')
filepath = filepath_bytes.decode('utf-8')
saved_path = save_model(model, filepath)
print('SAVED_PATH:' + saved_path)
";

                string output = PythonScriptExecutor.ExecuteCode(pythonCode);
                DA.SetData(0, ExtractValue(output, "SAVED_PATH:"));
                
                // Readme输出
                string readme = @"组件名称: Save Model
功能: 保存训练好的模型到文件

═══════════════════════════════════════════════════════════════
输入参数详解:
═══════════════════════════════════════════════════════════════

1. Model (模型) - Generic类型，必需
   • 数据类型: 训练好的模型对象（Base64编码的pickle对象）
   • 数据结构: 包含模型的所有参数和状态
   • 连接建议:
     ← Train Classifier的Model输出（分类模型）
     ← Train Regressor的Model输出（回归模型）
     ← Train Cluster的Model输出（聚类模型）
   • 注意事项: 
     - 必须是有效的模型对象
     - 模型必须已经训练完成
     - 保存的模型可以跨会话使用

2. Filepath (文件路径) - Text类型，必需
   • 数据类型: 文本字符串
   • 格式: 完整的文件路径，包含文件名和扩展名
   • 示例: 
     - ""C:\Users\Models\iris_classifier.pkl""
     - ""D:\Projects\models\regression_model.model""
   • 文件扩展名建议:
     - .pkl: Python pickle格式（推荐，最常用）
     - .model: 通用模型格式
     - 其他: 任意扩展名，但建议使用.pkl
   • 注意事项: 
     - 如果文件已存在，将被覆盖
     - 确保目录存在，否则会报错
     - 路径中包含中文或特殊字符时需确保编码正确

3. Save (执行保存) - Boolean类型，默认False
   • 取值范围: True / False
   • 说明:
     - False: 不执行保存操作（默认）
     - True: 执行保存操作，保存模型到文件
   • 建议: 
     - 设置好所有参数后，将Save设置为True以保存模型
     - 这样可以避免意外覆盖文件

═══════════════════════════════════════════════════════════════
输出参数详解:
═══════════════════════════════════════════════════════════════

1. Saved Path (保存路径) - Text类型
   • 内容: 实际保存的文件完整路径
   • 用途: 确认文件保存位置，可用于后续加载
   • 说明: 如果保存成功，返回完整的文件路径；如果Save为False，返回空字符串

═══════════════════════════════════════════════════════════════
典型工作流程:
═══════════════════════════════════════════════════════════════

训练并保存模型:
Create Dataset → Split Dataset → Train Classifier/Regressor → Save Model

保存模型用于后续使用:
Train Classifier/Regressor → Save Model → (后续) → Load Model → Predict

模型版本管理:
Train Classifier (v1) → Save Model (model_v1.pkl)
Train Classifier (v2) → Save Model (model_v2.pkl)

═══════════════════════════════════════════════════════════════
注意事项:
═══════════════════════════════════════════════════════════════

1. 必须将Save设置为True才会执行保存操作
2. 文件路径必须有效，确保目录存在
3. 如果文件已存在，将被覆盖，请谨慎操作
4. 保存的模型可以使用Load Model组件加载
5. 建议使用.pkl扩展名，便于识别模型文件
6. 保存的模型包含所有训练参数，文件可能较大
7. 模型文件可以在不同的Grasshopper会话中使用";
                
                DA.SetData(1, readme);
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

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(SaveModelComponent));
        public override Guid ComponentGuid => new Guid("A7B8C9D0-E1F2-3456-0123-456789012346");
    }
}
