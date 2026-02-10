import json
import datetime

class AdaptiveCommandsKeyNames():
    ''' List of the adaptive command key names for the JSON object sent to the RabbitMQ host '''

    TIME_STAMP = "TIME_STAMP"
    DAS_IDENTIFIER = "DAS_IDENTIFIER"
    TARGET = "TARGET"
    DEFAULT_TARGET = "ATE_SOFTWARE"

    ACTION = "ACTION"
    
    CLEAR = "CLEAR"

    ACTION_ENABLE = "ENABLE"
    ACTION_DISABLE = "DISABLE"

    ENABLEWORDS = "ENABLEWORDS"
    ENABLEWORD_ACTIONS = "ENABLEWORD_ACTIONS"
    
    TSTEP_NAMES = "TESTSTEP_NAMES"
    TSTEP_NUMS = "TESTSTEP_NUMBERS"
    TSTEP_ACTIONS = "TESTSTEP_ACTIONS"

    LIMIT_NAMES = "LIMIT_NAMES"
    LIMIT_NUMBERS = "LIMIT_NUMBERS"
    LO_LIMIT = "LO_LIMIT"
    HI_LIMIT = "HI_LIMIT"
    LLM_SCALE = "LLM_SCALE"
    HLM_SCALE = "HLM_SCALE"
    LIMITS = "LIMITS"
    UPDATE_LIMITS = "UPDATE_LIMITS"

class AdaptiveBaseCommand():
    ''' Common elements to all adaptive commands '''    

    def __init__(self, DAS_URL):
        '''
        Constructor.
        :param str DAS_URL: identifier sent to the RabbitMQ host for traceability. 
        It is meant to be the URL of the DAS sending the adaptive commands.
        '''
        self.time_stamp = f"{datetime.datetime.now().isoformat()}"
        self.das_id = DAS_URL
        self.target = AdaptiveCommandsKeyNames.DEFAULT_TARGET

    def create_base(self):
        ''' Base command with the fields common to all commands '''
        return {
            AdaptiveCommandsKeyNames.TIME_STAMP: self.time_stamp,
            AdaptiveCommandsKeyNames.DAS_IDENTIFIER: self.das_id,
            AdaptiveCommandsKeyNames.TARGET: self.target
        }

class EnableWordCommand():
    ''' Adaptive command targeting enable words '''

    def __init__(self, DAS_URL):
        '''
        Constructor.
        :param str DAS_URL: identifier sent to the RabbitMQ host for traceability. 
        It is meant to be the URL of the DAS sending the adaptive commands.
        '''
        self.das_id = DAS_URL
        self.actions = []

    def activate_enable_words(self, enablewords: list):
        '''
        This function generates a command enabling a list of IGXL enable words.
        :param list enablewords: list of enable words strings to activate.
        '''
        return { 
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_ENABLE,
            AdaptiveCommandsKeyNames.ENABLEWORDS: enablewords
        }

    def deactivate_enable_words(self, enablewords: list):
        '''
        This function generates a command disabling a list of IGXL enable words.
        :param list enablewords: list of enable words strings to deactivate.
        '''
        return { 
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_DISABLE,
            AdaptiveCommandsKeyNames.ENABLEWORDS: enablewords
        }

    def add_action(self, action):
        '''
        Add an enable word action to the command
        :param dict action: enable word action to add to the list of actions for this command.
        '''
        self.actions.append(action)
    
    def create_command(self):
        ''' Creates a JSON adaptive command string using the recorded actions '''
        command = AdaptiveBaseCommand(self.das_id).create_base()
        if self.actions.count == 0:
            return ""
        
        command.update( { AdaptiveCommandsKeyNames.ENABLEWORD_ACTIONS: self.actions } )
        return json.dumps(command)

class TestControlCommand():
    ''' Adaptive command to enable/disable tests '''

    def __init__(self, DAS_URL):
        '''
        Constructor.
        :param str DAS_URL: identifier sent to the RabbitMQ host for traceability. 
        It is meant to be the URL of the DAS sending the adaptive commands.
        '''
        self.das_id = DAS_URL
        self.actions = []

    def enable_testnames(self, tnames: list):
        '''
        Enable a test using its name.
        :param list tnames: List of test names to enable
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_ENABLE,
            AdaptiveCommandsKeyNames.TSTEP_NAMES: tnames
        }

    def enable_testnumbers(self, tnums: list):
        '''
        Enable a test using its number.
        :param list tnums: List of test numbers to enable
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_ENABLE,
            AdaptiveCommandsKeyNames.TSTEP_NUMS: tnums
        }

    def disable_testnames(self, tnames: list):
        '''
        Disable a test using its name
        :param list tnames: List of test names to disable
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_DISABLE,
            AdaptiveCommandsKeyNames.TSTEP_NAMES: tnames
        }

    def disable_testnumbers(self, tnums: list):
        '''
        Disable a test using its number.
        :param list tnums: List of test numbers to disable
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.ACTION_DISABLE,
            AdaptiveCommandsKeyNames.TSTEP_NUMS: tnums
        }
    
    def add_action(self, action):
        '''
        Add a test enabling/disabling action to the command.
        :param dict action: test step action to add to the list of actions for this command.
        '''
        self.actions.append(action)

    def create_command(self):
        ''' Creates a JSON adaptive command string using the recorded actions. '''
        command = AdaptiveBaseCommand(self.das_id).create_base()
        if self.actions.count == 0:
            return ""
        
        command.update( { AdaptiveCommandsKeyNames.TSTEP_ACTIONS: self.actions } )
        return json.dumps(command)

class UpdateLimitsCommand():
    ''' Adaptive command to update limits for given tests '''

    def __init__(self, DAS_URL):
        '''
        Constructor.
        :param str DAS_URL: identifier sent to the RabbitMQ host for traceability. 
        It is meant to be the URL of the DAS sending the adaptive commands.
        '''
        self.das_id = DAS_URL
        self.actions = []

    def limits_command_with_names(self, limits: list, lo_lim: float, hi_lim: float, llm_scale: int = None, hlm_scale: int = None):
        '''
        Creates a portion of a limit command focused on a specific limit update.        
        :param list limits: list of names as found in the "Use-Limit" IGXL flow entries.
        :param float lo_lim: new low limit
        :param float hi_lim: new high limit
        :param int llm_scale: scaling value for the low limit (0 = no scaling)
        :param int hlm_scale: scaling value for the high limit (0 = no scaling)
        '''

        limits_cmd = {
            AdaptiveCommandsKeyNames.LIMIT_NAMES: limits,
            AdaptiveCommandsKeyNames.LO_LIMIT: lo_lim,
            AdaptiveCommandsKeyNames.HI_LIMIT: hi_lim
        }

        if (llm_scale is not None): 
            limits_cmd.update({ AdaptiveCommandsKeyNames.LLM_SCALE: llm_scale })
        if (hlm_scale is not None): 
            limits_cmd.update({ AdaptiveCommandsKeyNames.HLM_SCALE: hlm_scale })

        return limits_cmd

    def limits_command_with_numbers(self, limits: list, lo_lim: float, hi_lim: float, llm_scale: int = None, hlm_scale: int = None):
        '''
        Creates a portion of a limit command focused on a specific limit update.        
        :param list limits: list of numbers as found in the "Use-Limit" IGXL flow entries.
        :param float lo_lim: new low limit
        :param float hi_lim: new high limit
        :param int llm_scale: scaling value for the low limit (0 = no scaling)
        :param int hlm_scale: scaling value for the high limit (0 = no scaling)
        '''

        limits_cmd = {
            AdaptiveCommandsKeyNames.LIMIT_NUMBERS: limits,
            AdaptiveCommandsKeyNames.LO_LIMIT: lo_lim,
            AdaptiveCommandsKeyNames.HI_LIMIT: hi_lim
        }

        if (llm_scale is not None): 
            limits_cmd.update({ AdaptiveCommandsKeyNames.LLM_SCALE: llm_scale })
        if (hlm_scale is not None): 
            limits_cmd.update({ AdaptiveCommandsKeyNames.HLM_SCALE: hlm_scale })

        return limits_cmd

    def update_limits_with_testnames(self, tnames: list, limits_cmd: dict):
        '''
        Update limits for a list of test names.
        :param list tnames: list of test names on which to update the limits.
        :param dict limits_cmd: update limits command to apply the list of test names.
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.UPDATE_LIMITS,
            AdaptiveCommandsKeyNames.TSTEP_NAMES: tnames,
            AdaptiveCommandsKeyNames.LIMITS: limits_cmd
        }

    def update_limits_with_testnumbers(self, tnums: list, limits_cmd: dict):
        '''
        Update limits for a list of test numbers.
        :param list tnums: list of test number on which to update the limits.
        :param dict limits_cmd: update limits command to apply the list of test number.
        '''
        return {
            AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.UPDATE_LIMITS,
            AdaptiveCommandsKeyNames.TSTEP_NUMS: tnums,
            AdaptiveCommandsKeyNames.LIMITS: limits_cmd
        }
    
    def add_action(self, action):
        '''
        Add an enable word action to the command.
        :param dict action: single update limits action to perform.
        '''
        self.actions.append(action)

    def create_command(self):
        ''' Creates a JSON adaptive command string using the recorded actions '''
        command = AdaptiveBaseCommand(self.das_id).create_base()
        if self.actions.count == 0:
            return ""
        
        command.update( { AdaptiveCommandsKeyNames.TSTEP_ACTIONS: self.actions } )
        return json.dumps(command)

class ClearCommand():

    def __init__(self, DAS_URL):
        '''
        Constructor.
        :param str DAS_URL: identifier sent to the RabbitMQ host for traceability. 
        It is meant to be the URL of the DAS sending the adaptive commands.
        '''
        self.das_id = DAS_URL
    
    def create_command(self):
        ''' Creates the JSON adaptive command string to clear previous commands. '''
        command = AdaptiveBaseCommand(self.das_id).create_base()       
        command.update( { AdaptiveCommandsKeyNames.ACTION: AdaptiveCommandsKeyNames.CLEAR } )
        return json.dumps(command)

# class AdaptiveCommand():
#     ''' Adaptive command information to be sent in JSON format '''    

#     USE_NAMES = True
#     USE_NUMBERS = False

#     def __init__(self, url):
#         self.das_url = url

#     # def create_base_command(self):
#     #     ''' Base command with the common fields '''
#     #     return {
#     #         "TIME_STAMP":f"{datetime.datetime.now().isoformat()}",
#     #         "DAS_IDENTIFIER":f"{self.das_url}",
#     #         "TARGET": "ATE_SOFTWARE"
#     #     }

#     # def create_enableword_actions(self, actions: list):
#     #     command = self.create_base_command()        
#     #     command.update( { "ENABLEWORD_ACTIONS": actions } )
#     #     return json.dumps(command)

#     # def create_enableword_action(self, action: str, enablewords: list):
#     #     return { "ACTION": action, "ENABLEWORDS": enablewords }

#     # def create_teststep_actions(self, actions: list):
#     #     ''' TestSteps action to enable/disable tests and update limits '''
#     #     command = self.create_base_command()        
#     #     command.update( { "TESTSTEP_ACTIONS": actions } )
#     #     return json.dumps(command)

#     # def create_action(self, action_name: str, tsteps: list = None, withnames: bool = True, limits: dict = None):
#     #     ''' Creation of one type of action: CLEAR/ENABLE/DISABLE/UPDATE_LIMITS '''
#     #     ''' No check on the validity of the action provided '''

#     #     action = { "ACTION": f"{action_name}" }

#     #     if tsteps is not None: 
#     #         action.update( { "TESTSTEP_NAMES" if withnames else "TESTSTEP_NUMBERS": tsteps } )
        
#     #     if limits is None: return action

#     #     action.update( { "LIMITS": limits } )
#     #     return action
    
#     # def create_limits(self, limits: list, withnames: bool, lo_lim: float, hi_lim: float, llm_scale: int = None, hlm_scale: int = None):
#     #     limits_cmd = {
#     #         "LIMIT_NAMES" if withnames else "LIMIT_NUMBERS": limits,
#     #         "LO_LIMIT": lo_lim,
#     #         "HI_LIMIT": hi_lim,            
#     #     }

#     #     if (llm_scale is not None): limits_cmd.update({ "LLM_SCALE": llm_scale })
#     #     if (hlm_scale is not None): limits_cmd.update({ "HLM_SCALE": hlm_scale })

#     #     return limits_cmd

