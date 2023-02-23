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

def twitter_sentiment_export():
    gpt2_output_file_path = "UnityProject\\Assets\\StreamingAssets\\AIModels\\LLMs\\Roberta\\TwitterSentiment"
    gpt2_name = "cardiffnlp/twitter-roberta-base-emotion"
    transformers_export(gpt2_name, "sequence-classification", "13", gpt2_output_file_path)

def english_emotion_export():
    gpt2_output_file_path = "UnityProject\\Assets\\StreamingAssets\\AIModels\\LLMs\\Roberta\\EnglishEmotion"
    gpt2_name = "j-hartmann/emotion-english-distilroberta-base"
    transformers_export(gpt2_name, "sequence-classification", "13", gpt2_output_file_path)

def parse_args():
    parser = argparse.ArgumentParser()

    parser.add_argument("--model", choices=["gpt2", "sentiment", "emotion"], type=str.lower)

    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()

    if args.model == "gpt2":
        gpt2_export()
    if args.model == "sentiment":
        twitter_sentiment_export()
    if args.model == "emotion":
        english_emotion_export()