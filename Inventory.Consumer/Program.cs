using System;
using System.Text.Json;
using System.Threading;
using Confluent.Kafka;

namespace Inventory.Consumer;

class Program
{
    static void Main(string[] args)
    {
        // 1. Consumer Configuration Settings
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            
            // ASSIGNMENT REQUIREMENT 3: CONSUMERS & CONSUMER GROUPS
            // GroupId is crucial. If you run multiple Consumers with the same GroupId,
            // Kafka distributes partitions among these consumers (Load Balancing).
            // If a new service (e.g., Shipping Service) wants to read this data independently, 
            // we must provide it with a different GroupId (e.g., "shipping-group").
            GroupId = "inventory-group",
            
            // Start reading from the oldest (earliest) messages when the application runs for the first time
            AutoOffsetReset = AutoOffsetReset.Earliest 
        };

        // Creating the Consumer instance
        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        // Subscribing to the 'orders' topic
        consumer.Subscribe("orders");

        Console.WriteLine("=== Inventory Service (CONSUMER) Started ===");
        Console.WriteLine("Waiting for orders... (Press Ctrl+C to exit)\n");

        // Token for gracefully shutting down the application
        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; 
            cts.Cancel();
        };

        try
        {
            // Continuous listening loop (Event Streaming logic)
            while (true)
            {
                // Blocks here until a new message arrives
                var consumeResult = consumer.Consume(cts.Token);

                // Processing the incoming JSON data
                var messageValue = consumeResult.Message.Value;
                var messageKey = consumeResult.Message.Key;

                Console.WriteLine($"[ORDER RECEIVED] Customer: {messageKey}");
                Console.WriteLine($"-> Content: {messageValue}");
                Console.WriteLine($"-> Partition Read: {consumeResult.Partition.Value} | Offset: {consumeResult.Offset.Value}");
                Console.WriteLine("-> Updating inventory...\n");
            }
        }
        catch (OperationCanceledException)
        {
            // Exits the loop gracefully when Ctrl+C is pressed
        }
        finally
        {
            // Closes the consumer safely and commits the current offsets to Kafka
            consumer.Close();
        }
    }
}