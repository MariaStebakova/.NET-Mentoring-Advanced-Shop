Cart Service Extensibility: The solution is designed with clean architecture principles to ensure easy extensibility.
New features like support for different databases (e.g., MongoDB instead of LiteDB), API exposure can be added with minimal changes.
Thanks to layered separation and dependency inversion, extensions typically require creating new services or repositories without modifying existing business logic.

Catalog Service Extensibility: The system is designed using Clean Architecture principles, allowing for clear separation of concerns. Business logic, data access, and infrastructure can be extended independently. You can easily introduce new features (e.g., additional services, rules, or endpoints), switch databases (SQL Server, SQLite), or integrate external APIs without changing core logic. Adding new entity types or modules follows existing patterns, keeping the cost of extension low and predictable.

