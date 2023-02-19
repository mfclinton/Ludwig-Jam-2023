import os
import argparse


def transformers_export(model_name, feature, opset, output_file_path):
    """Export a model from the transformers library to ONNX format using the transformers.onnx script.
    https://huggingface.co/docs/transformers/serialization?highlight=onnx#exporting-a-model-to-onnx
    """
    export_model_command = f"python -m transformers.onnx --model={model_name} --feature={feature} --opset={opset} --framework=pt {output_file_path}"
    os.system(export_model_command)


def gpt2_export():
    gpt2_output_file_path = "UnityProject\\Assets\\StreamingAssets\\AIModels\\LLMs\\GPTDecoders\\GPT2OpenAI"
    gpt2_name = "gpt2"
    transformers_export(gpt2_name, "causal-lm", "13", gpt2_output_file_path)


def parse_args():
    parser = argparse.ArgumentParser()

    parser.add_argument("--model", choices=["gpt2"], type=str.lower)

    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()

    if args.model == "gpt2":
        gpt2_export()