using System;
using System.Collections.Generic;
using LewiStoreOOPSQL.Models;
using LewiStoreOOPSQL.Services;
using Spectre.Console;

namespace LewiStoreOOPSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InventoryService service = new InventoryService();
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

                    case "Cancel / Return to Menu":
                        continue;

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
                    new Panel("[cyan]1. Add Product\n2. View Products\n3. Sell Product\n4. View Sales\n5. Exit[/]")
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
                        "Cancel / Return to Menu",
                        "❌ Exit"
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

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Continue adding product?")
                    .AddChoices("Continue", "Cancel"));

            if (confirm == "Cancel")
                return;

            try
            {
                int productId = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter Product ID:[/]")
                        .ValidationErrorMessage("[red]Invalid number[/]")
                        .Validate(id => id > 0)
                );

                string productName = AnsiConsole.Ask<string>("[green]Enter Product Name:[/]");

                string description = AnsiConsole.Ask<string>("[green]Enter Description:[/]");

                decimal priceExclusiveVat = AnsiConsole.Prompt(
                    new TextPrompt<decimal>("[green]Enter Price Excluding VAT:[/]")
                        .ValidationErrorMessage("[red]Invalid price[/]")
                        .Validate(price => price > 0)
                );

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
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
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
                    .Title("[yellow]What would you like to do?[/]")
                    .AddChoices("Return to Menu", "Refresh View"));

            if (back == "Refresh View")
            {
                ViewProductsUI(service);
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

            var stockTable = new Table().Border(TableBorder.Minimal);
            stockTable.AddColumns(
                "[yellow]ID[/]",
                "[bold]Item[/]",
                "[bold]Price[/]",
                "[bold]Qty[/]",
                "[bold]Status[/]"
            );

            foreach (Product product in products)
            {
                string qtyDisplay = product.QuantityInStock > 0
                    ? $"[green]{product.QuantityInStock}[/]"
                    : "[red]OUT[/]";

                string status = product.QuantityInStock > 0
                    ? "[green]Available[/]"
                    : "[red]Sold out[/]";

                stockTable.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName),
                    $"R{product.PriceExclusiveVat:N2}",
                    qtyDisplay,
                    status
                );
            }

            AnsiConsole.Write(
                new Panel(stockTable)
                    .Header("[cyan] Available Products [/]")
                    .Border(BoxBorder.Rounded)
                    .Padding(1, 0)
            );

            var confirmStep = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Sell a product?")
                    .AddChoices("Continue", "Cancel"));

            if (confirmStep == "Cancel")
                return;

            try
            {
                int productId = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter Product ID to sell:[/]")
                        .ValidationErrorMessage("[red]Please enter a valid ID[/]")
                );

                Product selectedProduct = service.GetProductById(productId);

                if (selectedProduct == null)
                {
                    AnsiConsole.MarkupLine("[red]Product does not exist[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"Selected Product: {Markup.Escape(selectedProduct.ProductName)}");

                int sellQty = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter quantity to sell:[/]")
                        .ValidationErrorMessage("[red]Please enter a positive number[/]")
                        .Validate(qty => qty > 0, "[red]Quantity must be at least 1[/]")
                );

                if (ConfirmCancel())
                {
                    AnsiConsole.MarkupLine("[yellow]Sale cancelled.[/]");
                    return;
                }

                var confirmSale = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[yellow]Confirm sale of {sellQty} {Markup.Escape(selectedProduct.ProductName)}?[/]")
                        .AddChoices("Confirm", "Cancel"));

                if (confirmSale == "Cancel")
                {
                    AnsiConsole.MarkupLine("[red]Sale cancelled.[/]");
                    return;
                }

                Sale sale = service.SellProduct(productId, sellQty);

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

                AnsiConsole.MarkupLine($"\n[green]Stock remaining: {selectedProduct.QuantityInStock}[/]");
                AnsiConsole.MarkupLine("[grey]Thank you for shopping![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
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
            table.AddColumn("[yellow]Qty[/]");
            table.AddColumn("[yellow]Subtotal[/]");
            table.AddColumn("[yellow]VAT[/]");
            table.AddColumn("[yellow]Total[/]");
            table.AddColumn("[yellow]Date[/]");

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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Clear();
        }
    }
}