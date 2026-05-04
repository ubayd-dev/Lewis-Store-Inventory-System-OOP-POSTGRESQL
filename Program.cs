using System;
using System.Collections.Generic;
using System.Threading;
using LewiStoreOOPSQL.Models;
using LewiStoreOOPSQL.Services;
using Spectre.Console;
using LewiStoreOOPSQL.Data;

namespace LewiStoreOOPSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=LewisStoreDB";

            DatabaseManager db = new DatabaseManager(connectionString);
            InventoryService service = new InventoryService(db);
            bool running = true;

            while (running)
            {
                Console.Clear();
                ShowHeader();
                ShowMenuPanel();

                string choice = ShowMenuChoice();

                switch (choice)
                {
                    case "📦 Add Product":
                        AddProductUI(service);
                        break;

                    case "📋 View Products":
                        ViewProductsUI(service);
                        break;

                    case "💰 Sell Product":
                        SellProductUI(service);
                        break;

                    case "🧾 View Sales":
                        ViewSalesUI(service);
                        break;

                    case "✏️ Update Product":
                        UpdateProductUI(service);
                        break;

                    case "🗑️ Delete Product":
                        DeleteProductUI(service);
                        break;


                    // case "Cancel / Return to Menu":
                    //     continue;

                    case "❌ Exit":
                        running = false;
                        ExitScreen();
                        break;
                }

                if (running)
                {
                    Pause();
                }
            }
        }

        static void ShowHeader()
        {
            AnsiConsole.Write(
                Align.Center(
                    new Markup(@"[red]
        ▗▖   ▗▄▄▄▖▗▖ ▗▖▗▄▄▄▖ ▗▄▄▖     ▗▄▄▖▗▄▄▄▖▗▄▖ ▗▄▄▖ ▗▄▄▄▖
    ▐▌   ▐▌   ▐▌ ▐▌  █  ▐▌       ▐▌     █ ▐▌ ▐▌▐▌ ▐▌▐▌
        ▐▌   ▐▛▀▀▘▐▌ ▐▌  █   ▝▀▚▖     ▝▀▚▖  █ ▐▌ ▐▌▐▛▀▚▖▐▛▀▀▘
        ▐▙▄▄▖▐▙▄▄▖▐▙█▟▌▗▄█▄▖▗▄▄▞▘    ▗▄▄▞▘  █ ▝▚▄▞▘▐▌ ▐▌▐▙▄▄▖
[/]")
                )
            );
        }

        static void ShowMenuPanel()
        {
            AnsiConsole.Write(
                Align.Center(
                   new Panel("[cyan]1. Add Product\n2. View Products\n3. Sell Product\n4. View Sales\n5. Update Product\n6. Delete Product\n7. Exit[/]")
                        .Header("STORE MENU")
                        .Border(BoxBorder.Rounded)
                )
            );
        }

        static string ShowMenuChoice()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Choose an option:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Black, background: Color.Cyan))
                    .AddChoices(new[]
                    {
                      "📦 Add Product",
    "📋 View Products",
    "💰 Sell Product",
    "🧾 View Sales",
    "✏️ Update Product",
    "🗑️ Delete Product",
    "❌ Exit"
        // "Cancel / Return to Menu",
    
                    })
            );
        }

        static bool ConfirmCancel()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[red]Are you sure?[/] You selected by mistake.")
                    .AddChoices(new[] { "No, continue", "Yes, return to menu" }));

            return choice == "Yes, return to menu";
        }

        static bool ShouldContinueField(string fieldName)
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{fieldName}[/]")
                    .AddChoices($"Enter {fieldName}", "Return to Menu")
            );

            return choice != "Return to Menu";
        }


        static void AddProductUI(InventoryService service)
        {
            Console.Clear();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold yellow]Add New Product[/]")
                        .Border(BoxBorder.Double)
                        .Padding(1, 1)
                )
            );

            bool retryAdding = true;

            while (retryAdding)
            {
                try
                {
                    if (!ShouldContinueField("Product ID"))
                        return;

                    int productId = AnsiConsole.Prompt(
                        new TextPrompt<int>("[green]Enter Product ID:[/]")
                            .ValidationErrorMessage("[red]Invalid number[/]")
                            .Validate(id => id > 0)
                    );

                    Product existingProduct = service.GetProductById(productId);

                    if (existingProduct != null)
                    {
                        AnsiConsole.MarkupLine("[red]Product with this ID already exists.[/]");

                        string retryChoice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]What would you like to do?[/]")
                                .AddChoices("Try Again", "Return to Menu")
                        );

                        if (retryChoice == "Return to Menu")
                            return;

                        continue;
                    }

                    if (!ShouldContinueField("Product Name"))
                        return;

                    string productName = AnsiConsole.Ask<string>("[green]Enter Product Name:[/]");

                    if (!ShouldContinueField("Description"))
                        return;

                    string description = AnsiConsole.Ask<string>("[green]Enter Description:[/]");

                    if (!ShouldContinueField("Price Excluding VAT"))
                        return;

                    decimal priceExclusiveVat = AnsiConsole.Prompt(
                        new TextPrompt<decimal>("[green]Enter Price Excluding VAT:[/]")
                            .ValidationErrorMessage("[red]Invalid price[/]")
                            .Validate(price => price > 0)
                    );

                    if (!ShouldContinueField("Quantity In Stock"))
                        return;

                    int quantityInStock = AnsiConsole.Prompt(
                        new TextPrompt<int>("[green]Enter Quantity In Stock:[/]")
                            .ValidationErrorMessage("[red]Invalid quantity[/]")
                            .Validate(qty => qty >= 0)
                    );

                    Product product = new Product(
                        productId,
                        productName,
                        description,
                        priceExclusiveVat,
                        quantityInStock
                    );

                    service.AddProduct(product);

                    AnsiConsole.Write(
                        Align.Center(
                            new Panel("[bold green]✔ Product Added Successfully[/]")
                                .Border(BoxBorder.Rounded)
                        )
                    );

                    retryAdding = false;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");

                    string retryChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Would you like to try again?[/]")
                            .AddChoices("Try Again", "Return to Menu")
                    );

                    if (retryChoice == "Return to Menu")
                        retryAdding = false;
                }
            }
        }

        static void ViewProductsUI(InventoryService service)
        {
            Console.Clear();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold cyan]Inventory Overview[/]")
                        .Border(BoxBorder.Double)
                )
            );

            List<Product> products = service.GetAllProducts();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products in inventory[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);

            table.AddColumn("[yellow]ID[/]");
            table.AddColumn("[yellow]Name[/]");
            table.AddColumn("[yellow]Description[/]");
            table.AddColumn("[yellow]Price[/]");
            table.AddColumn("[yellow]Qty[/]");
            table.AddColumn("[yellow]Status[/]");



            foreach (Product product in products)
            {
                string status = product.QuantityInStock == 0
                    ? "[red]OUT OF STOCK[/]"
                    : "[green]In Stock[/]";

                table.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName),
                    Markup.Escape(product.Description),
                    $"R{product.PriceExclusiveVat:0.00}",
                    product.QuantityInStock.ToString(),
                    status
                );
            }

            AnsiConsole.Write(Align.Center(table));

            var back = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    // .Title("[yellow]What would you like to do?[/]")
                    .AddChoices("Return to Menu"));

            if (back == "Refresh View")
            {
                ViewProductsUI(service);
            }
        }

        static void UpdateProductUI(InventoryService service)
        {
            Console.Clear();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold cyan]Update Product[/]")
                        .Border(BoxBorder.Double)
                        .Padding(1, 1)
                )
            );

            List<Product> products = service.GetAllProducts();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available to update.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);

            table.AddColumn("[yellow]ID[/]");
            table.AddColumn("[yellow]Name[/]");
            table.AddColumn("[yellow]Description[/]");
            table.AddColumn("[yellow]Price[/]");
            table.AddColumn("[yellow]Qty[/]");

            foreach (Product product in products)
            {
                table.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName),
                    Markup.Escape(product.Description),
                    $"R{product.PriceExclusiveVat:0.00}",
                    product.QuantityInStock.ToString()
                );
            }

            AnsiConsole.Write(Align.Center(table));

            try
            {
                int productId = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter Product ID to update:[/]")
                        .ValidationErrorMessage("[red]Invalid Product ID[/]")
                );

                Product existingProduct = service.GetProductById(productId);

                if (existingProduct == null)
                {
                    AnsiConsole.MarkupLine("[red]Product not found.[/]");
                    return;
                }

                bool updating = true;

                while (updating)
                {
                    Console.Clear();

                    AnsiConsole.Write(
                        Align.Center(
                            new Panel($"[bold yellow]Updating: {Markup.Escape(existingProduct.ProductName)}[/]")
                                .Border(BoxBorder.Double)
                        )
                    );

                    AnsiConsole.MarkupLine($"[grey]Product ID:[/] {existingProduct.ProductId}");
                    AnsiConsole.MarkupLine($"[grey]Name:[/] {Markup.Escape(existingProduct.ProductName)}");
                    AnsiConsole.MarkupLine($"[grey]Description:[/] {Markup.Escape(existingProduct.Description)}");
                    AnsiConsole.MarkupLine($"[grey]Price:[/] R{existingProduct.PriceExclusiveVat:0.00}");
                    AnsiConsole.MarkupLine($"[grey]Stock:[/] {existingProduct.QuantityInStock}");

                    string updateChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]What would you like to update?[/]")
                            .AddChoices(
                                "Name",
                                "Description",
                                "Price",
                                "Quantity",
                                "Finish Updating",
                                "Cancel"
                            )
                    );

                    switch (updateChoice)
                    {
                        case "Name":
                            existingProduct.ProductName = AnsiConsole.Ask<string>("[green]Enter new product name:[/]");
                            service.UpdateProduct(existingProduct);
                            AnsiConsole.MarkupLine("[green]Name updated successfully.[/]");
                            break;

                        case "Description":
                            existingProduct.Description = AnsiConsole.Ask<string>("[green]Enter new description:[/]");
                            service.UpdateProduct(existingProduct);
                            AnsiConsole.MarkupLine("[green]Description updated successfully.[/]");
                            break;

                        case "Price":
                            existingProduct.PriceExclusiveVat = AnsiConsole.Prompt(
                                new TextPrompt<decimal>("[green]Enter new price:[/]")
                                    .ValidationErrorMessage("[red]Invalid price[/]")
                                    .Validate(price => price > 0)
                            );
                            service.UpdateProduct(existingProduct);
                            AnsiConsole.MarkupLine("[green]Price updated successfully.[/]");
                            break;

                        case "Quantity":
                            existingProduct.QuantityInStock = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter new quantity:[/]")
                                    .ValidationErrorMessage("[red]Invalid quantity[/]")
                                    .Validate(qty => qty >= 0)
                            );
                            service.UpdateProduct(existingProduct);
                            AnsiConsole.MarkupLine("[green]Quantity updated successfully.[/]");
                            break;

                        case "Finish Updating":
                            updating = false;
                            AnsiConsole.Write(
                                Align.Center(
                                    new Panel("[bold green]✔ Product Update Complete[/]")
                                        .Border(BoxBorder.Rounded)
                                )
                            );
                            break;

                        case "Cancel":
                            updating = false;
                            AnsiConsole.MarkupLine("[grey]Update cancelled.[/]");
                            break;
                    }

                    if (updating)
                    {
                        existingProduct = service.GetProductById(productId);
                        AnsiConsole.MarkupLine("\n[grey]Press ENTER to continue updating...[/]");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            }
        }
        static void DeleteProductUI(InventoryService service)
        {
            Console.Clear();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold red]Delete Product[/]")
                        .Border(BoxBorder.Double)
                        .Padding(1, 1)
                )
            );

            List<Product> products = service.GetAllProducts();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available to delete.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);

            table.AddColumn("[yellow]ID[/]");
            table.AddColumn("[yellow]Name[/]");
            table.AddColumn("[yellow]Description[/]");
            table.AddColumn("[yellow]Price[/]");
            table.AddColumn("[yellow]Qty[/]");

            foreach (Product product in products)
            {
                table.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName),
                    Markup.Escape(product.Description),
                    $"R{product.PriceExclusiveVat:0.00}",
                    product.QuantityInStock.ToString()
                );
            }

            AnsiConsole.Write(Align.Center(table));

            try
            {
                int productId = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter Product ID to delete:[/]")
                        .ValidationErrorMessage("[red]Invalid Product ID[/]")
                );

                Product existingProduct = service.GetProductById(productId);

                if (existingProduct == null)
                {
                    AnsiConsole.MarkupLine("[red]Product not found.[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[yellow]You are about to delete:[/] {Markup.Escape(existingProduct.ProductName)}");

                string confirmDelete = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[red]Are you sure you want to delete this product?[/]")
                        .AddChoices("Delete", "Cancel")
                );

                if (confirmDelete == "Cancel")
                {
                    AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
                    return;
                }

                service.DeleteProduct(productId);

                AnsiConsole.Write(
                    Align.Center(
                        new Panel("[bold green]✔ Product Deleted Successfully[/]")
                            .Border(BoxBorder.Rounded)
                    )
                );
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            }
        }
        static void SellProductUI(InventoryService service)
        {
            Console.Clear();

            List<Product> products = service.GetAllProducts();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available to sell[/]");
                return;
            }

            var stockTable = new Table().Border(TableBorder.Rounded);

            stockTable.AddColumn("[yellow]ID[/]");
            stockTable.AddColumn("[yellow]Name[/]");
            stockTable.AddColumn("[yellow]Description[/]");
            stockTable.AddColumn("[yellow]Price[/]");
            stockTable.AddColumn("[yellow]Qty[/]");
            stockTable.AddColumn("[yellow]Status[/]");

            foreach (Product product in products)
            {
                string status = product.QuantityInStock == 0
                    ? "[red]OUT OF STOCK[/]"
                    : "[green]In Stock[/]";

                stockTable.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName),
                    Markup.Escape(product.Description),
                    $"R{product.PriceExclusiveVat:0.00}",
                    product.QuantityInStock.ToString(),
                    status
                );
            }
            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold cyan]Available Products[/]")
                        .Border(BoxBorder.Double)
                )
            );

            AnsiConsole.Write(Align.Center(stockTable));

            var confirmStep = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Sell a product?")
                    .AddChoices("Continue", "Cancel"));

            if (confirmStep == "Cancel")
                return;

            bool retrySelling = true;

            while (retrySelling)
            {
                try
                {
                    int productId = AnsiConsole.Prompt(
                        new TextPrompt<int>("[green]Enter Product ID to sell:[/]")
                            .ValidationErrorMessage("[red]Please enter a valid ID[/]")
                    );

                    Product selectedProduct = service.GetProductById(productId);

                    if (selectedProduct == null)
                        throw new Exception("Product does not exist.");

                    AnsiConsole.MarkupLine($"Selected Product: {Markup.Escape(selectedProduct.ProductName)}");

                    int sellQty = AnsiConsole.Prompt(
                        new TextPrompt<int>("[green]Enter quantity to sell:[/]")
                            .ValidationErrorMessage("[red]Please enter a positive number[/]")
                            .Validate(qty => qty > 0, "[red]Quantity must be at least 1[/]")
                    );

                    var confirmSale = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"[yellow]Confirm sale of {sellQty} {Markup.Escape(selectedProduct.ProductName)}?[/]")
                            .AddChoices("Confirm", "Cancel"));

                    if (confirmSale == "Cancel")
                    {
                        AnsiConsole.MarkupLine("[yellow]Sale cancelled.[/]");
                        return;
                    }

                    Sale sale = service.SellProduct(productId, sellQty);
                    Product updatedProduct = service.GetProductById(productId);

                    var receipt = new Table();
                    receipt.AddColumn("Field");
                    receipt.AddColumn("Value");

                    receipt.AddRow("Sale ID", sale.SaleId.ToString());
                    receipt.AddRow("Item", Markup.Escape(selectedProduct.ProductName));
                    receipt.AddRow("Price", $"R{selectedProduct.PriceExclusiveVat:0.00}");
                    receipt.AddRow("Quantity", sale.QuantitySold.ToString());
                    receipt.AddRow("Subtotal", $"R{sale.Subtotal:0.00}");
                    receipt.AddRow("VAT", $"R{sale.VatAmount:0.00}");
                    receipt.AddRow("TOTAL", $"R{sale.TotalAmount:0.00}");

                    AnsiConsole.Write(
                        new Panel(receipt)
                            .Header("[green] RECEIPT [/]")
                            .Border(BoxBorder.Double)
                            .Padding(1, 1)
                    );

                    AnsiConsole.MarkupLine($"\n[green]Stock remaining: {updatedProduct.QuantityInStock}[/]");
                    AnsiConsole.MarkupLine("[grey]Thank you for shopping![/]");

                    retrySelling = false;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");

                    string retryChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Would you like to try again?[/]")
                            .AddChoices("Try Again", "Return to Menu")
                    );

                    if (retryChoice == "Return to Menu")
                    {
                        retrySelling = false;
                    }
                }
            }
        }

        static void ViewSalesUI(InventoryService service)
        {
            Console.Clear();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold magenta]Sales History[/]")
                        .Border(BoxBorder.Double)
                )
            );

            List<Sale> sales = service.GetAllSales();

            if (sales.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No sales found[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);

            table.AddColumn("[yellow]Sale ID[/]");
            table.AddColumn("[yellow]Product ID[/]");
            table.AddColumn("[yellow]Quantity in stock[/]");
            table.AddColumn("[yellow]Subtotal[/]");
            table.AddColumn("[yellow]VAT[/]");
            table.AddColumn("[yellow]Total[/]");
            table.AddColumn("[yellow]Date Of Purchase[/]");

            foreach (Sale sale in sales)
            {
                table.AddRow(
                    sale.SaleId.ToString(),
                    sale.ProductId.ToString(),
                    sale.QuantitySold.ToString(),
                    $"R{sale.Subtotal:0.00}",
                    $"R{sale.VatAmount:0.00}",
                    $"R{sale.TotalAmount:0.00}",
                    sale.SaleDate.ToString("g")
                );
            }

            AnsiConsole.Write(Align.Center(table));
        }

        static void Pause()
        {
            AnsiConsole.MarkupLine("[grey]Press ENTER to continue...[/]");
            Console.ReadLine();
        }

        static void ExitScreen()
        {
            Console.Clear();

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            AnsiConsole.Write(
                Align.Center(
                    new Panel("[bold green]THANK YOU FOR USING LEWIS STORE[/]\n[grey]See you again soon.[/]")
                        .Border(BoxBorder.Double)
                        .Padding(4, 2)
                )
            );

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            AnsiConsole.Write(
                Align.Center(
                    new Markup("[bold white]Developed By[/]\n[italic cyan]Thaqib Ghany & Geraldo Koopman[/]")
                )
            );

            Thread.Sleep(3000);
        }
    }
}