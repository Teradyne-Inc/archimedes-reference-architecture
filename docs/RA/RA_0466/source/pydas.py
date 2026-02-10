#!/usr/local/bin
'''
PyDAS application file to be run in a docker on the UltraEdge
'''

import logging

from flask import Flask, request

fifo_output = "/app/exec/fifo_to_TP"
output_stream = open(fifo_output, 'w')

# To avoid printing messages on the console
log = logging.getLogger('werkzeug')
log.setLevel(logging.ERROR)

# Flask app
app = Flask(__name__)

class TestData():
    ''' Class to record data for one test '''
    def __init__(self):
        self.test_num = None
        self.site_num = None
        self.result = None
        self.lo_lim = None
        self.hi_lim = None

    def extract_data(self, json_data):
        ''' From an AMP json object, extract some relevant parametric test data '''
        self.test_num = json_data["TEST_NUMBER"]
        self.site_num = json_data["SITE_NUM"]
        self.result = json_data["RESULT"]
        self.lo_lim = json_data["LO_LIM"]
        self.hi_lim = json_data["HI_LIM"]

    def to_string(self):
        ''' Stringify the test data '''
        return f"[S:{self.site_num},TN:{self.test_num},R:{self.result},LLM:{self.lo_lim},HLM:{self.hi_lim}]"

class TestStart():
    ''' Class to record the start status of one site for the current touchdown '''

    def __init__(self):
        self.site_num = None
        self.status = None

    def extract_data(self, json_data):
        ''' From an AMP json object, extract the starting status and site number '''        
        self.site_num = json_data["SITE_NUM"]
        self.status = json_data["STATUS"]

    def to_string(self):
        ''' Stringify the test start information '''
        return f"[SITE:{self.site_num},STATUS:{self.status}]"

class TestEnd():
    ''' Class to record the testing status of one site for the current touchdown '''

    def __init__(self):
        self.site_num = None
        self.pf = None
        self.sbin = None
        self.hbin = None
        self.partid = None

    def extract_data(self, json_data):
        ''' From an AMP json object, extract the site status information at the end of test '''        
        self.site_num = json_data["SITE_NUM"]
        self.pf = json_data["PASS_FAIL"]
        self.sbin = json_data["SOFT_BIN"]
        self.hbin = json_data["HARD_BIN"]
        self.partid = json_data["PART_ID"]

    def to_string(self):
        ''' Stringify the test end information '''
        return f"[SITE:{self.site_num},PF:{self.pf},SBIN:{self.sbin},HBIN:{self.hbin},PART:{self.partid}]"

# Touchdowns counter
counter = 0

# Log filename
current_log_filename = f"LoggedMessages_{counter}.txt"
log_folder = "/app/exec"

def log_information(msg):
    ''' Simple file output function '''
    with open(f"{log_folder}/{current_log_filename}", "a", -1, "UTF-8") as log_file:
        log_file.write(f"{msg}\n")

def log_message_data(message, listname, classname, title, lotid, sublotid):
    ''' Generic function to extract data from a message and save to a file '''
    lst = []
    data = message[listname]
    if len(data) > 0:
        for jsonitem in data:
            obj = classname()
            obj.extract_data(jsonitem)
            lst.append(obj)
    lst_str = ','.join([obj.to_string() for obj in lst])
    log_information(f"[LOT:{lotid}][SUBLOT:{sublotid}]{title}({lst_str})")

@app.route('/', defaults={'path': ''}, methods=['POST'])
@app.route('/<path:path>', methods=['POST'])
def handle_default(path):
    ''' 
    Flask (HTTP server) handling function for the default route. Skip AMP messages we are not interested in.
    
    The returned value '200' corresponds to the Success status in the HTTP protocol.
    '''
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_START', methods=['POST'])
def handle_TEST_START(cellhost, lotid, sublotid):
    ''' 
    Flask (HTTP server) handling function for the TEST_START AMP message.

    Data is extracted and logged. Lot Id and Sublot Id are extracted from the URL and logged as well.
    
    The returned value '200' corresponds to the Success status in the HTTP protocol.
    '''
    message = request.get_json(force=True)
    log_message_data(message, "SITE_STATUS", TestStart, "TEST_START", lotid, sublotid)
    return ('', 200)

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_DATA', methods=['POST'])
def handle_TEST_DATA(cellhost, lotid, sublotid):
    ''' 
    Flask (HTTP server) handling function for the TEST_DATA AMP message.

    Data is extracted and logged. Lot Id and Sublot Id are extracted from the URL and logged as well.

    The returned value '200' corresponds to the Success status in the HTTP protocol.
    '''
    message = request.get_json(force=True)
    log_message_data(message, "PARAMETRIC_DATA", TestData, "TEST_DATA", lotid, sublotid)

    return ('', 200)

def update_output():
    ''' Update the touchdowns counter and send filename to retrieve if 10 touchdowns were achieved. '''
    global counter, current_log_filename
    counter = counter + 1
    log_information(f"Touchdown: {counter} completed")
    if (counter % 10 == 0):
        try:
            output_stream.write(current_log_filename)
            output_stream.flush()
        except Exception as ex:
            log_information(f"error output to FIFO {ex}")
        
        current_log_filename = f"LoggedMessages_{counter}.txt"    

@app.route('/PyDAS/TEST_CELL/<string:cellhost>/LOT/<string:lotid>/SUBLOT/<string:sublotid>/TEST_END', methods=['POST'])
def handle_TEST_END(cellhost, lotid, sublotid):
    ''' 
    Flask (HTTP server) handling function for the TEST_END AMP message.

    Data is extracted and logged. Lot Id and Sublot Id are extracted from the URL and logged as well.
    
    The returned value '200' corresponds to the Success status in the HTTP protocol.
    '''
    message = request.get_json(force=True)
    log_message_data(message, "SITE_STATUS", TestEnd, "TEST_END", lotid, sublotid)
    update_output()

    return ('', 200)

def main():
    ''' TOP LEVEL function: start the Flash HTTP server (variable 'app') on port 5001 '''
    global app
    app.run(host='0.0.0.0', port=5001)

if __name__ == '__main__':
    main()
