# 测试

```bash
python -m pip install -r requirements.txt
python tests/test_simpleml.py -v
python tests/test_installed_full.py -v
python examples/quickstart_all.py
```

`test_simpleml.py`：源码核心链路  
`test_installed_full.py`：对 `SIMPLEML_PATH` / Helio 目录 / 源码根做更全覆盖  

Grasshopper 组件需在本机 Rhino 中目视验收。
