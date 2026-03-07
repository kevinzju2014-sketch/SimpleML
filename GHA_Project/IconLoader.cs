using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace SimpleML.Core
{
    /// <summary>
    /// 图标加载器 - 从嵌入资源加载组件图标
    /// </summary>
    public static class IconLoader
    {
        private static Assembly _assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// 从嵌入资源加载图标
        /// </summary>
        /// <param name="iconName">图标文件名（不含路径，如 "data_components.png"）</param>
        /// <returns>Bitmap对象，如果加载失败返回null</returns>
        public static Bitmap LoadIcon(string iconName)
        {
            try
            {
                // 尝试从嵌入资源加载
                // MSBuild会将icons文件夹中的文件嵌入为 SimpleML.icons.{filename}
                string resourceName = $"SimpleML.icons.{iconName}";
                using (Stream stream = _assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        return new Bitmap(stream);
                    }
                }

                // 如果嵌入资源不存在，尝试从文件系统加载（开发时使用）
                string assemblyPath = Path.GetDirectoryName(_assembly.Location);
                string iconPath = Path.Combine(assemblyPath, "Icons", iconName);
                if (File.Exists(iconPath))
                {
                    return new Bitmap(iconPath);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据组件名称加载图标
        /// </summary>
        /// <param name="componentName">组件类名（如 "ReadCSVComponent"）</param>
        /// <returns>Bitmap对象，如果加载失败返回null</returns>
        public static Bitmap LoadComponentIcon(string componentName)
        {
            string iconFileName = ComponentIconMap.GetIconFileName(componentName);
            if (string.IsNullOrEmpty(iconFileName))
            {
                return null;
            }
            return LoadIcon(iconFileName);
        }
    }
}
