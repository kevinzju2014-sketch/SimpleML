"""
跨平台环境引导：自动发现 Rhino / 系统 Python 的 site-packages。
避免写死用户名或 Windows 绝对路径。
"""

from __future__ import annotations

import os
import site
import sys
from pathlib import Path
from typing import Iterable, List


def _unique_existing(paths: Iterable[str]) -> List[str]:
    seen = set()
    result = []
    for p in paths:
        if not p:
            continue
        norm = os.path.normpath(p)
        if norm in seen:
            continue
        if os.path.isdir(norm):
            seen.add(norm)
            result.append(norm)
    return result


def iter_rhinocode_roots() -> List[Path]:
    """返回可能的 .rhinocode 根目录（跨平台）。"""
    roots = []
    home = Path.home()
    roots.append(home / ".rhinocode")

    # Windows 额外常见位置
    appdata = os.environ.get("APPDATA")
    localappdata = os.environ.get("LOCALAPPDATA")
    userprofile = os.environ.get("USERPROFILE")
    for base in (appdata, localappdata, userprofile):
        if base:
            roots.append(Path(base) / ".rhinocode")

    # macOS Rhino 有时把 Python 放在 Application Support
    roots.append(home / "Library" / "Application Support" / "McNeel" / "Rhinoceros" / ".rhinocode")

    # 去重并保持顺序
    unique = []
    seen = set()
    for r in roots:
        key = str(r)
        if key not in seen:
            seen.add(key)
            unique.append(r)
    return unique


def discover_site_env_dirs() -> List[str]:
    """发现 Rhinocode site-envs 虚拟环境目录。"""
    found = []
    for root in iter_rhinocode_roots():
        if not root.exists():
            continue
        # py39-rh8 / py311-rh8 / ...
        for child in sorted(root.iterdir()):
            if not child.is_dir():
                continue
            site_envs = child / "site-envs"
            if site_envs.is_dir():
                found.append(str(site_envs))
            # 有些布局直接把 site-packages 放在环境根下
            for candidate in (
                child / "lib" / "python3.9" / "site-packages",
                child / "lib" / "python3.10" / "site-packages",
                child / "lib" / "python3.11" / "site-packages",
                child / "lib" / "python3.12" / "site-packages",
                child / "Lib" / "site-packages",
                child / "site-packages",
            ):
                if candidate.is_dir():
                    found.append(str(candidate))
    return _unique_existing(found)


def expand_site_env_packages(site_envs_dirs: Iterable[str]) -> List[str]:
    packages = []
    for site_envs in site_envs_dirs:
        try:
            for item in os.listdir(site_envs):
                env_path = os.path.join(site_envs, item)
                if not os.path.isdir(env_path):
                    continue
                packages.append(env_path)
                for sub in (
                    os.path.join(env_path, "Lib", "site-packages"),
                    os.path.join(env_path, "lib", "site-packages"),
                    os.path.join(env_path, "site-packages"),
                ):
                    if os.path.isdir(sub):
                        packages.append(sub)
        except OSError:
            continue
    return _unique_existing(packages)


def ensure_project_on_path(project_dir: str = None) -> str:
    """确保 SimpleML 项目根目录在 sys.path 中。"""
    if project_dir is None:
        env = os.environ.get("SIMPLEML_PATH")
        if env and os.path.isdir(env):
            project_dir = env
        else:
            current_dir = os.path.dirname(os.path.abspath(__file__))
            project_dir = os.path.dirname(current_dir)

    if project_dir and project_dir not in sys.path:
        sys.path.insert(0, project_dir)
    return project_dir


def bootstrap_python_paths(project_dir: str = None) -> List[str]:
    """
    配置 sys.path，返回已添加的路径列表。
    可在任意组件模块顶部调用。
    """
    added = []
    root = ensure_project_on_path(project_dir)
    if root:
        added.append(root)

    try:
        for sp in site.getsitepackages():
            if sp not in sys.path and os.path.isdir(sp):
                sys.path.insert(0, sp)
                added.append(sp)
    except Exception:
        pass

    try:
        usp = site.getusersitepackages()
        if usp and usp not in sys.path and os.path.isdir(usp):
            sys.path.insert(0, usp)
            added.append(usp)
    except Exception:
        pass

    for pkg in expand_site_env_packages(discover_site_env_dirs()):
        if pkg not in sys.path:
            sys.path.insert(0, pkg)
            added.append(pkg)

    return added


# 模块导入时自动引导（保持与旧组件兼容）
bootstrap_python_paths()
