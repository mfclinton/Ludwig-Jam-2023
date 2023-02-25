# Generating Transformer
* Install python
* `pip install -r PythonDev\requirements.txt`
* `python PythonDev\transformers_export.py --model gpt2`

# Packages Included
* [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) (MIT License)
* [ONNX Runtime for C#](https://github.com/microsoft/onnxruntime) (MIT License)
* [MathNet.Numerics](https://www.nuget.org/packages/MathNet.Numerics) (MIT License)

## PythonAIDev
**Setting up a Python environment is necessary for downloading ONNX model files as we currently do not redistribute them in this project due to file limitations.**

### Requirements (Windows)
* [Python 3.10.x](https://www.python.org/downloads/)
* (optional) CUDA 11.7 for using models on GPU

### Python Environment Installation (Windows)
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

# Sprite References

- https://www.pinterest.com/pin/619315386248540791/ (Temp Cat Paw)

## Attributions
- [GH Copilot Banned Word List ROT13 courtesy of Dolan-Gavitt](https://moyix.net/~moyix/copilot_slurs_rot13.txt) (No license)

## Warnings and Limitations
The language models we use in the project were trained on datasets known to contain profanity, lewd, and otherwise abrasive language. 
Depending on the use case, our game may produce offensive language and socially unacceptable text. 
In the spirit of the game jam, we hope that players of the game will not abuse the language models in the game to produce offensive text.