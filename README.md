# XR AI Workbench

A Unity-based XR AI workbench built on Unity 6000.0.34f1.

## Setup Instructions

### 1. Scene Setup
- Open the **XrScene** in Unity
- Locate the **XrAiModelManager** in the scene

### 2. AI Provider Configuration
- Register for all the AI providers and obtain API Keys
- Enter the API Keys into the **XrAiModelManager**
- Save the Scene
- Click **Save to File** at the bottom of the XrAiModelManager

### 3. YOLO11 Installation

#### Download and Convert Model
1. Download `yolo11n-seg.pt` from [YOLO11 Performance Metrics](https://docs.ultralytics.com/models/yolo11/#performance-metrics)
2. Convert to `yolo11n-seg.onnx` using the [ONNX Integration Instructions](https://docs.ultralytics.com/integrations/onnx/#usage)

#### Unity Integration
1. Drag `yolo11n-seg.onnx` to `Assets/Resources/Yolo` folder
2. In Unity scene, go to **YoloModelConverter**
3. Drag `yolo11n-seg.onnx` to the **Onnx Model** field
4. Click **Generate**
5. Go to **Yolo Assets** and drag the generated `yolo11n-seg-sentis.sentis` to **Model Asset**
6. Find `Packages/XRAI Accelerator/Resources/yolo11n-labels` and drag to **Labels Asset**
7. Save Scene

## Requirements
- Unity 6000.0.34f1
- Valid API Keys for AI providers
- YOLO11 model files (see installation instructions above)
