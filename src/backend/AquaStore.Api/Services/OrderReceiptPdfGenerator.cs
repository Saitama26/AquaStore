using AquaStore.Application.Orders.Queries;
using AquaStore.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AquaStore.Api.Services;

public static class OrderReceiptPdfGenerator
{
    public static byte[] Generate(OrderDetailResponse order)
    {
        var generatedAtUtc = DateTime.UtcNow;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.PageColor("#F1FAFF");
                page.DefaultTextStyle(text => text.FontSize(10.5f).FontColor("#123B5D"));

                page.Header().Element(header => ComposeHeader(header, order));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Element(card => ComposeOrderMeta(card, order));
                    column.Item().Element(card => ComposeShipping(card, order));
                    column.Item().Element(card => ComposeItems(card, order));
                    column.Item().Element(card => ComposeTotals(card, order));

                    if (!string.IsNullOrWhiteSpace(order.CustomerNote))
                    {
                        column.Item().Element(card => ComposeComment(card, order.CustomerNote!));
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"AquaStore • Океанический чек • Сформирован {generatedAtUtc:yyyy-MM-dd HH:mm} UTC")
                    .FontSize(9)
                    .FontColor("#4A7AA0");
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, OrderDetailResponse order)
    {
        container
            .PaddingBottom(10)
            .Background("#0B3C66")
            .Border(1)
            .BorderColor("#1E5D92")
            .Column(column =>
            {
                column.Item()
                    .PaddingHorizontal(18)
                    .PaddingTop(14)
                    .Text("AquaStore — Чек заказа")
                    .FontColor(Colors.White)
                    .SemiBold()
                    .FontSize(17);

                column.Item()
                    .PaddingHorizontal(18)
                    .PaddingTop(3)
                    .Text($"Заказ № {order.OrderNumber}")
                    .FontColor("#BEE6FF")
                    .FontSize(11);

                column.Item().PaddingTop(8).Background("#0F548A").PaddingHorizontal(18).PaddingVertical(7).Row(row =>
                {
                    row.RelativeItem().Text($"Статус: {ToStatusText(order.Status)}").FontColor(Colors.White).SemiBold();
                    row.AutoItem().Text($"Создан: {order.CreatedAt:dd.MM.yyyy HH:mm}").FontColor("#D2EEFF");
                });
            });
    }

    private static void ComposeOrderMeta(IContainer container, OrderDetailResponse order)
    {
        container
            .Background(Colors.White)
            .Border(1)
            .BorderColor("#CFE6F7")
            .Padding(14)
            .Column(column =>
            {
                column.Item().Text("Сводка по заказу").SemiBold().FontColor("#0B3C66").FontSize(12);
                column.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text($"Статус оплаты: {ToPaymentStatusText(order.PaymentStatus)}");
                    row.RelativeItem().AlignRight().Text($"Позиции: {order.Items.Count}");
                });

                column.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Отправлен: {(order.ShippedAt.HasValue ? order.ShippedAt.Value.ToString("dd.MM.yyyy HH:mm") : "—")}");
                    row.RelativeItem().AlignRight().Text($"Доставлен: {(order.DeliveredAt.HasValue ? order.DeliveredAt.Value.ToString("dd.MM.yyyy HH:mm") : "—")}");
                });
            });
    }

    private static void ComposeShipping(IContainer container, OrderDetailResponse order)
    {
        container
            .Background(Colors.White)
            .Border(1)
            .BorderColor("#CFE6F7")
            .Padding(14)
            .Column(column =>
            {
                column.Item().Text("Адрес доставки").SemiBold().FontColor("#0B3C66").FontSize(12);
                column.Item().PaddingTop(8).Text(
                    $"{order.ShippingAddress.City}, {order.ShippingAddress.Street}, {order.ShippingAddress.Building}" +
                    (string.IsNullOrWhiteSpace(order.ShippingAddress.Apartment) ? string.Empty : $", кв. {order.ShippingAddress.Apartment}"));
                column.Item().Text($"Индекс: {order.ShippingAddress.PostalCode}");
            });
    }

    private static void ComposeItems(IContainer container, OrderDetailResponse order)
    {
        container
            .Background(Colors.White)
            .Border(1)
            .BorderColor("#CFE6F7")
            .Padding(14)
            .Column(column =>
            {
                column.Item().Text("Состав заказа").SemiBold().FontColor("#0B3C66").FontSize(12);

                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4.2f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(2f);
                        columns.RelativeColumn(2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeader).Text("Товар").FontColor(Colors.White).SemiBold().FontSize(10);
                        header.Cell().Element(CellHeader).AlignRight().Text("Кол-во").FontColor(Colors.White).SemiBold().FontSize(10);
                        header.Cell().Element(CellHeader).AlignRight().Text("Цена").FontColor(Colors.White).SemiBold().FontSize(10);
                        header.Cell().Element(CellHeader).AlignRight().Text("Сумма").FontColor(Colors.White).SemiBold().FontSize(10);
                    });

                    foreach (var item in order.Items)
                    {
                        table.Cell().Element(CellBody).Text(item.ProductName).FontColor("#18476D");
                        table.Cell().Element(CellBody).AlignRight().Text(item.Quantity.ToString()).FontColor("#18476D");
                        table.Cell().Element(CellBody).AlignRight().Text(FormatMoney(item.UnitPrice, order.Currency)).FontColor("#18476D");
                        table.Cell().Element(CellBody).AlignRight().Text(FormatMoney(item.TotalPrice, order.Currency)).FontColor("#18476D").SemiBold();
                    }
                });
            });
    }

    private static void ComposeTotals(IContainer container, OrderDetailResponse order)
    {
        container
            .Background("#E8F6FF")
            .Border(1)
            .BorderColor("#B8DBF3")
            .Padding(14)
            .Column(column =>
            {
                column.Spacing(4);
                column.Item().Text("Итоги оплаты").SemiBold().FontColor("#0B3C66").FontSize(12);
                column.Item().Element(row => ComposeTotalRow(row, "Подытог", FormatMoney(order.SubTotal, order.Currency)));
                column.Item().Element(row => ComposeTotalRow(row, "Доставка", FormatMoney(order.ShippingCost, order.Currency)));

                if (order.Discount.HasValue)
                {
                    column.Item().Element(row => ComposeTotalRow(row, "Скидка", $"-{FormatMoney(order.Discount.Value, order.Currency)}"));
                }

                column.Item().PaddingTop(6).LineHorizontal(1).LineColor("#9BC9E8");
                column.Item().Element(row => ComposeTotalRow(row, "Итого", FormatMoney(order.TotalAmount, order.Currency), true));
            });
    }

    private static void ComposeComment(IContainer container, string comment)
    {
        container
            .Background(Colors.White)
            .Border(1)
            .BorderColor("#CFE6F7")
            .Padding(14)
            .Column(column =>
            {
                column.Item().Text("Комментарий клиента").SemiBold().FontColor("#0B3C66").FontSize(12);
                column.Item().PaddingTop(6).Text(comment).FontColor("#1C496C");
            });
    }

    private static void ComposeTotalRow(IContainer container, string label, string value, bool isTotal = false)
    {
        container.Row(row =>
        {
            var labelText = row.RelativeItem().Text(label).FontColor(isTotal ? "#0A3255" : "#2C5A7F");
            if (isTotal)
            {
                labelText.SemiBold();
            }

            var valueText = row.AutoItem().Text(value).FontColor(isTotal ? "#0A3255" : "#2C5A7F").FontSize(isTotal ? 13 : 11);
            if (isTotal)
            {
                valueText.SemiBold();
            }
        });
    }

    private static IContainer CellHeader(IContainer container)
    {
        return container
            .Background("#0F548A")
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer CellBody(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor("#E6F1F8")
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static string FormatMoney(decimal amount, string currency)
    {
        return $"{amount:N2} {currency}";
    }

    private static string ToStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Ожидает",
            OrderStatus.Confirmed => "Подтверждён",
            OrderStatus.Processing => "В обработке",
            OrderStatus.Shipped => "Отправлен",
            OrderStatus.Delivered => "Доставлен",
            OrderStatus.Cancelled => "Отменён",
            _ => "Неизвестно"
        };
    }

    private static string ToPaymentStatusText(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Ожидает",
            PaymentStatus.Paid => "Оплачено",
            PaymentStatus.Failed => "Ошибка",
            PaymentStatus.Refunded => "Возврат",
            _ => "Неизвестно"
        };
    }
}
