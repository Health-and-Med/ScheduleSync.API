using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ScheduleSync.Application.Services
{
    public class RabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            //var factory = new ConnectionFactory()
            //{
            //    HostName = "localhost",
            //    Port = 5672,
            //    UserName = "guest",
            //    Password = "guest"
            //};

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "new_appointment",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public void PublishNewAppointment(string doctorEmail, string patientEmail, string body)
        {
            var notification = new
            {
                DoctorEmail = doctorEmail,
                PatientEmail = patientEmail,
                Body = body
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification));

            _channel.BasicPublish(exchange: "",
                                 routingKey: "new_appointment",
                                 basicProperties: null,
                                 body: messageBody);

            Console.WriteLine($"Notificação enviada para {doctorEmail}!");
        }
    }
}
