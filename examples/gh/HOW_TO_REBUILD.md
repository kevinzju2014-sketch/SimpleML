# How to refresh teaching .gh files

1. Open Rhino + Grasshopper, load SimpleML.
2. For each recipe in examples/*.md, place:
   Language | Health Check | Load Dataset | Split(optional) | Smart Train | Predict | Evaluate
3. Set Load Dataset name (iris / make_blobs / diabetes).
4. Add a Panel with the title + "Swap data: replace Load Dataset with Read CSV + Quick Dataset".
5. Save as examples/gh/01_classification_iris.gh (etc.).

Default parameters: Smart Train Task=auto or as listed; leave other knobs alone.
