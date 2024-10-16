
# Automated SQL Report Generator with OpenAI API

This project demonstrates how to generate SQL queries and execute them on a relational database using the OpenAI API. The code is designed to automatically create reports based on natural language descriptions, making it easier for non-technical users to request data without needing to understand SQL.

## Features

- **Natural Language Input**: Users can provide a description of the report they need in plain language.
- **SQL Generation**: The OpenAI API generates a SQL query based on the user's request.
- **Database Initialization**: Sets up example tables (`Customers` and `Transactions`) and populates them with sample data.
- **Query Execution**: Executes the generated SQL query and exports the results to a CSV file.
- **Configurable Database**: Uses a local database (`DebugDatabase`) to simulate a real-world scenario.

## Prerequisites

- .NET SDK (version 6.0 or higher)
- SQL Server (LocalDB instance)
- An OpenAI API key

## Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/YourUsername/ReportGenerator.git
   cd ReportGenerator
   ```

2. **Configure the OpenAI API Key**
   - Set your OpenAI API key in the code:
     ```csharp
     string apiKey = "your-openai-api-key-here";
     ```

3. **Run the Application**
   - Use the following command to run the console application:
     ```bash
     dotnet run
     ```

## How It Works

### 1. Report Request Setup

The application starts by taking a user-provided description of the desired report. This description is used to create a prompt that instructs the OpenAI API to generate a corresponding SQL query.

```csharp
string reportRequest = "Eu quero um relatório com todas as transações de feitas em Fevereiro de 2015 com valor entre 10.00 e 150.00";
string columns = "Id do cliente, telefone, e-mail, id da transação e status da transação";
```

### 2. Pre-Prompt for SQL Query Generation

The pre-prompt guides the API on how to interpret the user's request, including term mapping and instructions on handling filters like date ranges and customer identifiers.

### 3. Database Initialization

The program creates the `Customers` and `Transactions` tables if they do not exist and populates them with sample data.

### 4. Query Execution and Export to CSV

The generated SQL query is executed against the local database, and the results are exported to a CSV file on the desktop.

## Example Tables

The project creates the following tables:

### `Customers` Table

| Column        | Data Type         | Description                          |
|---------------|-------------------|--------------------------------------|
| `CustomerID`  | INT (Primary Key) | Unique identifier for each customer  |
| `FullName`    | NVARCHAR(100)     | Customer's full name                 |
| `Email`       | NVARCHAR(100)     | Customer's email                     |
| `PhoneNumber` | NVARCHAR(15)      | Customer's phone number              |
| `DateOfBirth` | DATE              | Date of birth                        |

### `Transactions` Table

| Column           | Data Type         | Description                                |
|------------------|-------------------|--------------------------------------------|
| `TransactionID`  | INT (Primary Key) | Unique identifier for each transaction     |
| `CustomerID`     | INT (Foreign Key) | References `Customers(CustomerID)`         |
| `TransactionDate`| DATETIME          | Date of the transaction                    |
| `Amount`         | DECIMAL(18, 2)    | Transaction amount                         |
| `PaymentMethod`  | NVARCHAR(50)      | Method of payment                          |
| `Status`         | NVARCHAR(20)      | Status of the transaction                  |

## Error Handling

The code includes error handling for various stages:
- Initialization of the database
- Executing the generated SQL query
- Exporting the results to a CSV file

## Points to Consider

- **API Costs**: Using the OpenAI API may incur costs, depending on the number of requests.
- **Security**: Make sure to keep the API key secure and avoid exposing it in public repositories.
- **Scalability**: This project is designed for small-scale use cases. Adjustments may be needed for larger databases or more complex queries.
  
--- 
