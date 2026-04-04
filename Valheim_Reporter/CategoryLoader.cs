using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ValheimReporter
{
    /// <summary>
    /// Handles loading and management of report categories and their webhooks.
    /// </summary>
    public static class CategoryLoader
    {
        /// <summary>
        /// Loads a CategoriesRoot object from a JSON file.
        /// </summary>
        /// <param name="fileName">The name of the JSON file to load.</param>
        /// <returns>A CategoriesRoot object containing the loaded categories, or null if an error occurred.</returns>
        /// <remarks>
        /// The JSON file should be located in the "Data" directory of the plugin.
        /// The JSON file should contain a root object with dynamic keys like "category_1".
        /// Each category should contain "name", "webhook", "required" and "subcategories" properties.
        /// </remarks>
        public static CategoriesRoot Load(string fileName)
        {
            try 
            {
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(dllPath, "ValheimReporter", "Data", fileName);

                if (!File.Exists(filePath))
                {
                    Utils.LogSystem("CategoryLoader", "Load", $"JSON file missing: {fileName}", "error", 1);
                    return null;
                }

                string jsonContent = File.ReadAllText(filePath);
                
                // We use a Dictionary because the JSON root has dynamic keys like "category_1"
                var data = JsonConvert.DeserializeObject<Dictionary<string, Category>>(jsonContent);
                
                return new CategoriesRoot { Categories = data };
            }
            catch (Exception ex)
            {
                Utils.LogSystem("CategoryLoader", "Load", $"Error parsing categories: {ex.Message}", "error", 1);
                return null;
            }
        }
    }

    public class CategoriesRoot
    {
        public Dictionary<string, Category> Categories { get; set; } = new Dictionary<string, Category>();

        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to retrieve.</param>
        /// <returns>The category with the given ID, or null if it does not exist.</returns>
        public Category GetCategory(string id)
        {
            return Categories.TryGetValue(id, out var category) ? category : null;
        }


        /// <summary>
        /// Gets a category by its name.
        /// </summary>
        /// <param name="name">The name of the category to retrieve.</param>
        /// <returns>The category with the given name, or null if it does not exist.</returns>
        /// <remarks>
        /// This method iterates over all the categories in the root and compares the name of each category with the given name.
        /// If a matching category is found, it is returned and a log message is printed.
        /// If no matching category is found, a log message is printed and null is returned.
        /// </remarks>
        public Category GetCategoryByName(string name)
        {
            foreach (var cat in Categories.Values)
            {
                if (cat.Name == name){ Utils.LogSystem("CategoryLoader", "GetCategoryByName", $"Found category: {cat.Name}", "log", 4); return cat; }
            }
            Utils.LogSystem("CategoryLoader", "GetCategoryByName", $"Category not found: {name}", "error", 4);
            return null;
        }


        /// <summary>
        /// Gets a subcategory by its ID.
        /// </summary>
        /// <param name="id">The ID of the subcategory to retrieve.</param>
        /// <returns>The subcategory with the given ID, or null if it does not exist.</returns>
        /// <remarks>
        /// This method iterates over all the categories in the root and checks if each category has a subcategory with the given ID.
        /// If a matching subcategory is found, it is returned and a log message is printed.
        /// If no matching subcategory is found, a log message is printed and null is returned.
        /// </remarks>
        public Subcategory GetSubcategory(string id)
        {
            foreach (var category in Categories.Values)
            {
                if (category.HasSubcategories && category.Subcategories.TryGetValue(id, out var subcategory))
                {
                    Utils.LogSystem("CategoryLoader", "GetSubcategory", $"Found subcategory: {subcategory.Name}", "log", 4);
                    return subcategory;
                }
            }
            Utils.LogSystem("CategoryLoader", "GetSubcategory", $"Subcategory not found: {id}", "error", 4);
            return null;
        }

        /// <summary>
        /// Gets a subcategory by its name.
        /// </summary>
        /// <param name="name">The name of the subcategory to retrieve.</param>
        /// <returns>The subcategory with the given name, or null if it does not exist.</returns>
        /// <remarks>
        /// This method iterates over all the categories in the root and checks if each category has a subcategory with the given name.
        /// If a matching subcategory is found, it is returned and a log message is printed.
        /// If no matching subcategory is found, a log message is printed and null is returned.
        /// </remarks>
        public Subcategory GetSubcategoryByName(string name)
        {
            foreach (var category in Categories.Values)
            {
                if (category.HasSubcategories)
                {
                    foreach (var subcategory in category.Subcategories.Values)
                    {
                        if (subcategory.Name == name) { Utils.LogSystem("CategoryLoader", "GetSubcategoryByName", $"Found subcategory: {subcategory.Name}", "log", 4); return subcategory;}
                    }
                }
            }

            Utils.LogSystem("CategoryLoader", "GetSubcategoryByName", $"Subcategory not found: {name}", "error", 4);
            return null;
        }

        public List<Category> GetList() => new List<Category>(Categories.Values);
    }

    public class Category
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webhook")]
        public string Webhook { get; set; }

        [JsonProperty("required")]
        public Requirements Required { get; set; }

        [JsonProperty("subcategories")]
        public Dictionary<string, Subcategory> Subcategories { get; set; } = new Dictionary<string, Subcategory>();

        [JsonIgnore]
        public bool HasSubcategories => Subcategories != null && Subcategories.Count > 0;
    }

    public class Subcategory
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webhook")]
        public string Webhook { get; set; }

        [JsonProperty("required")]
        public Requirements Required { get; set; }
    }

    public class Requirements
    {
        [JsonProperty("pictures")]
        public bool Pictures { get; set; }

        [JsonProperty("subject")]
        public bool Subject { get; set; }

        [JsonProperty("description")]
        public bool Description { get; set; }
    }
}
