import os
#os.environ['HF_HOME'] = '/Users/goose/Documents/tf_cache/'

from transformers import AutoModelForSeq2SeqLM, AutoTokenizer

import torch

from utils import MyTimer, MyStruct
import argparse
from t5_speed_test import speed_test

parser = argparse.ArgumentParser(
                    prog='ProgramName',
                    description='What the program does',
                    epilog='Text at the bottom of help')

## mps or cpu, mps is only good for larger batch, but mps does not use cpu, so python can use gpu, while unity uses cpu
parser.add_argument('--device', default = 'mps') #mps or cpu, mps is only good for larger batch
parser.add_argument('--msize', default = 'large')

args = parser.parse_args()
args.mname = "google/flan-t5-{}".format(args.msize)

print(args)

device = torch.device(args.device)

model = AutoModelForSeq2SeqLM.from_pretrained(args.mname) #xl
model = model.to(device)

tokenizer = AutoTokenizer.from_pretrained(args.mname) #xl

glo = MyStruct(model = model, tokenizer = tokenizer, device = device)
 
##for larger batch (8 or 16), or longer length (5) mps is much faster than cpu
##
speed_test(glo, bz = 8, time = 4)

inputs = tokenizer("Please tell a long love story. Story:", return_tensors="pt")
inputs = inputs.to(device)

with MyTimer('generation_timer'):
    outputs = model.generate(**inputs, max_length = 100, min_length = 50)
    print(tokenizer.batch_decode(outputs, skip_special_tokens=True))

breakpoint()