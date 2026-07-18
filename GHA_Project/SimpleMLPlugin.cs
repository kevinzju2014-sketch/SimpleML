using System;
using System.Reflection;
using Grasshopper.Kernel;

namespace SimpleML
{
    /// <summary>
    /// SimpleML Plugin Info
    /// Grasshopper会自动发现所有继承自GH_Component的类
    /// </summary>
    public class SimpleMLPlugin : GH_AssemblyInfo
    {
        public override string Name => "SimpleML";
        public override string Description => "基于scikit-learn的Grasshopper机器学习插件（跨平台友好）";
        public override string AuthorName => "参数化凯通学";
        public override string AuthorContact => "zhao_guijia@outlook.com";
        // 正式插件 GUID（请勿再使用占位符）
        public override Guid Id => new Guid("8F3C2A91-6B47-4E1D-9C55-2D8A0E7B4F16");
        public override string Version => "1.1.0";
        
        public override System.Drawing.Bitmap Icon => null;
        
        public override string AssemblyVersion
        {
            get
            {
                try
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    return version != null ? version.ToString() : "1.1.0.0";
                }
                catch
                {
                    return "1.1.0.0";
                }
            }
        }
    }
}
