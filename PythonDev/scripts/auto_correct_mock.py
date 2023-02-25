import transformers
import torch
import re
import random


def is_next_word_state(context):
    if context[-1] in [" ", ".", ","]:
        return True
    else:
        return False


def sample_replacement_words(num_words):
    starter_words = words_generic + words_catlike + words_creative
    # sample num_words without replacement
    sampled_words = []
    for _ in range(num_words):
        sampled_words.append(random.choice(starter_words))
        starter_words.remove(sampled_words[-1])
    return sampled_words


def sample_tokens(context, top_k=25, sample=True):
    """Returns a list of the top 50 tokens in order of "sampled probability" """
    # Encode the text
    input_ids = tokenizer.encode(context, return_tensors="pt").cuda()
    # Use the model to get the logits for the last word
    logits = model(input_ids).logits[0][-1]
    logits = torch.softmax(logits, dim=0)

    # Zero out any tokens that are not in the allowed tokens
    logits[banned_tokens_ids] = 0

    # Get the top k words (probabilities and indices)
    top_k_tokens = torch.topk(logits, top_k)
    if sample:
        # Sample 50 words without replacement
        sampled_indices = torch.multinomial(top_k_tokens.values, top_k, replacement=False)
        # Get the indices of the top k words
        sampled_from_top_k = top_k_tokens.indices[sampled_indices]
    else:
        sampled_from_top_k = top_k_tokens.indices[0:top_k]

    # Decode the tokens
    sampled_tokens = []
    for token in sampled_from_top_k:
        sampled_tokens.append(tokenizer.decode(token))

    return sampled_tokens


def complete_word(context, current_word):
    for _ in range(5):
        # Sample the next token deterministically
        temp_context = context + " " + current_word
        next_token = sample_tokens(temp_context, top_k=1)[0]

        if next_token[0] in [",", ".", " "]:
            return current_word
        else:
            current_word += next_token
            context += next_token
    return current_word


def get_next_words(context):
    """If the last character is a space, period or comma, get the next words.
    Generate first next tokens - only select tokens that are not in the banned token list, sample without replacement from top ~20
    Finish words one by one (ordered by highest probability) and check them against a banned word list
    Once we get to 3 words, return them randomly
    """
    # Get a list of the top tokens from sampling without replacement
    # If last character is a space, remove it from context
    if context[-1] == " ":
        context = context[:-1]
    top_tokens = sample_tokens(context)

    top_3_next_words = []
    for top_token in top_tokens:
        # Remove any non alphanumeric characters
        top_token = re.sub(r"[^a-zA-Z0-9']", "", top_token)
        top_token = top_token.lower()
        # Complete the word
        next_word = complete_word(context, top_token)

        # Check if it is banned
        if next_word in banned_words:
            continue

        top_3_next_words.append(next_word)

        # Terminate once we have 3 words
        if len(top_3_next_words) == 3:
            break

    # Once 3 words are found, return them (if not pad to some random words)
    if len(top_3_next_words) < 3:
        top_3_next_words += sample_replacement_words(3 - len(top_3_next_words))

    return top_3_next_words


def get_word_completions(context):
    """Case if the user is in a middle of a word and the suggestion will replace their current word."""
    top_tokens = sample_tokens(context, top_k=50, sample=False)

    # Split into context and the word to be completed
    current_word = context.split(" ")[-1]
    context = " ".join(context.split(" ")[:-1])

    top_3_completed_words = []
    added_current_word = False
    for top_token in top_tokens:
        # Terminate once we have 3 words
        if len(top_3_completed_words) == 3:
            break

        # Check if the token suggests the word is over
        if top_token[0] in [",", ".", " "]:
            if not added_current_word:
                top_3_completed_words.append(current_word)
                added_current_word = True
            continue

        top_token = re.sub(r"[^a-zA-Z0-9']", "", top_token)
        top_token = top_token.lower()

        completed_word = complete_word(context, current_word + top_token)

        # Check if it is banned
        if completed_word in banned_words:
            continue

        top_3_completed_words.append(completed_word)

    # If there are less than 3 words, pad with random words
    if len(top_3_completed_words) < 3:
        top_3_completed_words += sample_replacement_words(3 - len(top_3_completed_words))

    # If added_current_word is true, then place that word at index 1 (so it appears in the middle)
    if added_current_word:
        top_3_completed_words.remove(current_word)
        top_3_completed_words.insert(1, current_word)

    return top_3_completed_words


if __name__ == "__main__":
    # Load model and tokenizer
    model_name = "gpt2"
    if True:
        with torch.no_grad():
            model = transformers.GPT2LMHeadModel.from_pretrained(model_name).cuda()
    tokenizer = transformers.GPT2Tokenizer.from_pretrained(model_name, use_fast=False)

    # Loading the starting words
    words_generic_path = "data/starting_words/words_generic.txt"
    words_catlike_path = "data/starting_words/words_catlike.txt"
    words_creative_path = "data/starting_words/words_creative.txt"

    with open(words_generic_path, "r") as f:
        words_generic = f.read().splitlines()

    with open(words_catlike_path, "r") as f:
        words_catlike = f.read().splitlines()

    with open(words_creative_path, "r") as f:
        words_creative = f.read().splitlines()

    # Computing banned tokens, i.e. any tokens that contain characters that are not alphanumeric, comma, space, period
    all_tokens = [tokenizer.decode(token_id) for token_id in list(range(50257))]
    regex = re.compile("^[a-zA-Z0-9 ']+$")
    banned_tokens = []
    for token in all_tokens:
        if not regex.match(token):
            banned_tokens.append(token)
    banned_token_ids = set()
    for token in banned_tokens:
        token_id = tokenizer.encode(token)[0]
        banned_token_ids.add(token_id)
    banned_tokens_ids = torch.tensor(list(banned_token_ids))

    # Load banned_words
    banned_words_path = "data/banned_words.txt"
    with open(banned_words_path, "r") as f:
        banned_words = f.read().splitlines()
        # Shift each character in the word back 13 letters in the alphabet
        banned_words = [
            word.translate(
                str.maketrans(
                    "ABCDEFGHIJKLMabcdefghijklmNOPQRSTUVWXYZnopqrstuvwxyz",
                    "NOPQRSTUVWXYZnopqrstuvwxyzABCDEFGHIJKLMabcdefghijklm",
                )
            )
            for word in banned_words
        ]

    def pretty_print_next_words(context, next_words):
        print(f"{context}[{' '.join(next_words)}]")

    test_next_words = [
        "my best ",
        "allen just sent a spaceship to the ",
        "i can't ",
        "i don't know what to do ",
        "how are my cats ",
        "mario is a cool ",
        "i just ",
        "when does ",
        "i forgot my ",
        "the door creaked ",
        "she whispered ",
        "the coffee was ",
        "i need more ",
        "the car won't ",
        "i love to ",
        "the sky is ",
        "my favorite color is ",
        "the rain is ",
        "he always ",
        "i'm allergic to ",
        "the movie made me ",
        "the cat meowed ",
        "the phone rang ",
        "the tree swayed ",
        "the music played ",
        "the pen ran out of ",
        "the book fell ",
        "the fire crackled ",
    ]

    test_middle_words = [
        "the door crea",
        "my be",
        "my n",
        "allen just sent a spaceship to th",
        "i can",
        "i don't know what to d",
        "how are my ca",
        "mario is a co",
        "i jus",
        "when doe",
        "i forgot m",
        "she whispe",
        "the coffee wa",
        "i need mor",
        "the car won",
        "i love t",
        "the sky i",
        "my favorite color i",
        "he alw",
        "the movie made m",
        "the cat me",
        "the phone ran",
        "the tree swaye",
        "the music pla",
        "the pen ran out o",
        "the book fe",
        "the fire cra",
    ]

    for test_scenario in test_next_words:
        pretty_print_next_words(test_scenario, get_next_words(test_scenario))

    print()

    for test_scenario in test_middle_words:
        pretty_print_next_words(test_scenario, get_word_completions(test_scenario))
