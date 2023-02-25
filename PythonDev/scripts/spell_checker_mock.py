""" Based on Peter Norvig's spell checker blog http://norvig.com/spell-correct.html
"""

if __name__ == "__main__":

    def edits1(word):
        "All edits that are one edit away from `word`."
        letters = "abcdefghijklmnopqrstuvwxyz"
        splits = [(word[:i], word[i:]) for i in range(len(word) + 1)]
        deletes = [L + R[1:] for L, R in splits if R]
        transposes = [L + R[1] + R[0] + R[2:] for L, R in splits if len(R) > 1]
        replaces = [L + c + R[1:] for L, R in splits if R for c in letters]
        inserts = [L + c + R for L, R in splits for c in letters]
        return set(deletes + transposes + replaces + inserts)

    def edits2(word):
        """All edits that are two edits away from `word`"""
        edits = set()
        for e1 in edits1(word):
            for e2 in edits1(e1):
                edits.add(e2)
        return edits

    def known(words, word_probs):
        known_words = set()
        for word in words:
            # Check if word is a key in word_probs
            if word in word_probs:
                known_words.add(word)
        return known_words

    # Load word_probs into a dictionary of word:prob
    word_probs_path = "data/word_probs.txt"
    word_probs = {}
    with open(word_probs_path, "r") as f:
        for line in f:
            word, prob = line.split(",")
            word_probs[word] = float(prob)

    misspelled_words = [
        "x",
        "xy",
        "tst",
        "accomodate",
        "recieve",
        "apele",
        "aple",
        "rythm",
        "banjana",
        "ghist",
        "flopeer",
    ]

    for word in misspelled_words:
        edits1_words = edits1(word)
        edits2_words = edits2(word)
        edit1_candidates = known(edits1_words, word_probs)
        edit2_candidates = known(edits2_words, word_probs)
        candidates = edit1_candidates.union(edit2_candidates)

        # Get the top 3 candidates by looking up the probability in word_probs
        top_3_candidates = sorted(candidates, key=lambda x: word_probs[x], reverse=True)[:3]
        print(f"{word} -> {top_3_candidates}")
