using OpenAI.Chat;
using System.ClientModel;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        #region Report Request Setup
        string reportRequest = "Eu quero um relatório com todas as transações de feitas em Fevereiro de 2015 com valor entre 10.00 e 150.00";
        string columns = "Id do cliente, telefone, e-mail, id da transação e status da transação";
        string apiKey = "";
        string model = "gpt-4o";
        #endregion

        #region SQL Prompt Setup
        string sqlPrompt = $@"
Generate a SQL Server query based on the following information. Return only the SQL query itself, with no extra text, explanations, or formatting:

This is the report request:

Report Request: ""{reportRequest}.""
Columns: ""{columns}""
";
        #endregion

        #region Table Definitions
        string customerTableDefinition = @"
IF OBJECT_ID('Customers', 'U') IS NULL
BEGIN
    CREATE TABLE Customers (
        CustomerID INT PRIMARY KEY,
        FullName NVARCHAR(100),
        Email NVARCHAR(100),
        PhoneNumber NVARCHAR(15),
        DateOfBirth DATE
    );
END
";

        string transactionTableDefinition = @"
IF OBJECT_ID('Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE Transactions (
        TransactionID INT PRIMARY KEY,
        CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
        TransactionDate DATETIME,
        Amount DECIMAL(18, 2),
        PaymentMethod NVARCHAR(50),
        Status NVARCHAR(20)
    );
END
";
        #endregion

        #region Chat Setup
        ChatMessage presetMessage = ChatMessage.CreateAssistantMessage(@$"

You are an AI assistant that generates SQL queries based on non-technical requests in Brazilian Portuguese. Below is a mapping of terms and descriptions:

Tables:

""Clientes"" (Portuguese) maps to ""Customers"" (SQL Table).
""Transações"" (Portuguese) maps to ""Transactions"" (SQL Table).
Columns in the Customers Table:

""ID do Cliente"" maps to ""CustomerID"".
""Nome Completo"" maps to ""FullName"".
""Email"" maps to ""Email"".
""Telefone"" maps to ""PhoneNumber"".
""Data de Nascimento"" maps to ""DateOfBirth"".
Columns in the Transactions Table:

""ID da Transação"" maps to ""TransactionID"".
""ID do Cliente"" maps to ""CustomerID"".
""Data da Transação"" maps to ""TransactionDate"".
""Valor"" maps to ""Amount"".
""Forma de Pagamento"" maps to ""PaymentMethod"".
""Status"" maps to ""Status"".
The goal is to create SQL queries based on non-technical user requests. The requests will be made in Brazilian Portuguese and should be translated into proper SQL syntax using the mappings above. The AI should interpret filtering conditions such as dates, statuses, and customer identifiers.

Important Points to Consider:

When the user specifies date ranges, use the TransactionDate column.
For status filtering, use the Status column.
For customer identification, use the CustomerID column.
The user may specify which columns should appear in the report. Use these column names in the SELECT clause, mapping them to their respective SQL column names.
Example Requests:

Request: ""Quero ver todas as transações de clientes que compraram acima de R$1000.""
Expected SQL: SELECT * FROM Transactions WHERE Amount > 1000;
Request: ""Eu quero um relatório que me traga todas as transações que estão com o status como 'PAGO' em 2023 do cliente com id 4.""
Expected SQL: SELECT c.CustomerID, c.Email, c.PhoneNumber, t.TransactionID, t.TransactionDate, t.Amount, t.PaymentMethod, t.Status FROM Customers c INNER JOIN Transactions t ON c.CustomerID = t.CustomerID WHERE t.Status = 'PAGO' AND YEAR(t.TransactionDate) = 2023 AND c.CustomerID = 4;
Given a non-technical description in Portuguese, generate the corresponding SQL query using the table and column mappings provided.

Tables definition:
{customerTableDefinition}

{transactionTableDefinition}

");

        ChatMessage userCommand = ChatMessage.CreateUserMessage(sqlPrompt);
        ChatClient chatClient = new ChatClient(model, apiKey);
        #endregion

        #region Database Initialization
        string connectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand createDbCommand = new SqlCommand("IF DB_ID('DebugDatabase') IS NULL CREATE DATABASE DebugDatabase;", connection);
            createDbCommand.ExecuteNonQuery();
        }

        connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=DebugDatabase;Integrated Security=true;";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(customerTableDefinition, connection);
            command.ExecuteNonQuery();
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(transactionTableDefinition, connection);
            command.ExecuteNonQuery();
        }
        #endregion

        #region Populate Tables with Sample Data
        string clearTables = @"
DELETE FROM Transactions;
DELETE FROM Customers;
";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(clearTables, connection))
            {
                command.ExecuteNonQuery();
            }
            Console.WriteLine("Existing data cleared.");
        }

        string insertCustomers = @"
INSERT INTO Customers (CustomerID, FullName, Email, PhoneNumber, DateOfBirth) VALUES
(1, 'Alice Johnson', 'alice.johnson@example.com', '123-456-7890', '1990-05-15'),
(2, 'Bob Smith', 'bob.smith@example.com', '234-567-8901', '1985-03-22'),
(3, 'Charlie Brown', 'charlie.brown@example.com', '345-678-9012', '1992-07-30'),
(4, 'David Wilson', 'david.wilson@example.com', '456-789-0123', '1980-12-10'),
(5, 'Emma Davis', 'emma.davis@example.com', '567-890-1234', '1995-08-05'),
(6, 'Frank Miller', 'frank.miller@example.com', '678-901-2345', '1988-02-14'),
(7, 'Grace Lee', 'grace.lee@example.com', '789-012-3456', '1993-11-20'),
(8, 'Hannah Martin', 'hannah.martin@example.com', '890-123-4567', '1987-04-08'),
(9, 'Ian Thompson', 'ian.thompson@example.com', '901-234-5678', '1983-09-25'),
(10, 'Jane White', 'jane.white@example.com', '012-345-6789', '1996-06-18');
";

        string insertTransactions = @"
INSERT INTO Transactions (TransactionID, CustomerID, TransactionDate, Amount, PaymentMethod, Status) VALUES
(1, 1, '2015-02-10', 150.00, 'Credit Card', 'PENDING'),
(2, 2, '2015-03-05', 200.00, 'PayPal', 'COMPLETED'),
(3, 3, '2015-03-15', 300.00, 'Debit Card', 'FAILED'),
(4, 4, '2015-02-20', 120.00, 'Credit Card', 'COMPLETED'),
(5, 5, '2015-03-25', 450.00, 'Bank Transfer', 'PENDING'),
(6, 6, '2015-02-28', 75.00, 'Cash', 'COMPLETED'),
(7, 7, '2015-03-10', 500.00, 'Credit Card', 'PENDING'),
(8, 8, '2015-02-15', 250.00, 'Debit Card', 'FAILED'),
(9, 9, '2015-03-18', 320.00, 'PayPal', 'COMPLETED'),
(10, 10, '2015-02-05', 100.00, 'Credit Card', 'PENDING');
";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(insertCustomers, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(insertTransactions, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Fetch and Format SQL Command
        ClientResult<ChatCompletion> completion = await chatClient.CompleteChatAsync(presetMessage, userCommand);
        string sqlCommand = completion.Value.Content[0].Text;

        sqlCommand = sqlCommand.Replace("`", string.Empty);
        sqlCommand = sqlCommand.Trim();
        sqlCommand = Regex.Replace(sqlCommand, @"\s+", " ");
        sqlCommand = sqlCommand.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
        if (sqlCommand.StartsWith("sql ", StringComparison.OrdinalIgnoreCase))
        {
            sqlCommand = sqlCommand.Substring(4).Trim();
        }

        Console.Write($"Generated SQL Query: {sqlCommand}");
        #endregion

        #region Execute the Query and Export to CSV
        try
        {
            string csvFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Report.csv");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlCommand, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        var csvBuilder = new StringBuilder();

                        if (reader.HasRows)
                        {
                            int fieldCount = reader.FieldCount;

                            for (int i = 0; i < fieldCount; i++)
                            {
                                csvBuilder.Append(reader.GetName(i));
                                if (i < fieldCount - 1)
                                    csvBuilder.Append(",");
                            }
                            csvBuilder.AppendLine();

                            while (reader.Read())
                            {
                                for (int i = 0; i < fieldCount; i++)
                                {
                                    csvBuilder.Append(reader.GetValue(i).ToString());
                                    if (i < fieldCount - 1)
                                        csvBuilder.Append(",");
                                }
                                csvBuilder.AppendLine();
                            }

                            File.WriteAllText(csvFilePath, csvBuilder.ToString());
                            Console.WriteLine($"\nData has been successfully written to {csvFilePath}");
                        }
                        else
                        {
                            Console.WriteLine("\nNo data found.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while executing the query: {ex.Message}");
        }
        #endregion
    }
}
