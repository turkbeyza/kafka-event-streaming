using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Order.Producer;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Kafka Connection and Configuration Settings
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            
            // ASSIGNMENT REQUIREMENT 1: IDEMPOTENCY
            // When this setting is true, Kafka prevents a message from being 
            // written twice by mistake even if the network fails (Exactly-Once semantics).
            // This is vital in e-commerce to avoid charging a customer twice!
            EnableIdempotence = true, 
            Acks = Acks.All // Acks.All is mandatory for Idempotency to function.
        };

        // Creating the Producer instance
        using var producer = new ProducerBuilder<string, string>(config).Build();
        
        Console.WriteLine("=== E-Commerce Order System (PRODUCER) ===");
        Console.WriteLine("System Ready. Leave empty and press Enter to exit.\n");

        int orderIdCounter = 1000; // Initial sample order ID counter

        while (true)
        {
            Console.Write("Enter the Customer ID (e.g., user123): ");
            var userId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userId)) break;

            // Sample order object (Event Data)
            var orderEvent = new 
            { 
                OrderId = ++orderIdCounter, 
                UserId = userId, 
                Product = "Apple Airpods Pro 2", 
                Amount = 1499.99,
                OrderDate = DateTime.UtcNow
            };

            // Serializing the object to JSON format
            string orderJson = JsonSerializer.Serialize(orderEvent);

            // ASSIGNMENT REQUIREMENT 2: PARTITIONS & KEYS
            // We assign the Customer ID to the 'Key' of the message.
            // Kafka always writes messages with the same Key to the same Partition.
            // This ensures that the order of messages for the same customer is strictly preserved.
            var message = new Message<string, string> 
            { 
                Key = userId, 
                Value = orderJson 
            };

            try
            {
                // Sending the message to the 'orders' topic
                var deliveryResult = await producer.ProduceAsync("orders", message);
                
                Console.WriteLine($"\n[SUCCESS] Order Event dispatched to Kafka!");
                Console.WriteLine($"-> Topic: {deliveryResult.Topic}");
                Console.WriteLine($"-> Target Partition: {deliveryResult.Partition}"); // Shows which partition it landed in
                Console.WriteLine($"-> Offset (Sequence No): {deliveryResult.Offset}\n");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"[ERROR] Order could not be delivered: {e.Error.Reason}");
            }
        }
    }
}