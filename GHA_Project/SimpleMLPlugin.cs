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
        public override string Description => "基于scikit-learn的Grasshopper机器学习插件";
        public override string AuthorName => "SimpleML Team";
        public override string AuthorContact => "";
        public override Guid Id => new Guid("12345678-1234-1234-1234-123456789012");
        public override string Version => "1.0.0";
        
        /// <summary>
        /// 返回程序集的图标（可选）
        /// </summary>
        public override System.Drawing.Bitmap Icon => null;
        
        /// <summary>
        /// 返回程序集的唯一标识符
        /// </summary>
        public override string AssemblyVersion
        {
            get
            {
                try
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    return version != null ? version.ToString() : "1.0.0.0";
                }
                catch
                {
                    return "1.0.0.0";
                }
            }
        }
    }

}
