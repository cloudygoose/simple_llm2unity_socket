import socket
import struct
import traceback
import logging
import time
import numpy as np
import os

import logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s: %(message)s')
logger = logging.getLogger()

from transformers import AutoModelForSeq2SeqLM, AutoTokenizer

import torch

from my_utils import MyTimer, MyStruct, message_parse, message_form
import argparse
from t5_speed_test import speed_test

from openai_api import openai_call

import threading
locks = MyStruct(llm_local = threading.Lock(), llm_openai = threading.Lock(), mes = threading.Lock())

# part of code is from https://stackoverflow.com/questions/57080210/how-to-make-c-sharp-in-unity-communicate-with-python

def thread_llm_gen(g, mes_in):
    midx = mes_in.idx
    logger.info('thread start idx: %s llm_name: %s mes_in: %s', midx, mes_in.llm_name, str(mes_in.input_s))

    if mes_in.llm_name == 'flan_t5':
        llm_input = "Please give an interesting response to the following user utterance: " \
            + mes_in.input_s + " Your response:";
        inputs = g.tokenizer(llm_input, return_tensors="pt")
        inputs = inputs.to(g.device)

        g.locks.llm_local.acquire()
        with MyTimer('generation_timer', do_print = True):
            outputs = model.generate(**inputs, max_length = 100, min_length = 5)
            out_s = tokenizer.batch_decode(outputs, skip_special_tokens=True)[0]
        g.locks.llm_local.release()

    elif mes_in.llm_name in ['gpt_35_turbo']:
        g.locks.llm_openai.acquire() # is this lock necessary?
        out_s = openai_call(mes_in.llm_name, mes_in.input_s)
        g.locks.llm_openai.release()

    else:
        logger.error("unknown llm_name")
        sys.exit(1)

    g.locks.mes.acquire()
    g.mes_d[midx]['llm_out'] = out_s
    g.mes_d[midx]['llm_status'] = 'DONE'
    g.locks.mes.release()

    logger.info('thread finish idx: %s', midx)
    #return out_s

def sending_and_reciveing(g):
    mes_d = g.mes_d
    s = socket.socket()
    socket.setdefaulttimeout(None)
    logger.info('socket created ')
    port = 60000
    s.bind(('127.0.0.1', port)) #local host
    s.listen(30) #listening for connection for 30 sec?
    logger.info('socket listensing ... ')
    while True:
        try:
            c, addr = s.accept() #when port connected
            bytes_received = c.recv(4000) #received bytes
            sr = bytes_received.decode("utf-8")
            #logger.info('received:' + sr)
            mes_in = message_parse(sr); midx = mes_in.idx;

            if mes_in.ctrl == 'REQUEST':
                if not midx in mes_d:
                    mes_d[midx] = {'idx': midx, 'ctrl': mes_in.ctrl, 'npc_name': mes_in.npc_name, 'llm_name': mes_in.llm_name, 'llm_status': 'WORKING', 'input_s': mes_in.input_s}
                    #start the llm thread!
                    threading.Thread(target=thread_llm_gen, args=(g, mes_in)).start()
                if mes_d[midx]['llm_status'] == 'DONE':
                    ss = message_form(midx, 'DONE', mes_in.npc_name, mes_in.llm_name, mes_d[midx]['llm_out'])
                elif mes_d[midx]['llm_status'] == 'WORKING':
                    ss = message_form(midx, 'WORKING', mes_in.npc_name, mes_in.llm_name, 'placeholder')
                else:
                    logger.error("unknown llm_status"); sys.exit(1)
            elif mes_in.ctrl == 'CLEAR':
                ss = message_form('-1', 'CLEAR', 'placeholder', 'placeholder', 'placeholder')
                mes_d.clear()
                logger.info("mes_d cleared.")
            #time.sleep(0.5)
            c.send(ss.encode("utf-8"))
            c.close()
        except Exception as e:
            logging.error(traceback.format_exc())
            print("error")
            c.sendall(bytearray([]))
            c.close()
            break

if __name__ == "__main__":

    parser = argparse.ArgumentParser(
                        prog='ProgramName',
                        description='What the program does',
                        epilog='Text at the bottom of help')

    ## mps or cpu, mps is only good for larger batch, but mps does not use cpu, so python can use gpu, while unity uses cpu
    parser.add_argument('--device', default = 'mps') #mps or cpu, mps is only good for larger batch
    parser.add_argument('--msize', default = 'large')

    args = parser.parse_args()
    args.mname = "google/flan-t5-{}".format(args.msize)

    logger.info(str(args))

    device = torch.device(args.device)

    model = AutoModelForSeq2SeqLM.from_pretrained(args.mname) #xl
    model = model.to(device)

    tokenizer = AutoTokenizer.from_pretrained(args.mname) #xl

    mes_d = {} #the global message dict that threads will update and main logic read from

    glo = MyStruct(model = model, tokenizer = tokenizer, device = device, args = args, locks = locks, mes_d = mes_d)
 
    ##for larger batch (8 or 16), or longer length (5) mps is much faster than cpu
    ##
    #speed_test(glo, bz = 8, time = 4)

    sending_and_reciveing(glo) 