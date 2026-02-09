from AMP_adaptive_command import *
# from AMP_adaptive_rmqclient import RabbitMQAdaptiveActionSender

def main():
    DAS_NAME = "AdaptiveDAS"
    AMPDAS_PORT = 5002
    DAS_URL = f"http://10.100.100.1:{AMPDAS_PORT}/{DAS_NAME}/"
#    TESTER_IP_ADDRESS = "10.100.100.126"

    # Add warning saying commands are not actually sent anywhere...

#    rmq_sender = RabbitMQAdaptiveActionSender(TESTER_IP_ADDRESS)
#    rmq_sender.open()

    # Enable two tests identified by the names: test1 and test2
    # Disable two tests identified by the numbers: 100 and 200    

    cmd = TestControlCommand(DAS_URL)
    cmd.add_action(cmd.enable_testnames(["test1", "test2" ]))
    cmd.add_action(cmd.disable_testnumbers([100, 200]))
    adaptive_command = cmd.create_command()
    print(adaptive_command)
#    rmq_sender.send_action(adaptive_command)

    # Update the following limits
    # - On tests identified by numbers 100, 200, and 300,
    #   set limit names limitsSubA and limitsSubB to low limit 1.2 and high limit 3.4 with no scaling
    # - On test identified by the name test1,
    #   set limit numbers 1001, 1002, and 1003 to low limit 20.34 and high limit 50.24 with scaling 3 (milli) on both limits

    cmd = UpdateLimitsCommand(DAS_URL)
    cmd.add_action(
        cmd.update_limits_with_testnumbers(
            [100,200,300],
            cmd.limits_command_with_names(["limitsSubA", "limitsSubB"], 1.2, 3.4)))
    cmd.add_action(
        cmd.update_limits_with_testnames(
            ["test1"],
            cmd.limits_command_with_numbers([1001, 1002, 1003], 20.34, 50.24, 3, 3)))
    adaptive_command = cmd.create_command()
    print(adaptive_command)
#    rmq_sender.send_action(adaptive_command)
    
    # Create and send the CLEAR command

    cmd = ClearCommand(DAS_URL)    
    adaptive_command = cmd.create_command()
    print(adaptive_command)
#    rmq_sender.send_action(adaptive_command)

    # Enable word activation
    # - Enable the following enable words: EWS1 and EWS2
    # - Disable the following enable word: EWS3

    cmd = EnableWordCommand(DAS_URL)
    cmd.add_action(cmd.activate_enable_words([ "EWS1", "EWS2" ]))
    cmd.add_action(cmd.deactivate_enable_words(["EWS3"]))
    adaptive_command = cmd.create_command()
    print(adaptive_command)
#    rmq_sender.send_action(adaptive_command)

#    rmq_sender.close()

if __name__ == '__main__':
    main()