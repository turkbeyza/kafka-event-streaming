# Event-Driven Architecture with Apache Kafka: E-Commerce Demo

## 📌 Project Overview

In traditional monolithic systems, processes like saving an order and updating inventory happen synchronously, which can create bottlenecks. This project solves that by using **Kafka as an Event Streaming Platform**. 

When a user places an order, the `Order.Producer` service simply publishes an `OrderPlaced` event to Kafka and finishes its job. The `Inventory.Consumer` service listens to this stream in real-time and processes the stock update independently.

## 🚀 Key Concepts Demonstrated (Assignment Requirements)

* **Event Streaming:** Real-time, decoupled communication between a Producer and a Consumer using Kafka Topics (`orders`).
* **Partitions & Keys:** The `orders` topic is divided into 3 partitions. By using the `UserId` as the Message Key, Kafka guarantees that all orders from the same user always route to the same partition, ensuring strict chronological processing order.
* **Consumers & Offsets:** The Inventory Service pulls data at its own pace. It tracks its position using `Offsets`. If the consumer crashes, it resumes from its last committed offset, preventing data loss.
* **Idempotency (Exactly-Once Semantics):** To prevent duplicate orders (e.g., double-charging a customer due to a network retry), the Producer is configured with `EnableIdempotence = true`. Kafka assigns unique PIDs to messages to safely ignore accidental duplicates.

## 🏗️ Architecture & Technologies

* **C# / .NET 9:** Used for building the Producer (Console/API) and Consumer (Console) applications.
* **Apache Kafka & Zookeeper:** Running locally via Docker to act as the central message broker.
* **Confluent.Kafka:** The official .NET client library for Kafka.

## 🛠️ How to Run the Demo

### 1. Start the Kafka Infrastructure
Make sure Docker Desktop is running. Navigate to the folder containing the `docker-compose.yml` file and run:
```bash
docker-compose up -d
