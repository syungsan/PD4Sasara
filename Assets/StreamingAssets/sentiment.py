#!/usr/bin/env python
# coding: utf-8

import warnings
warnings.simplefilter("ignore")

from keras.preprocessing.sequence import pad_sequences
from keras.models import load_model
import pickle
import tensorflow as tf

import os
PWD = os.path.dirname(os.path.realpath(__file__))


def preprocess(data, tokenizer, maxlen=280):
    return(pad_sequences(tokenizer.texts_to_sequences(data), maxlen=maxlen))


def predict(sentences, graph, emolabels, tokenizer, model, maxlen):
    preds = []
    targets = preprocess(sentences, tokenizer, maxlen=maxlen)
    with graph.as_default():
        for i, ds in enumerate(model.predict(targets)):
            preds.append({
                "sentence":sentences[i],
                "emotions":dict(zip(emolabels, [str(round(100.0*d)) for d in ds]))
            })
    return preds


def load(path):
    model = load_model(path)
    graph = tf.get_default_graph()
    return model, graph


if __name__ == "__main__":

    maxlen = 280
    model, graph = load(PWD + "/model/model_2018-08-28-15_00.h5")

    with open(PWD + "/model/tokenizer_cnn_ja.pkl", "rb") as f:
        tokenizer = pickle.load(f)

    emolabels = ["happy", "sad", "disgust", "angry", "fear", "surprise"]

    # print("Model Load Complete")

    import sys
    # sentence = [sys.argv[1]]
    
    import io
    
    sys.stdin = io.TextIOWrapper(sys.stdin.buffer, encoding="utf-8")
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
    
    import wave
    import winsound as ws
    
    sound_name = PWD + "/decision1.wav"
    ws.PlaySound(sound_name, ws.SND_FILENAME)

    while True:
 
        raw = input()
        
        # encoded = raw.encode("utf-8", errors="surrogateescape")
        
        sentence = [raw]

        results = predict(sentence, graph, emolabels, tokenizer, model, maxlen)[0]

        # for text_with_emotion in results:
        emotions = results['emotions']

        probabilitys = []
        for emotion_item in emotions.items():
            probabilitys.append(float(emotion_item[1]))

        summation = sum(probabilitys)

        probability_distributions = []
        for probability in probabilitys:
            probability_distributions.append(probability / summation)

        # 感情のスコアがもっとも高いものだけを抽出（後々確率分布をアンサンブルに利用する）
        max_emo = [max_emotions[0] for max_emotions in emotions.items() if max_emotions[1] == max(
            emotions.items(), key=(lambda emotion: float(emotion[1])))[1]][0]

        max_probability = float([max_emotions[1] for max_emotions in emotions.items() if max_emotions[1] == max(
            emotions.items(), key=(lambda emotion: float(emotion[1])))[1]][0]) / summation * 100.0

        output = "WORD_EMOTION,%s,%s" % (max_emo, max_probability)
        
        for probability_distribution in probability_distributions:
            output += ",%s" % str(probability_distribution)
            
        # output += raw + str(encoded) + str(sentence)

        print(output, flush=True)
