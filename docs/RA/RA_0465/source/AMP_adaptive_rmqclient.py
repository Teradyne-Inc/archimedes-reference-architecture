import pika
import uuid
from threading import Lock

class RabbitMQAdaptiveActionSender:
    ''' RabbitMQ producer code to send adaptive commands in the form of JSON strings '''

    RABBITMQ_SCHEME = "amqp://"
    QUEUE_NAME = "AMP_DAS_Queue"
    EXCHANGE_NAME = "AMP_DAS_Exchange"
    ROUTING_KEY = "AMP_DAS_RoutingKey"
    CLIENT_PROVIDED_NAME = "AMP Python DAS"
    VHOST = "TER-AMP-vhost"
    EXCHANGE = ""
    RABBITMQ_PORT = 5672

    # below constants will no longer exist in Tier 1, but exist for Tier 0
    RABBITMQ_USERNAME = "AMP-adaptive"
    RABBITMQ_PASSWORD = "TER-ADAPTIVE-P4SSW0RD"

    def __init__(self, amp_server_address):
        '''
        Constructor
        :param str amp_server_address: RabbitMQ host IP address.
        '''
        self.m_amp_server_addr = amp_server_address
        self.m_is_open = False
        self.m_lock = Lock()
        self.m_response = None

    @property
    def is_open(self):
        ''' Is the connection open or not? '''
        return self.m_is_open

    def send_action(self, message):
        '''
        Main feature of this class.        
        :param str message: JSON adaptive command as created by one of the classes in the AMP_adaptive_command module.
        '''
        
        if not self.m_is_open:
            raise Exception("Cannot send action when connection is closed.")

        message_bytes = message.encode('utf-8')

        with self.m_lock:
            self.m_channel.basic_publish(
                exchange=self.EXCHANGE,
                routing_key=self.QUEUE_NAME,
                body=message_bytes,
                properties=self.m_basic_properties
            )

            def on_response(channel, method, properties, body):
                if properties.correlation_id == self.m_basic_properties.correlation_id:
                    self.m_response = body.decode('utf-8')
                    self.m_channel.stop_consuming()

            self.m_channel.basic_consume(
                queue=self.m_reply_queue_name,
                on_message_callback=on_response,
                auto_ack=True
            )

            self.m_channel.start_consuming()
            return self.m_response

    def open(self):
        ''' Initializes the communication '''
        self._init_connection()
        self._init_basic_properties()
        self._init_consumer_received()
        self.m_is_open = True

    def close(self):
        ''' Closes all RabbitMQ communication objects '''
        if not self.m_is_open:
            return
        if self.m_channel:
            self.m_channel.close()
        if self.m_connection:
            self.m_connection.close()
        self.m_is_open = False

    def _init_connection(self):
        ''' Initiates a Pika connection using the parameters defined at the top of this class '''
        credentials = pika.PlainCredentials(self.RABBITMQ_USERNAME, self.RABBITMQ_PASSWORD)
        parameters = pika.ConnectionParameters(
            self.m_amp_server_addr,
            self.RABBITMQ_PORT,
            self.VHOST,
            credentials,
            client_properties={'connection_name': self.CLIENT_PROVIDED_NAME}
        )
        self.m_connection = pika.BlockingConnection(parameters)
        self.m_channel = self.m_connection.channel()
        self.m_channel.exchange_declare(exchange=self.EXCHANGE_NAME, exchange_type='direct')
        self.m_channel.queue_declare(queue=self.QUEUE_NAME, durable=False)
        self.m_channel.queue_bind(queue=self.QUEUE_NAME, exchange=self.EXCHANGE_NAME, routing_key=self.ROUTING_KEY)
        self.m_reply_queue_name = self.m_channel.queue_declare(queue='', exclusive=True).method.queue

    def _init_basic_properties(self):
        ''' Utility for the Pika connection '''
        self.m_basic_properties = pika.BasicProperties(
            correlation_id=str(uuid.uuid4()),
            reply_to=self.m_reply_queue_name
        )

    def _init_consumer_received(self):
        ''' Defines a consumer function for the adaptive command acknowledgements '''
        def on_message(channel, method, properties, body):
            if properties.correlation_id == self.m_basic_properties.correlation_id:
                self.m_response = body.decode('utf-8')
                self.m_channel.stop_consuming()

        self.m_channel.basic_consume(queue=self.m_reply_queue_name, on_message_callback=on_message, auto_ack=True)