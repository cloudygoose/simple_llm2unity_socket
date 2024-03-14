import os
#os.environ['HF_HOME'] = '/Users/goose/Documents/tf_cache/'

from transformers import AutoModelForSeq2SeqLM, AutoTokenizer

import torch

from my_utils import MyTimer
import argparse

sample_text = "Donald John Trump (born June 14, 1946) is an American politician, media personality, and businessman who served as the 45th president of the United States from 2017 to 2021. Trump received a Bachelor of Science in economics from the University of Pennsylvania in 1968, and his father named him president of his real estate business in 1971. Trump renamed it the Trump Organization and reoriented the company toward building and renovating skyscrapers, hotels, casinos, and golf courses. After a series of business failures in the late twentieth century, he successfully launched side ventures that required little capital, mostly by licensing the Trump name. From 2004 to 2015, he co-produced and hosted the reality television series The Apprentice. He and his businesses have been plaintiff or defendant in more than 4,000 state and federal legal actions, including six business bankruptcies. Trump won the 2016 presidential election as the Republican Party nominee against Democratic Party nominee Hillary Clinton while losing the popular vote.[a] During the campaign, his political positions were described as populist, protectionist, isolationist, and nationalist. His election and policies sparked numerous protests. He was the first U.S. president with no prior military or government experience. A special counsel investigation established that Russia had interfered in the 2016 election to favor Trump's campaign. Trump promoted conspiracy theories and made many false and misleading statements during his campaigns and presidency, to a degree unprecedented in American politics. Many of his comments and actions have been characterized as racially charged or racist and many as misogynistic."

def speed_test(g, bz = 1, len_mul = 1, time = 1):

    inputs = g.tokenizer(sample_text * len_mul, return_tensors="pt")
    inputs = inputs.to(g.device)
    input_ids = inputs['input_ids']
    if bz > 1:
        input_ids = torch.stack([input_ids] * bz, dim = 0).squeeze()

    with MyTimer('speedtest_timer'):
        for tt in range(time):
            outputs = g.model.encoder(input_ids)
            print(tt, outputs.last_hidden_state.size())
        #outputs = model.generate(**inputs, max_length = 100, min_length = 50)
        #print(tokenizer.batch_decode(outputs, skip_special_tokens=True))

    breakpoint()