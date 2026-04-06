# 🛒 E-Commerce Backend API

## 📝 Description
A robust, highly scalable RESTful API built with **.NET 8** to power a modern e-commerce platform. This project serves as the backend engine for managing product catalogs, secure user authentication, shopping carts, and a transactional order checkout system.

## ✨ Key Features
* **Secure Identity:** Full JWT (JSON Web Token) authentication and Role-Based Access Control (RBAC) for Admins and regular Users.
* **Shopping Cart Engine:** Live inventory validation, dynamic total calculation, and state management.
* **Transactional Checkout:** Converts carts to complete orders, locks in pricing, deducts from master inventory, and clears cart state in a single transaction.
* **Optimized Data Delivery:** High-performance cursor pagination (`Skip` and `Take`) with X-Pagination metadata headers.
* **Advanced Querying:** Dynamic search, filtering, and sorting capabilities built over Entity Framework Core.
* **Interactive Documentation:** Fully documented endpoints using Swagger/OpenAPI with XML code summaries.

## 🛠️ Tech Stack
* **Framework:** .NET 8 / C# 12
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **Authentication:** JWT Bearer Tokens / BCrypt Password Hashing
* **Architecture:** N-Tier Architecture (Controllers, Services, DTOs, Data/Models)

## 🚀 Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* SQL Server (LocalDB or standard)
