"""
安装与运行环境体检（含下一步引导与可选自动安装依赖）。
"""

from __future__ import annotations

import os
import platform
import subprocess
import sys
from typing import Dict, List, Tuple

from core.env_bootstrap import bootstrap_python_paths, discover_site_env_dirs, ensure_project_on_path
from components.i18n import is_zh, t


REQUIRED_PACKAGES = [
    ("numpy", "1.20.0"),
    ("pandas", "1.3.0"),
    ("sklearn", "1.0.0"),
    ("joblib", "1.0.0"),
]

OPTIONAL_PACKAGES = [
    ("openpyxl", "3.0.0"),
]


def _pkg_version(mod_name: str) -> Tuple[bool, str]:
    try:
        if mod_name == "sklearn":
            import sklearn
            return True, getattr(sklearn, "__version__", "unknown")
        mod = __import__(mod_name)
        return True, getattr(mod, "__version__", "unknown")
    except Exception as exc:
        return False, str(exc)


def _os_family() -> str:
    s = platform.system().lower()
    if s == "darwin":
        return "mac"
    if s == "windows":
        return "windows"
    return "linux"


def build_next_steps(status: str, required_failed: List[dict], project_dir: str) -> str:
    if is_zh():
        lines = ["下一步（请按顺序）", "=" * 40]
        if status == "PASS":
            lines.append("1. 在 Grasshopper 搜索「新手向导」，选择 classification")
            lines.append("2. 或打开 examples/ 中的配方说明，按线连接")
            lines.append("3. 推荐链路: 加载示例数据集 → 分割 → 智能训练 → 预测 → 评估")
            return "\n".join(lines)
    else:
        lines = ["Next steps (in order)", "=" * 40]
        if status == "PASS":
            lines.append("1. In Grasshopper search Beginner Wizard, set Task=classification")
            lines.append("2. Or follow examples/ recipes and wire components")
            lines.append("3. Recommended: Load Dataset → Split → Smart Train → Predict → Evaluate")
            return "\n".join(lines)

    fam = _os_family()
    missing_deps = [c for c in required_failed if c["name"].startswith("依赖") or c["name"].startswith("Dependency")]
    path_fail = [c for c in required_failed if "PATH" in c["name"] or "目录" in c["name"] or "folder" in c["name"].lower()]

    step = 1
    if path_fail:
        if is_zh():
            lines.append(f"{step}. 在 Grasshopper 搜索并打开「安装指南」组件，按平台说明放置 .gha 与包路径")
        else:
            lines.append(f"{step}. Open Installation Guide in Grasshopper and place .gha + package path")
        step += 1
        if fam == "mac":
            lines.append(
                f"{step}. macOS: put files under Rhinoceros/7.0 or 8.0 Grasshopper/Libraries/SimpleML/"
                if not is_zh()
                else f"{step}. macOS: 把文件放到 Rhinoceros/7.0 或 8.0 的 Grasshopper/Libraries/SimpleML/"
            )
        else:
            lines.append(
                f"{step}. Windows: %APPDATA%\\Grasshopper\\Libraries\\SimpleML\\"
                if not is_zh()
                else f"{step}. Windows: 放到 %APPDATA%\\Grasshopper\\Libraries\\SimpleML\\"
            )
        step += 1

    if missing_deps:
        if is_zh():
            lines.append(f"{step}. 缺少 Python 依赖。任选其一：")
            lines.append("   a) 将「环境体检」的 AutoFix 设为 true（自动 pip）")
            lines.append("   b) 终端执行:")
        else:
            lines.append(f"{step}. Missing Python packages. Choose one:")
            lines.append("   a) Set Health Check AutoFix=true (auto pip)")
            lines.append("   b) Run in a terminal:")
        lines.append(f"      \"{sys.executable}\" -m pip install scikit-learn numpy pandas joblib openpyxl")
        step += 1

    if is_zh():
        lines.append(f"{step}. 重新运行「环境体检」，确认 PASS 后再训练")
        step += 1
        lines.append(f"{step}. 若仍失败: 打开「安装指南」+ 查看 About 联系方式")
        if project_dir:
            lines.append(f"\n当前包路径候选: {project_dir}")
    else:
        lines.append(f"{step}. Re-run Health Check and confirm PASS before training")
        step += 1
        lines.append(f"{step}. If it still fails: open Installation Guide + About for contact")
        if project_dir:
            lines.append(f"\nPackage path candidate: {project_dir}")
    return "\n".join(lines)


def auto_install_deps() -> Tuple[bool, str]:
    """尝试用当前解释器 pip 安装依赖。"""
    pkgs = ["scikit-learn", "numpy", "pandas", "joblib", "openpyxl"]
    try:
        cmd = [sys.executable, "-m", "pip", "install", "--upgrade"] + pkgs
        proc = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=600,
        )
        out = (proc.stdout or "")[-1500:]
        err = (proc.stderr or "")[-1500:]
        ok = proc.returncode == 0
        detail = out + ("\n" + err if err else "")
        return ok, detail if detail else ("安装成功" if ok else "安装失败")
    except Exception as exc:
        return False, str(exc)


def run_health_check(project_dir: str = None, auto_fix: bool = False) -> Dict:
    added = bootstrap_python_paths(project_dir)
    root = ensure_project_on_path(project_dir)

    checks = []
    ok_count = 0
    auto_fix_log = ""

    def add(name, ok, detail):
        nonlocal ok_count
        checks.append({"name": name, "ok": bool(ok), "detail": detail})
        if ok:
            ok_count += 1

    add("操作系统", True, f"{platform.system()} {platform.release()} ({platform.machine()})")
    add("Python", True, f"{sys.version.split()[0]} @ {sys.executable}")
    add("SIMPLEML_PATH", bool(root and os.path.isdir(root)), root or "未找到项目路径")
    add(
        "components 目录",
        bool(root and os.path.isdir(os.path.join(root, "components"))),
        os.path.join(root, "components") if root else "N/A",
    )
    add(
        "core 目录",
        bool(root and os.path.isdir(os.path.join(root, "core"))),
        os.path.join(root, "core") if root else "N/A",
    )

    rhino_envs = discover_site_env_dirs()
    add(
        "Rhino site-envs",
        True,
        f"发现 {len(rhino_envs)} 处: {', '.join(rhino_envs[:3]) or '无（将使用当前 Python）'}",
    )

    for name, _min_ver in REQUIRED_PACKAGES:
        found, ver = _pkg_version(name)
        label = "scikit-learn" if name == "sklearn" else name
        add(f"依赖 {label}", found, ver if found else f"缺失: {ver}")

    for name, _min_ver in OPTIONAL_PACKAGES:
        found, ver = _pkg_version(name)
        add(f"可选 {name}", True, ver if found else f"未安装（Excel .xlsx 可能不可用）: {ver}")

    required_failed = [c for c in checks if not c["ok"] and not c["name"].startswith("可选")]
    if auto_fix and any(c["name"].startswith("依赖") for c in required_failed):
        ok_pip, log = auto_install_deps()
        auto_fix_log = log
        add("自动安装依赖", ok_pip, (log[:500] + "...") if len(log) > 500 else log)
        # 重新检测依赖
        for name, _ in REQUIRED_PACKAGES:
            found, ver = _pkg_version(name)
            label = "scikit-learn" if name == "sklearn" else name
            # 更新同名检查
            for c in checks:
                if c["name"] == f"依赖 {label}":
                    c["ok"] = found
                    c["detail"] = ver if found else f"缺失: {ver}"
        ok_count = sum(1 for c in checks if c["ok"])

    try:
        from sklearn.ensemble import RandomForestClassifier
        import numpy as np

        clf = RandomForestClassifier(n_estimators=5, random_state=42)
        X = np.array([[0, 0], [1, 1], [0, 1], [1, 0]])
        y = np.array([0, 1, 0, 1])
        clf.fit(X, y)
        pred = clf.predict([[0, 0]])
        add("sklearn 训练探测", True, f"预测样例={pred.tolist()}")
    except Exception as exc:
        add("sklearn 训练探测", False, str(exc))

    total = len(checks)
    required_failed = [c for c in checks if not c["ok"] and not c["name"].startswith("可选")]
    status = "PASS" if not required_failed else "FAIL"
    next_steps = build_next_steps(status, required_failed, root or "")

    lines: List[str] = []
    lines.append("SimpleML 环境体检报告")
    lines.append("=" * 56)
    lines.append(f"状态: {status}  |  通过项: {ok_count}/{total}")
    lines.append(f"已注入路径数: {len(added)}")
    lines.append("")
    for c in checks:
        mark = "✓" if c["ok"] else "✗"
        lines.append(f"{mark} {c['name']}: {c['detail']}")
    lines.append("")
    lines.append(next_steps)
    if auto_fix_log:
        lines.append("")
        lines.append("AutoFix 日志（节选）")
        lines.append(auto_fix_log[-800:])

    report = "\n".join(lines)
    summary = f"{status}: {ok_count}/{total} checks passed"
    return {
        "status": status,
        "summary": summary,
        "report": report,
        "next_steps": next_steps,
        "checks": checks,
        "project_dir": root,
        "python": sys.executable,
        "auto_fix_log": auto_fix_log,
    }
