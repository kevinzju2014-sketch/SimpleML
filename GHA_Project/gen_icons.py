import os
from PIL import Image, ImageDraw, ImageFont

icons_dir = os.path.join(os.path.dirname(__file__), "icons")
os.makedirs(icons_dir, exist_ok=True)

components = {
    "About": "AB",
    "CalculateCorrelation": "COR",
    "CalculateStatistics": "STA",
    "CreateDataset": "CDS",
    "DeconstructDataset": "DDS",
    "DescribeFeatures": "DES",
    "EvaluateClassification": "ECL",
    "EvaluateClustering": "ECU",
    "EvaluateRegression": "ERG",
    "GetDataSummary": "SUM",
    "InstallationGuide": "IG",
    "LoadDataset": "LDS",
    "LoadModel": "LDM",
    "PredictClassifier": "PCL",
    "PredictCluster": "PCU",
    "PredictRegressor": "PGR",
    "ReadCSV": "RCS",
    "ReadExcel": "REX",
    "SaveModel": "SVM",
    "SplitDataset": "SDS",
    "TrainAgglomerativeClustering": "TAC",
    "TrainClassifier": "TCL",
    "TrainCluster": "TCU",
    "TrainDBSCAN": "TDB",
    "TrainDecisionTreeClassifier": "TDT",
    "TrainKMeans": "TKM",
    "TrainKNNClassifier": "TKN",
    "TrainKNNRegressor": "TKR",
    "TrainLassoRegression": "TLS",
    "TrainLinearRegression": "TLR",
    "TrainLogisticRegressionClassifier": "TLG",
    "TrainNaiveBayesClassifier": "TNB",
    "TrainRandomForestClassifier": "TRF",
    "TrainRandomForestRegressor": "TFR",
    "TrainRegressor": "TGR",
    "TrainRidgeRegression": "TRD",
    "TrainSVMClassifier": "TSV",
    "TrainSVR": "TSR",
    "WriteCSV": "WCS",
    "WriteExcel": "WEX",
}

size = 24
font_size = 11

for name, abbr in components.items():
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    try:
        font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", font_size)
    except:
        font = ImageFont.load_default()
    
    bbox = draw.textbbox((0, 0), abbr, font=font)
    w = bbox[2] - bbox[0]
    h = bbox[3] - bbox[1]
    x = (size - w) / 2 - bbox[0]
    y = (size - h) / 2 - bbox[1]
    
    draw.text((x, y), abbr, fill=(0, 0, 0, 255), font=font)
    img.save(os.path.join(icons_dir, name + ".png"), 'PNG')
    print(f"Created: {name}.png")

print(f"\nDone! {len(components)} icons created in {icons_dir}")
