# *its caturday*
**Download the game on [itch.io](https://unitedfailures.itch.io/its-caturday).** This game was made for [Ludwig Jam 2023](https://itch.io/jam/ludwig-2023).

Play as coots on her journey to be a social media influencer! 
Coots is trying to write posts about trending topics, but her paws are making it difficult to use a regular keyboard!

The game is powered by the same underlying AI model architecture as ChatGPT (transformer-based language models) to suggest next words based on the current written tweet. We also use transformers to determine how to score tweets with respect to the trending topics.
As far as we know, this is one of the first games written in Unity to perform local inference on transformer-based language models. We hope that this can serve as an example and inspiration for other Unity developers to use state-of-the-art AI models in their games.

The only controls are to use your mouse to select letters or suggested words on the onscreen keyboard.

![Screenshot](GameScreenshot.png)

## Warning
The AI models we use in the project were trained on datasets known to contain profanity, lewd, and otherwise abrasive language. 
Depending on the scenario, a user may be able to bypass our filtering produce socially unacceptable text. 
In the spirit of the game jam, we hope that players of the game will not abuse the language models in the game to produce offensive text.

# Development Prerequisites and Setup
Currently we only provide instructions for Windows.
## PythonDev
**Setting up a Python environment is necessary for downloading ONNX model files as we currently do not redistribute them in this repo due to file limitations.**

### Requirements (Windows)
* [Python 3.10.x](https://www.python.org/downloads/)
* (optional) CUDA 11.7 for using models on GPU

### (Simple instructions) Downloading ONNX exported models into the correct directory in the Unity Project.
* `pip install -r PythonDev\requirements.txt`
* From root run: `python PythonDev\transformers_export.py --model gpt2` and `python PythonDev\transformers_export.py --model largemnli`

### (Advanced) Python Environment Installation (Windows)
Setup a virtual environment for the project for robust development.
1. Create a new Python virtual environment in the `PythonDev` project directory.

    In the following example command, the first path is the path to your installed Python executable and the second path is the path to the directory where the virtual environment will be created.
    
    ```bash
    "C:\Program Files\Python310\python" -m venv "C:\Ludwig-Jam-2023\PythonDev\.venv"
    ```

2. Activate the virtual environment.
    
    ```bash
    cd PythonDev
    .\.venv\Scripts\activate
    ```

3. Upgrade pip, setuptools, and wheel
    
    ```bash
    pip install --upgrade pip setuptools wheel
    ```

4. Install the package (in editable mode). This also installs most dependencies. Some dependencies like PyTorch require custom installation (see step 5).
        
    ```bash
    pip install -e .[dev]
    ```

5. Install PyTorch. See [PyTorch's website](https://pytorch.org/get-started/locally/) for instructions for your platform.

### Run scripts to download ONNX model files and test the models
1. Activate the virtual environment making sure you are in the top level directory.
    
    ```bash
    >>> C:\Ludwig-Jam-2023> .\PythonDev\.venv\Scripts\activate
    >>> (.venv) C:\Ludwig-Jam-2023>
    ```

2. Download and export the models from HuggingFace. 
Run the following script twice to automatically place the ONNX files in the correct directory under `UnityProject`.
    
    ```bash
    python PythonDev\transformers_export.py --model gpt2
    python PythonDev\transformers_export.py --model largemnli
    ```

3. With the activated environment, scripts we used for testing can be run
    ```bash
    cd PythonDev
    python PythonDev\scripts\auto_correct_mock.py
    ```

## UnityProject
### Requirements (Windows)
* [Unity 2021.3.17f1](https://unity.com/download#how-get-started)

### Opening the Project in Unity
1. Launch Unity Hub
2. Click Projects > Open
3. Locate the project folder, `Ludwig-Jam-2023\UnityProject` from the cloned location on your machine using the file explorer and click Open.
4. The project immediately opens in the Editor and is added to the Hub.
5. Click the play button in the Editor to start using it!

### Model Files
ONNX Model files are placed in the `Assets\StreamingAssets` folder. 
See the [Unity Documentation](https://docs.unity3d.com/Manual/StreamingAssets.html) for more information on how to use this folder.
The models objects are referenced in the Unity project by the path to the model file relative to the `Assets` folder. 
Doing things this way allows for the Windows build to include the model files in the build automatically.
Alternatively, model files could be loaded from a web server only when they are requested.
In the future, using Barracuda to load the models may be a better option since model files could be referenced directly in the scene.

### Reference for installing ONNX Runtime in the Unity Project (already included in the project)
1. Install [NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity#how-do-i-install-nugetforunity) via Package Manager (see linked page for instructions).
2. Using NuGet inside Unity (dropdown at the top), search for and install the following packages:
    * Microsoft.Onnx.Runtime
    * Microsoft.Onnx.Runtime.Managed
3. After you have installed the Microsoft.Onnx.Runtime package via NuGet for Unity
    * In File Explorer go to the folder under `UnityProject` --> `Assets` -> `Packages` -> `Microsoft.ML.OnnxRuntime.1.13.1`
    * Locate `Microsoft.ML.OnnxRuntime.nupkg` inside the folder
    * Rename the `Microsoft.ML.OnnxRuntime.nupkg` to `Microsoft.ML.OnnxRuntime.zip` and extract it.
    * Inside the extracted folder, go to `runtimes` -> `win-x64` -> `native` and copy `onnxruntime.dll` and `onnxruntime_providers_shared.dll` into `UnityProject/Assets/Plugins`


# Attributions
## Redistributed by this project
* [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) (MIT License)
* [ONNX Runtime for C#](https://github.com/microsoft/onnxruntime) (MIT License)
* [MathNet.Numerics](https://www.nuget.org/packages/MathNet.Numerics) (MIT License)
* [Fastenshtein](https://github.com/DanHarltey/Fastenshtein) (MIT License)

## Other resources used by this project
- [GH Copilot Banned Word List ROT13 courtesy of Dolan-Gavitt](https://moyix.net/~moyix/copilot_slurs_rot13.txt) (No license)
- Music from Pixabay: [1](https://pixabay.com/music/beats-lo-fi-beauty-99516/), [2](https://pixabay.com/music/beats-sweet-chillhop-113777/), [3](https://pixabay.com/music/beats-lo-fi-chillhop-beat-background-music-133473/), [4](https://pixabay.com/music/beats-lofi-study-112191/), [5](https://pixabay.com/music/beats-breeze-soothing-lo-fi-music-78bpm-13596/), [6](https://pixabay.com/music/beats-relaxed-vlog-night-street-131746/) (Pixabay Simplified License)
- [Peter Norvig's Spell Corrector](http://norvig.com/spell-correct.html)
- [Cat Paw](https://www.pinterest.com/pin/619315386248540791/)
