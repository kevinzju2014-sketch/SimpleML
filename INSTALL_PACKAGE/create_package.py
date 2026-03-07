"""
创建SimpleML安装包
将myML文件夹和文档打包成可分发的安装包
"""

import os
import shutil
import zipfile
from pathlib import Path
from datetime import datetime

def create_install_package():
    """创建安装包"""
    
    # 路径设置
    script_path = Path(__file__).resolve()
    # INSTALL_PACKAGE在myML内部，所以myML是parent
    myml_dir = script_path.parent.parent
    base_dir = myml_dir.parent  # myML的父目录
    package_dir = base_dir / "SimpleML_Package"
    package_dir.mkdir(exist_ok=True)
    
    print("=" * 60)
    print("SimpleML Package Creator")
    print("=" * 60)
    print()
    print(f"Script path: {script_path}")
    print(f"myML directory: {myml_dir}")
    print(f"Base directory: {base_dir}")
    print(f"Package directory: {package_dir}")
    print()
    
    # 1. 复制myML文件夹
    print("[1/5] Copying myML folder...")
    myml_source = myml_dir
    myml_dest = package_dir / "myML"
    if myml_dest.exists():
        shutil.rmtree(myml_dest)
    shutil.copytree(myml_source, myml_dest, ignore=shutil.ignore_patterns(
        '__pycache__', '*.pyc', '.git', '*.gh', '*.ghx', 'INSTALL_PACKAGE', 'GHA_Project',
        '*.sln', '*.bat', '*.md'  # 排除开发文件
    ))
    print(f"  ✓ Copied to: {myml_dest}")
    
    # 2. 复制安装脚本
    print("[2/5] Copying installation scripts...")
    install_dir = package_dir / "Install"
    install_dir.mkdir(exist_ok=True)
    
    # 复制install.bat和install.sh
    install_package_dir = script_path.parent
    if (install_package_dir / "install.bat").exists():
        shutil.copy2(install_package_dir / "install.bat", install_dir)
    if (install_package_dir / "install.sh").exists():
        shutil.copy2(install_package_dir / "install.sh", install_dir)
        # 设置执行权限
        os.chmod(install_dir / "install.sh", 0o755)
    
    # 复制requirements.txt
    if (myml_dir / "requirements.txt").exists():
        shutil.copy2(myml_dir / "requirements.txt", install_dir)
    
    print(f"  ✓ Installation scripts copied")
    
    # 3. 复制文档
    print("[3/5] Copying documentation...")
    docs_dir = package_dir / "Docs"
    docs_dir.mkdir(exist_ok=True)
    
    doc_files = [
        "README_SIMPLEML.md",
        "SIMPLEML_CATEGORIES.md",
        "SIMPLEML_GRASSHOPPER_SETUP.md",
        "COMPONENTS_EXPLANATION.md",
        "WORKFLOW_DIAGRAM.md",
        "DATASET_GUIDE.md",
        "ALGORITHMS_GUIDE.md",
        "LOAD_MODEL_GUIDE.md",
        "ALGORITHM_DEFAULTS_GUIDE.md",
        "SPLIT_AND_EVALUATE_GUIDE.md"
    ]
    
    for doc_file in doc_files:
        source = myml_dir / doc_file
        if source.exists():
            shutil.copy2(source, docs_dir)
    
    print(f"  ✓ Documentation copied")
    
    # 4. 创建README
    print("[4/5] Creating package README...")
    readme_content = f"""# SimpleML Plugin Installation Package

## Package Information
- Version: 1.0.0
- Created: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
- Platform: Windows/Mac/Linux

## Installation

### Quick Install (Windows)
1. Double-click `Install\\install.bat`
2. Follow the prompts

### Quick Install (Mac/Linux)
1. Open terminal in this directory
2. Run: `bash Install/install.sh`

### Manual Install
1. Install Python dependencies:
   ```bash
   pip install -r Install/requirements.txt
   ```
2. Copy `myML` folder to:
   - Windows: `%APPDATA%\\Grasshopper\\UserObjects\\SimpleML\\`
   - Mac/Linux: `~/.grasshopper/UserObjects/SimpleML/`
3. Restart Grasshopper

## Contents

- `myML/` - Core plugin code
- `Install/` - Installation scripts
- `Docs/` - Documentation files

## Documentation

See `Docs/` folder for detailed documentation:
- README_SIMPLEML.md - Main documentation
- SIMPLEML_CATEGORIES.md - Battery categories
- SIMPLEML_GRASSHOPPER_SETUP.md - Setup guide

## Support

For issues or questions, please refer to the documentation.

---
SimpleML v1.0.0
"""
    
    with open(package_dir / "README.md", 'w', encoding='utf-8') as f:
        f.write(readme_content)
    print(f"  ✓ README created")
    
    # 5. 创建ZIP压缩包
    print("[5/5] Creating ZIP archive...")
    zip_path = base_dir / f"SimpleML_v1.0.0_{datetime.now().strftime('%Y%m%d')}.zip"
    
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(package_dir):
            # 排除不需要的文件
            dirs[:] = [d for d in dirs if d not in ['__pycache__', '.git']]
            
            for file in files:
                if not file.endswith(('.pyc', '.gh', '.ghx')):
                    file_path = Path(root) / file
                    arcname = file_path.relative_to(package_dir.parent)
                    zipf.write(file_path, arcname)
    
    print(f"  ✓ ZIP archive created: {zip_path}")
    
    print()
    print("=" * 60)
    print("Package creation completed!")
    print("=" * 60)
    print()
    print(f"Package directory: {package_dir}")
    print(f"ZIP archive: {zip_path}")
    print()
    print("Next steps:")
    print("1. Test the installation package")
    print("2. Create .ghuser files for Grasshopper user objects")
    print("3. Distribute the ZIP file")

if __name__ == "__main__":
    create_install_package()
