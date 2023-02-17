using clothes_bot.Models.ModelsDB;

namespace clothes_bot.Models
{
    public class CategoryProducts
    {
        public long? ChatId { get; set; }
        public long? ProductCategoryId { get; set; }
        public long? ProductsAmount { get; set; }
        public int CurrentProductListIndex { get; set; }
        public List<Product> ProductsList { get; set; }
    }

    public static class CategoryProductsExtensions
    {
        public static void Clear(this CategoryProducts currentCategoryProducts)
        {
            currentCategoryProducts.ProductsAmount = null;
            currentCategoryProducts.ProductCategoryId = null;
            currentCategoryProducts.CurrentProductListIndex = 0;
            currentCategoryProducts.ProductsList.Clear();
        }

        public static void Add(this CategoryProducts currentCategoryProducts, Product product)
        {
            currentCategoryProducts.ProductsList.Add(product);
        }

        public static void AddRange(this CategoryProducts currentCategoryProducts, List<Product> productList)
        {
            foreach (Product product in productList)
                currentCategoryProducts.ProductsList.Add(product);
        }

        public static string? GetCurrentIndexImage(this CategoryProducts currentCategoryProducts)
        {
            return currentCategoryProducts.ProductsList[currentCategoryProducts.CurrentProductListIndex].Image;
        }

        public static void DecreaseIndex(this CategoryProducts currentCategoryProducts)
        {
            if (currentCategoryProducts.CurrentProductListIndex > 0) 
                currentCategoryProducts.CurrentProductListIndex--;
        }

        public static void IncreaseIndex(this CategoryProducts currentCategoryProducts)
        {
            currentCategoryProducts.CurrentProductListIndex++;
        }
    }
}
