using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using AmdStockCheck.Models;
using Microsoft.Data.Sqlite;
using System.Linq;
using GenericUtil.Extensions;
using System.Threading;

namespace AmdStockCheck.DataAccess
{
    public class AmdDatabaseService
    {
        private readonly string _Source = "AmdStockDb";

        private readonly AmdStockCheckContext _DbContext;
        private readonly object _SyncLock;

        public AmdDatabaseService(IServiceProvider services)
        {
            _DbContext = services.GetService<AmdStockCheckContext>();
            _SyncLock = new object();
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Initialized!"));
        }

        #region Product
        #region Add
        public bool AddNewProduct(Product product)
        {
            bool ret = false;
            lock(_SyncLock)
            {
                try
                {
                    _DbContext.Products.Add(product);
                    _DbContext.SaveChanges();
                    ret = true;
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return ret;
        }
        #endregion

        #region Get
        public Product GetProductById(string productId)
        {
            Product product = null;
            lock(_SyncLock)
            {
                try
                {
                    product = _DbContext.Products.FirstOrDefault(x => x.ProductId == productId);
                    LoadProductDependencies(product);
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return product;
        }

        public Product GetProductByCheckUrl(string checkUrl)
        {
            Product product = null;
            lock (_SyncLock)
            {
                try
                {
                    product = _DbContext.Products.FirstOrDefault(x => x.CheckUrl == checkUrl);
                    LoadProductDependencies(product);
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return product;
        }

        public List<Product> GetAllRegisteredProducts()
        {
            List<Product> products = new List<Product>();
            lock (_SyncLock)
            {
                try
                {
                    products = _DbContext.Products.ToList();
                    if (products != null)
                    {
                        for (int i = 0; i < products.Count; i++)
                        {
                            LoadProductDependencies(products[i]);
                        }
                    }
                    return products;
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return products;
        }
        #endregion

        #region Remove
        public bool RemoveProduct(Product product)
        {
            bool ret = false;
            lock (_SyncLock)
            {
                try
                {
                    _DbContext.Products.Remove(product);
                    _DbContext.SaveChanges();
                    ret = true;
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return ret;
        }
        #endregion
        #endregion

        #region User
        #region Add
        public bool AddUserToProduct(Product product, User userToAdd)
        {
            bool ret = false;
            lock (_SyncLock)
            {
                try
                {
                    Product entry = _DbContext.Products.FirstOrDefault(x => x.ProductId == product.ProductId);
                    User user = _DbContext.Users.FirstOrDefault(x => x.UserId == userToAdd.UserId);

                    if (entry != null)
                    {
                        if (entry.Users == null)
                        {
                            entry.Users = new List<User>();
                        }

                        if (user == null)
                        {
                            _DbContext.Users.Add(userToAdd);
                        }

                        entry.Users.Add(userToAdd);
                        _DbContext.Products.Update(entry);
                    }
                    _DbContext.SaveChanges();
                    ret = true;
                }
                catch (Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
                }
            }
            return ret;
        }
        #endregion

        #region Remove
        public bool RemoveUser(Product product, User user)
        {
            bool ret = false;
            try
            {
                product.Users.Remove(user);
                _DbContext.Products.Update(product);

                if(CheckIfUserHasProduct(user))
                {
                    _DbContext.Users.Remove(user);
                }
                _DbContext.SaveChanges();

                ret = true;
            }
            catch (Exception e)
            {
                _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
            }
            return ret;
        }
        #endregion
        #endregion

        #region Internal Helpers
        private void LoadProductDependencies(Product product)
        {
            if (product != null
                && product.Users == null)
            {
                product.Users = _DbContext.Users.AsQueryable().Where(x => x.ProductId == product.Id).ToList();
            }
        }

        private bool CheckIfUserHasProduct(User user)
        {
            bool ret = false;
            try
            {
                ret = _DbContext.Products.FirstOrDefault(x => x.Users.FirstOrDefault(u => u.UserId == user.UserId) != null) != null;
            }
            catch (Exception e)
            {
                _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
            }
            return ret;
        }
        #endregion
    }
}
