# SimpleML Icon Spec — Plan L1 (Official Vivid)

**已选定：L1**

## 设计原则

- **24 × 24** PNG RGBA，透明底
- **像素原生块面**（在 24px 网格上直接画，不先做高清再缩小）
- 最小特征约 3–4px；硬边缘，无柔光碎点
- 浅灰 GH 工具栏上可辨认
- 官方气质：立体色块物件，非色砖底板

## 生成 / 安装

```bash
python scripts/gen_icons_l1.py
dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7
```

联系表：`docs/icon_l1_contact_sheet.png`  
选型记录：`docs/icon_choose_light.html`
