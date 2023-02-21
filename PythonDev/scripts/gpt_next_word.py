import random

"""Goal is to get three top words for the next word in the tweet.

First word: Sample top words from hardcoded categories
"""

""" EXTRA CODE
def sort_save_words(words, file_path):
    # Sort the words alphabetically
    words.sort()
    # Write them to that text file
    with open(file_path, "w") as f:
        # Write each word on a new line
        for word in words:
            f.write(word + "\n")

sort_save_words(words_generic, words_generic_path)
sort_save_words(words_catlike, words_catlike_path)
sort_save_words(words_creative, words_creative_path)
"""


if __name__ == "__main__":
    words_generic_path = "data/starting_words/words_generic.txt"
    words_catlike_path = "data/starting_words/words_catlike.txt"
    words_creative_path = "data/starting_words/words_creative.txt"
    # Load in the top starting words from data/starting_words/generic_words.txt
    with open(words_generic_path, "r") as f:
        words_generic = f.read().splitlines()

    with open(words_catlike_path, "r") as f:
        words_catlike = f.read().splitlines()

    with open(words_creative_path, "r") as f:
        words_creative = f.read().splitlines()

    # Combine all the words into one list and sample 3 words
    words = words_generic + words_catlike + words_creative
    curr_tweet = random.sample(words, 3)
