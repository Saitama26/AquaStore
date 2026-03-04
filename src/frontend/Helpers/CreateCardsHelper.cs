using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using frontend.ViewModels;

namespace frontend.Helpers;

public static class ListHelper
{
    public static IHtmlContent CreateCards(
        this IHtmlHelper htmlHelper,
        List<ProductViewModel> products,
        bool includeContainer = true)
    {
        if (products == null || products.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        if (!includeContainer)
        {
            var builder = new HtmlContentBuilder();
            foreach (var product in products)
            {
                var cardHtml = CreateProductCard(product);
                builder.AppendHtml(new HtmlString(cardHtml));
            }
            return builder;
        }

        var container = new TagBuilder("div");
        container.AddCssClass("products-grid");

        foreach (var product in products)
        {
            var cardHtml = CreateProductCard(product);
            container.InnerHtml.AppendHtml(new HtmlString(cardHtml));
        }

        return container;
    }

    private static string CreateProductCard(ProductViewModel product)
    {
        var card = new TagBuilder("div");
        card.AddCssClass("product-card");

        var link = new TagBuilder("a");
        link.AddCssClass("product-card-link");
        link.Attributes["href"] = $"/products/{product.Slug}";

        // Image container
        var imageContainer = new TagBuilder("div");
        imageContainer.AddCssClass("product-image-container");

        // Image or placeholder
        if (!string.IsNullOrEmpty(product.MainImageUrl))
        {
            var img = new TagBuilder("img");
            img.AddCssClass("product-image");
            img.Attributes["src"] = product.MainImageUrl;
            img.Attributes["alt"] = product.Name;
            imageContainer.InnerHtml.AppendHtml(img);
        }
        else
        {
            var placeholder = new TagBuilder("div");
            placeholder.AddCssClass("product-image-placeholder");
            placeholder.InnerHtml.Append("Нет изображения");
            imageContainer.InnerHtml.AppendHtml(placeholder);
        }

        // Discount badge
        // Показываем бейдж "СКИДКА" только если:
        // 1. У товара есть старая цена (OldPrice.HasValue)
        // 2. Старая цена больше текущей (OldPrice > Price) - значит есть реальная скидка
        // Если OldPrice отсутствует или меньше/равна текущей цене - бейдж не показываем
        if (product.OldPrice.HasValue && product.OldPrice > product.Price)
        {
            var discountPercent = (int)((1 - product.Price / product.OldPrice.Value) * 100);
            var discountBadge = new TagBuilder("div");
            discountBadge.AddCssClass("product-badge discount-badge");
            
            var discountPercentSpan = new TagBuilder("span");
            discountPercentSpan.AddCssClass("discount-percent");
            discountPercentSpan.InnerHtml.Append($"-{discountPercent}%");
            
            var discountTextSpan = new TagBuilder("span");
            discountTextSpan.AddCssClass("discount-text");
            discountTextSpan.InnerHtml.Append("СКИДКА");
            
            discountBadge.InnerHtml.AppendHtml(discountPercentSpan);
            discountBadge.InnerHtml.AppendHtml(discountTextSpan);
            imageContainer.InnerHtml.AppendHtml(discountBadge);
        }

        // Featured badge
        if (product.IsFeatured)
        {
            var featuredBadge = new TagBuilder("div");
            featuredBadge.AddCssClass("product-badge featured-badge");
            featuredBadge.InnerHtml.Append("ХИТ");
            imageContainer.InnerHtml.AppendHtml(featuredBadge);
        }

        link.InnerHtml.AppendHtml(imageContainer);

        // Product info
        var productInfo = new TagBuilder("div");
        productInfo.AddCssClass("product-info");

        // Price
        // Всегда показываем актуальную цену
        var priceContainer = new TagBuilder("div");
        priceContainer.AddCssClass("product-price");
        
        var currentPrice = new TagBuilder("span");
        currentPrice.AddCssClass("current-price");
        currentPrice.InnerHtml.Append($"{product.Price:N2} {product.Currency}");
        priceContainer.InnerHtml.AppendHtml(currentPrice);

        // Старая цена показывается только если:
        // 1. У товара есть старая цена (OldPrice.HasValue)
        // 2. Старая цена больше текущей (OldPrice > Price) - значит есть реальная скидка
        // Если OldPrice отсутствует или меньше/равна текущей цене - показываем только актуальную цену
        if (product.OldPrice.HasValue && product.OldPrice > product.Price)
        {
            var oldPrice = new TagBuilder("span");
            oldPrice.AddCssClass("old-price");
            oldPrice.InnerHtml.Append($"{product.OldPrice.Value:N2} {product.Currency}");
            priceContainer.InnerHtml.AppendHtml(oldPrice);
        }

        productInfo.InnerHtml.AppendHtml(priceContainer);

        // Title
        var title = new TagBuilder("h3");
        title.AddCssClass("product-title");
        title.InnerHtml.Append(product.Name);
        productInfo.InnerHtml.AppendHtml(title);

        // Rating
        if (product.AverageRating.HasValue && product.ReviewCount > 0)
        {
            var ratingContainer = new TagBuilder("div");
            ratingContainer.AddCssClass("product-rating");
            
            var ratingStars = new TagBuilder("span");
            ratingStars.AddCssClass("rating-stars");
            ratingStars.InnerHtml.Append($"★{product.AverageRating.Value:F1}");
            
            var ratingCount = new TagBuilder("span");
            ratingCount.AddCssClass("rating-count");
            ratingCount.InnerHtml.Append($"· {product.ReviewCount} отзывов");
            
            ratingContainer.InnerHtml.AppendHtml(ratingStars);
            ratingContainer.InnerHtml.AppendHtml(ratingCount);
            productInfo.InnerHtml.AppendHtml(ratingContainer);
        }

        // Add to cart button
        var addToCartBtn = new TagBuilder("button");
        addToCartBtn.AddCssClass("add-to-cart-btn");
        addToCartBtn.Attributes["type"] = "button";
        addToCartBtn.Attributes["data-product-id"] = product.Id.ToString();
        addToCartBtn.Attributes["onclick"] = $"event.preventDefault(); event.stopPropagation(); addToCart('{product.Id}', this);";
        
        var cartIcon = new TagBuilder("svg");
        cartIcon.AddCssClass("cart-icon");
        cartIcon.Attributes["viewBox"] = "0 0 24 24";
        cartIcon.Attributes["fill"] = "none";
        cartIcon.Attributes["stroke"] = "currentColor";
        cartIcon.Attributes["stroke-width"] = "2";
        
        var path = new TagBuilder("path");
        path.Attributes["d"] = "M9 21a1 1 0 1 0 0-2 1 1 0 0 0 0 2zM20 21a1 1 0 1 0 0-2 1 1 0 0 0 0 2zM1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6";
        cartIcon.InnerHtml.AppendHtml(path);
        
        var buttonText = new TagBuilder("span");
        buttonText.InnerHtml.Append("В корзину");
        
        addToCartBtn.InnerHtml.AppendHtml(cartIcon);
        addToCartBtn.InnerHtml.AppendHtml(buttonText);
        productInfo.InnerHtml.AppendHtml(addToCartBtn);

        link.InnerHtml.AppendHtml(productInfo);
        card.InnerHtml.AppendHtml(link);

        using var writer = new StringWriter();
        card.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }
}
