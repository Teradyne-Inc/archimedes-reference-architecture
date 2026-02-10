using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Teradyne.FA.DIA.DAS.Adaptive
{
    /// <summary>
    /// This class manages RabbitMQ communication to send adaptive request to the tester host.
    /// </summary>
    public class AdaptiveCommunication
    {
        public const int RMQ_PORT = 5672;
        public const string RMQ_SCHEME = "amqp://";
        public const string RMQ_USER = "AMP-adaptive";
        public const string RMQ_PWD = "TER-ADAPTIVE-P4SSW0RD";
        public const string RMQ_VHOST = "TER-AMP-vhost";
        public const string RMQ_EXCHANGE = "AMP_DAS_Exchange";
        public const string RMQ_QUEUE = "AMP_DAS_Queue";
        public const string RMQ_ROUTING = "AMP_DAS_RoutingKey";
        public const string RMQ_CLIENTNAME = "AMP DAS RabbitMQ Sender";

        protected string rmqServerIP = default!;
        protected ConnectionFactory connectionFactory = default!;
        protected IConnection m_connection = default!;
        protected IModel m_channel = default!;
        protected string m_replyQueueName = default!;
        protected BlockingCollection<string> m_respQueue = default!;
        protected IBasicProperties m_basicProperties = default!;
        protected EventingBasicConsumer m_consumer = default!;



        public AdaptiveCommunication(string serverIP)
        {
            rmqServerIP = serverIP;
            m_respQueue = new BlockingCollection<string>();
        }

        protected bool InitConnection()
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory();                
                factory.Uri = new Uri($"{RMQ_SCHEME}{RMQ_USER}:{RMQ_PWD}@{rmqServerIP}:{RMQ_PORT}");
                factory.ClientProvidedName = RMQ_CLIENTNAME;
                factory.VirtualHost = RMQ_VHOST;                

                m_connection = factory.CreateConnection();

                m_channel = m_connection.CreateModel();
                m_channel.ExchangeDeclare(RMQ_EXCHANGE, ExchangeType.Direct);
                m_channel.QueueDeclare(RMQ_QUEUE, false, false, false, null);
                m_channel.QueueBind(RMQ_QUEUE, RMQ_EXCHANGE, RMQ_ROUTING, null);
                m_replyQueueName = m_channel.QueueDeclare().QueueName;
                m_consumer = new EventingBasicConsumer(m_channel);
                Console.WriteLine("Communication Initialized");
                return true;
            }
            catch (Exception ex) {  return false; }
        }        
        
        /// <summary>
        /// Send an Adaptive Action to the AMP server.
        /// </summary>
        /// <param name="message">Adaptive Action to send. Must be in properly-formatted JSON to be processed correctly.</param>
        /// <returns>Response from AMP as a string.</returns>
        /// <remarks>
        /// See documentation for formatting details.
        /// Will throw an InvalidOperationException if the connection is not open.
        /// </remarks>
        public string SendAction(string message)
        {
            try
            {
                Console.WriteLine("Sending Action [" + message + "]");
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                // TODO: channel is not thread-safe. Worth it to lock here? https://stackoverflow.com/questions/12024241/c-sharp-rabbitmq-client-thread-safety
                m_channel.BasicPublish(
                    exchange: "",
                    routingKey: RMQ_QUEUE, // TODO: does routing key here not need to match the one in InitConnection? this is working.                   
                    mandatory: true,
                    body: messageBytes,
                    basicProperties: m_basicProperties);

                m_channel.BasicConsume(
                    consumer: m_consumer,
                    queue: m_replyQueueName,
                    autoAck: true);

             
                string  resp=m_respQueue.Take();
                Console.WriteLine("Received Response [" + resp + "]");
                return resp;
                
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Cannot send or read the response");
                Console.WriteLine(ex.Message); 
                return default!; 
            }
        }

        /// <summary>
        /// Send an Adaptive Action to the AMP server.
        /// </summary>
        /// <param name="message">Adaptive Action to send. Must be in properly-formatted JSON to be processed correctly.</param>
        /// <returns>Response from AMP as a string.</returns>
        /// <remarks>
        /// See documentation for formatting details.
        /// Will throw an InvalidOperationException if the connection is not open.
        /// </remarks>
        public string SendAction(AdaptiveCommand command)
        {
            return SendAction(command.toJSON());
        }

        /// <summary>
        /// Asynchronously send an Adaptive Action to the AMP server.
        /// </summary>
        /// <param name="message">Adaptive Action to send. Must be in properly-formatted JSON to be processed correctly.</param>
        /// <returns>Response from AMP as a string.</returns>
        /// <remarks>
        /// See documentation for formatting details.
        /// Will throw an InvalidOperationException if the connection is not open.
        /// </remarks>
        public async Task<string> SendActionAsync(string message)
        {
            return await Task.Run(() =>
            {
               return SendAction(message);
            });
        }


        /// <summary>
        /// Asynchronously send an Adaptive Action to the AMP server.
        /// </summary>
        /// <param name="message">Adaptive Action to send. Must be in properly-formatted JSON to be processed correctly.</param>
        /// <returns>Response from AMP as a string.</returns>
        /// <remarks>
        /// See documentation for formatting details.
        /// Will throw an InvalidOperationException if the connection is not open.
        /// </remarks>
        public async Task<string> SendActionAsync(AdaptiveCommand command)
        {
            return await Task.Run(() => SendAction(command.toJSON()));
        }

        protected void InitBasicProperties()
        {
            try
            {
                m_basicProperties = m_channel.CreateBasicProperties();
                string correlationId = Guid.NewGuid().ToString();
                m_basicProperties.CorrelationId = correlationId;
                m_basicProperties.ReplyTo = m_replyQueueName;
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); m_basicProperties = default!; }
        }

        protected void InitConsumerReceived()
        {
            m_consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    string response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == m_basicProperties.CorrelationId)
                    {
                        m_respQueue.Add(response);
                    }
                }
                catch(Exception ex) { Console.WriteLine(ex.Message); }
            };
        }


        /// <summary>
        /// Open a connection to the RabbitMQ server.
        /// </summary>
        public bool Open()
        {
            try
            {
                Console.WriteLine("Opening Communication");
                if (!InitConnection()) return false;
                InitBasicProperties();
                InitConsumerReceived();
                Console.WriteLine("Communication Opened");
                return true;
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Cannot open the communication");
                Console.WriteLine(ex.Message);
                return false; 
            } 
        }

        /// <summary>
        /// Close the connection to the RabbitMQ server if it is open.
        /// </summary>
        /// <remarks>
		/// Attempting to call SendAction(AdaptiveControlCommandBase)
        /// SendAction(AdaptiveControlCommandBase)
		/// after closure will result in undefined behavior.
		/// </remarks>
        public void Close()
        {
            try
            {
                m_channel.Close();               
                m_connection.Close();
                Console.WriteLine("Communication Closed");
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Cannot closed the communication");
                Console.WriteLine(ex.Message);
            }
        }

    }
}
