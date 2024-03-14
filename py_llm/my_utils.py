import time

class MyTimer:

    def __init__(self, name = None, do_print = True):
        self.name = name
        self.do_print = do_print

    def __enter__(self):
        self.tstart = time.time()

    def __exit__(self, type, value, traceback):
        time_elapse = time.time() - self.tstart
        if self.do_print:
            if self.name:
                print('[%s]' % self.name,)
            print('Elapsed: %s' % (time_elapse))

class MyStruct(object):
    def __init__(self, **kwargs):
        self.__dict__.update(kwargs)

def message_parse(s):
    tt = s.split(' ')
    idx, ctrl, npc_name, llm_name = tt[0], tt[1], tt[2], tt[3]
    input_s = ' '.join(tt[4:])
    return MyStruct(idx = idx, ctrl = ctrl, npc_name = npc_name, llm_name = llm_name, input_s = input_s)

def message_form(midx, ctrl, npc_name, llm_name, llm_out):
    return ' '.join([midx, ctrl, npc_name, llm_name, llm_out])