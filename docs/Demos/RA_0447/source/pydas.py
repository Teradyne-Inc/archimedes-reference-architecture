#!/usr/local/bin/python3

'''
PyDAS application file to be run in a docker on the UltraEdge
'''

import logging
import threading

from flask import Flask, request

# next_low_limit = 0

fifo_output = "/app/exec/fifo_to_TP"
output_stream = open(fifo_output, 'w')

# To avoid printing messages on the console
log = logging.getLogger('werkzeug')
log.setLevel(logging.ERROR)

# Flask app
app = Flask(__name__)

data_threads = []
data_info = []

class TestData():
    ''' Class to record data for one test '''
    def __init__(self):
        self.test_num = None
        self.site_num = None
        self.result = None
        self.lo_lim = None
        self.hi_lim = None

    def extract_data(self, json_data):
        ''' From an AMP json object, extract the required information '''
        self.test_num = json_data["TEST_NUMBER"]
        self.site_num = json_data["SITE_NUM"]
        self.result = json_data["RESULT"]
        self.lo_lim = json_data["LO_LIM"]
        self.hi_lim = json_data["HI_LIM"]

    def to_string(self):
        ''' Stringify the test data '''
        return f"[S:{self.site_num},TN:{self.test_num},R:{self.result},LLM:{self.lo_lim},HLM:{self.hi_lim}]"

def debug_log(host, msg, lotid = None, sublotid = None):
    ''' Debug logging. Not used in production. '''
    lotidstr = lotid if (lotid is not None) else ''
    sublotidstr = sublotid if (sublotid is not None) else ''
    msgtolog = f"{host}|{lotidstr}|{sublotidstr}|{msg}"
    print(msgtolog)
    with open("LoggedMessages.txt", "a", -1, "UTF-8") as log_file:
        log_file.write(f"{msgtolog}\n")

@app.route('/', defaults={'path': ''}, methods=['POST'])
@app.route('/<path:path>', methods=['POST'])
def handle_default(path):
    '''
    Default management function for AMP messages.
    Simple message name logging. 
    '''
    debug_log("", path)
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<path:cellhost>/STATUS', methods=['POST'])
def handle_STATUS(cellhost):
    ''' Skip STATUS messages '''
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/SUBLOT_START', methods=['POST'])
def handle_SUBLOT_START(cellhost, lotid, sublotid):
    ''' Sublot start management '''
    debug_log(cellhost, "SUBLOT_START", lotid, sublotid)
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_START', methods=['POST'])
def handle_TEST_START(cellhost, lotid, sublotid):
    ''' Test start management '''
    global data_threads
    debug_log(cellhost, "TEST_START", lotid, sublotid)
    data_threads.clear() # requires finer management, but, hey, it is not the purpose of this sample code
    data_info.clear()
    return ('', 200)

def manage_data(message, cellhost, lotid, sublotid):
    ''' Manage parametric data '''
    global data_info
    lst = []
    param = message["PARAMETRIC_DATA"]
    if len(param) > 0:
        for jsont in param:
            t = TestData()
            t.extract_data(jsont)
            lst.append(t)

    data_info.extend(lst)
    lst_str = ','.join([t.to_string() for t in lst])
    # data_info.append(lst_str)
    debug_log(cellhost, f"TEST_DATA({lst_str})", lotid, sublotid)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_DATA', methods=['POST'])
def handle_TEST_DATA(cellhost, lotid, sublotid):
    ''' Test data management '''
    global data_threads
    message = request.get_json(force=True)
    x = threading.Thread(target=manage_data, args=(message, cellhost, lotid, sublotid,))
    data_threads.append(x)
    x.start()
    return ('', 200)

def compute_next_LLM():
    ''' Use the data_info list to recompute the new low limit '''
    global data_info
    if len(data_info) <= 0:
        return -1
    val = sum(d.result for d in data_info)
    avg = val / len(data_info)
    return round(avg, 4)

def send_next_step_to_tp(cellhost):
    ''' Compute the next low limit to be used by the test program '''
    # global next_low_limit, data_threads, data_info
    global data_threads, data_info

    # Waiting for all threads managing data for this touchdown
    debug_log(cellhost, "Waiting for all TEST_DATA threads to terminate")
    for t in data_threads:
        t.join()

    # Compute test data report    
    report = ','.join([t.to_string() for t in data_info])
    debug_log(cellhost, f"Report: {report}")

    # Compute next low limit
    next_low_limit = compute_next_LLM()
    debug_log(cellhost, f"New low limit: {next_low_limit}")

    # Compute message to send back to the tester host
    tosend = f"CODE:{next_low_limit}|report:{report}"
    debug_log(cellhost, "Will send " + tosend + " to FIFO")

    # Send message to tester host
    output_stream.write(tosend + "\0")
    output_stream.flush()
    debug_log(cellhost, "Sent " + tosend + " to FIFO")

    # Prepare for next run
    # next_low_limit = next_low_limit + 1
    # if next_low_limit > 4:
    #     next_low_limit = 0

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_END', methods=['POST'])
def handle_TEST_END(cellhost, lotid, sublotid):
    ''' Test end management. Computes and send the next low limit. '''        
    debug_log(cellhost, "TEST_END", lotid, sublotid)
    x = threading.Thread(target=send_next_step_to_tp, args=(cellhost,))
    x.start()
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/SUBLOT_END', methods=['POST'])
def handle_SUBLOT_END(cellhost, lotid, sublotid):
    ''' TEST END management '''
    debug_log(cellhost, "SUBLOT_END", lotid, sublotid) 
    return ('', 200)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001)
    output_stream.close()
